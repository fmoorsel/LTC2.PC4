#nullable enable
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Unofficial C# client for Funda's internal mobile API.
/// Based on pyfunda. Not affiliated with or endorsed by Funda.
/// Usage may violate Funda's Terms of Service.
/// </summary>
public class FundaClient : IDisposable
{
    // ── Endpoints ──────────────────────────────────────────────────────────────
    private const string ApiBase = "https://listing-detail-page.funda.io/api/v4/listing/object/nl";
    private const string ApiSearch = "https://listing-search-wonen.funda.io/_msearch/template";
    private const string ApiWalter = "https://api.walterliving.com/hunter/lookup";
    private const string SearchTemplateId = "search_result_20250805";
    private const string SearchIndex = "listings-wonen-searcher-alias-prod";
    private const string TestUrl = ApiBase + "/tinyId/43117443";

    private static readonly int[] ValidRadii = { 1, 2, 5, 10, 15, 30, 50 };
    private static readonly Regex TinyIdPattern = new(@"/(\d{7,9})/?(?:\?|$|#)", RegexOptions.Compiled);

    // Construction periods matching Python's period_boundaries exactly
    private static readonly (string Period, int YearMin, int YearMax)[] ConstructionPeriods =
    {
        ("before_1906",        0,    1905),
        ("from_1906_to_1930",  1906, 1930),
        ("from_1931_to_1944",  1931, 1944),
        ("from_1945_to_1959",  1945, 1959),
        ("from_1960_to_1970",  1960, 1970),
        ("from_1971_to_1980",  1971, 1980),
        ("from_1981_to_1990",  1981, 1990),
        ("from_1991_to_2000",  1991, 2000),
        ("from_2001_to_2010",  2001, 2010),
        ("from_2011_to_2020",  2011, 2020),
        ("after_2020",         2021, 9999),
    };

    private readonly int _timeoutSeconds;
    private readonly ILogger<FundaClient>? _logger;
    private HttpClient? _client;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    public FundaClient(int timeoutSeconds = 30, ILogger<FundaClient>? logger = null)
    {
        _timeoutSeconds = timeoutSeconds;
        _logger = logger;
    }

    // ── Headers ────────────────────────────────────────────────────────────────
    // Mirrors Python's _make_headers_dict: same keys, same insertion order,
    // randomised Datadog trace IDs per request.

    private static List<(string Name, string Value)> BuildHeaders(bool forSearch = false)
    {
        // trace_id = str(random.randint(10**18, 10**19))  — clamped to long.MaxValue
        var traceId = Random.Shared.NextInt64(1_000_000_000_000_000_000L, long.MaxValue).ToString();
        // parent_id = hex(random.randint(10**15, 10**16))[2:]
        var parentId = Random.Shared.NextInt64(1_000_000_000_000_000L, 10_000_000_000_000_000L).ToString("x");
        // tid = hex(int(time.time()))[2:] + "00000000"
        var tid = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString("x") + "00000000";

        var headers = new List<(string, string)>
        {
            ("user-agent",                  "Dart/3.9 (dart:io)"),
            ("x-datadog-sampling-priority", "0"),
            ("x-datadog-origin",            "rum"),
            ("tracestate",                  $"dd=s:0;o:rum;p:{parentId}"),
            ("accept-encoding",             "gzip"),
            ("x-datadog-parent-id",         traceId),
        };

        if (forSearch)
        {
            headers.Add(("content-type", "application/json"));
            headers.Add(("referer", "https://www.funda.nl/"));
            headers.Add(("accept", "application/json"));
        }
        else
        {
            headers.Add(("x-funda-app-platform", "android"));
            headers.Add(("content-type", "application/json"));
        }

        // traceparent is always last — matches Python exactly
        var traceParentId = traceId.Length >= 16 ? traceId[..16] : traceId.PadLeft(16, '0');
        headers.Add(("traceparent", $"00-{tid}{traceParentId}-{parentId}-00"));

        return headers;
    }

    private static void ApplyHeaders(HttpRequestMessage request, List<(string Name, string Value)> headers)
    {
        foreach (var (name, value) in headers)
        {
            // content-type and accept are set on HttpContent / Accept header collection
            if (name is "content-type" or "accept") continue;
            request.Headers.TryAddWithoutValidation(name, value);
        }
    }

    // ── Fingerprint pool ───────────────────────────────────────────────────────
    // Python uses tls_client (Go-based JA3 spoof) + curl_cffi impersonate.
    // In .NET on Windows we approximate with:
    //   1. WinHttpHandler  — Windows SChannel TLS (different JA3 from managed .NET)
    //   2. SocketsHttpHandler — .NET managed TLS fallback
    // Both are tested against the known test URL in order, first success wins.

    private enum HandlerType { WinHttp, Sockets }

    private HttpClient BuildClient(HandlerType type)
    {
        HttpMessageHandler handler;
        if (type == HandlerType.WinHttp && OperatingSystem.IsWindows())
        {
            handler = new WinHttpHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                SendTimeout = TimeSpan.FromSeconds(_timeoutSeconds),
                ReceiveDataTimeout = TimeSpan.FromSeconds(_timeoutSeconds),
                ReceiveHeadersTimeout = TimeSpan.FromSeconds(_timeoutSeconds),
            };
        }
        else
        {
            handler = new SocketsHttpHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                ConnectTimeout = TimeSpan.FromSeconds(_timeoutSeconds),
            };
        }
        return new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(_timeoutSeconds + 5) };
    }

    private async Task<bool> TestClientAsync(HttpClient client)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var request = new HttpRequestMessage(HttpMethod.Get, TestUrl);
            ApplyHeaders(request, BuildHeaders(forSearch: false));
            using var response = await client.SendAsync(request, cts.Token);
            return response.StatusCode == HttpStatusCode.OK;
        }
        catch { return false; }
    }

    private async Task EnsureClientAsync(CancellationToken ct = default)
    {
        if (_client != null) return;

        await _initLock.WaitAsync(ct);
        try
        {
            if (_client != null) return;

            foreach (var type in new[] { HandlerType.WinHttp, HandlerType.Sockets })
            {
                _logger?.LogInformation("Funda: testing fingerprint {Type}", type);
                var candidate = BuildClient(type);
                if (await TestClientAsync(candidate))
                {
                    _logger?.LogInformation("Funda: fingerprint accepted {Type}", type);
                    _client = candidate;
                    return;
                }
                candidate.Dispose();
            }

            throw new InvalidOperationException(
                "No working TLS fingerprint found. Funda may have updated their bot detection.");
        }
        finally { _initLock.Release(); }
    }

    // ── HTTP helpers ───────────────────────────────────────────────────────────

    private async Task<HttpResponseMessage> GetAsync(string url, bool forSearch = false, CancellationToken ct = default)
    {
        await EnsureClientAsync(ct);
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        ApplyHeaders(request, BuildHeaders(forSearch));
        if (forSearch) request.Headers.TryAddWithoutValidation("accept", "application/json");
        return await _client!.SendAsync(request, ct);
    }

    private async Task<HttpResponseMessage> PostAsync(string url, string body, bool forSearch = false, CancellationToken ct = default)
    {
        await EnsureClientAsync(ct);
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        var contentType = forSearch ? "application/x-ndjson" : "application/json";
        ApplyHeaders(request, BuildHeaders(forSearch));
        request.Content = new StringContent(body, Encoding.UTF8, contentType);
        if (forSearch) request.Headers.TryAddWithoutValidation("accept", "application/json");
        return await _client!.SendAsync(request, ct);
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>Fetch a single listing by numeric ID or Funda URL.</summary>
    public async Task<FundaListing> GetListingAsync(string idOrUrl, CancellationToken ct = default)
    {
        var id = ExtractId(idOrUrl);
        var url = id.Length >= 8 ? $"{ApiBase}/tinyId/{id}" : $"{ApiBase}/{id}";

        using var response = await GetAsync(url, ct: ct);

        // If tinyId returns 404, retry as globalId (matches Python fallback)
        if (response.StatusCode == HttpStatusCode.NotFound && id.Length >= 8)
        {
            using var r2 = await GetAsync($"{ApiBase}/{id}", ct: ct);
            r2.EnsureSuccessStatusCode();
            var json2 = await r2.Content.ReadAsStringAsync(ct);
            using var doc2 = JsonDocument.Parse(json2);
            return ParseListingDetail(doc2.RootElement);
        }

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        return ParseListingDetail(doc.RootElement);
    }

    public Task<FundaListing> GetListingAsync(long id, CancellationToken ct = default)
        => GetListingAsync(id.ToString(), ct);

    /// <summary>
    /// Search for listings. Mirrors Python's search_listing parameters exactly,
    /// including availability, construction year periods, and retry-on-400 logic.
    /// </summary>
    public async Task<List<FundaListing>> SearchListingsAsync(
        string? location = null,
        string offeringType = "buy",
        string[]? availability = null,
        int? priceMin = null,
        int? priceMax = null,
        int? areaMin = null,
        int? areaMax = null,
        int? plotMin = null,
        int? plotMax = null,
        string[]? objectTypes = null,
        string[]? energyLabels = null,
        string[]? constructionType = null,
        int? constructionYearMin = null,
        int? constructionYearMax = null,
        int? radiusKm = 2,
        string? sort = null,
        int page = 0,
        CancellationToken ct = default)
    {
        // Map "sold" → "unavailable" (Python: avail_list = ["unavailable" if v == "sold" else v ...])
        var avail = (availability ?? new[] { "available", "negotiations" })
            .Select(a => a == "sold" ? "unavailable" : a)
            .ToArray();

        string[]? locations = location?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var @params = new Dictionary<string, object>
        {
            ["availability"] = avail,
            ["type"] = new[] { "single" },
            ["zoning"] = new[] { "residential" },
            ["object_type"] = (object)(objectTypes ?? new[] { "house", "apartment" }),
            ["publication_date"] = new { no_preference = true },
            ["offering_type"] = offeringType,
            ["page"] = new { from = page * 15 },
        };

        // Sort (matches Python sort_map, null fields for no-sort)
        @params["sort"] = sort switch
        {
            "newest" => (object)new { field = "publish_date_utc", order = "desc" },
            "oldest" => new { field = "publish_date_utc", order = "asc" },
            "price_asc" => new { field = "price.selling_price", order = "asc" },
            "price_desc" => new { field = "price.selling_price", order = "desc" },
            "area_asc" => new { field = "floor_area", order = "asc" },
            "area_desc" => new { field = "floor_area", order = "desc" },
            "plot_desc" => new { field = "plot_area", order = "desc" },
            "city" => new { field = "address.city", order = "asc" },
            "postcode" => new { field = "address.postal_code", order = "asc" },
            _ => new { field = (string?)null, order = (string?)null },
        };

        // Location: radius search (single postcode/city) or selected_area list
        if (locations?.Length == 1 && radiusKm.HasValue)
        {
            var actual = ValidRadii.MinBy(r => Math.Abs(r - radiusKm.Value));
            var locId = locations[0].ToLowerInvariant().Replace(" ", "-") + "-0";
            @params["radius_search"] = new
            {
                index = "geo-wonen-alias-prod",
                id = locId,
                path = $"area_with_radius.{actual}",
            };
        }
        else if (locations?.Length > 0)
        {
            @params["selected_area"] = locations;
        }

        // Price — nested under selling_price or rent_price key (Python: params["price"] = {price_key: ...})
        if (priceMin.HasValue || priceMax.HasValue)
        {
            var priceKey = offeringType == "buy" ? "selling_price" : "rent_price";
            var range = new Dictionary<string, int>();
            if (priceMin.HasValue) range["from"] = priceMin.Value;
            if (priceMax.HasValue) range["to"] = priceMax.Value;
            @params["price"] = new Dictionary<string, object> { [priceKey] = range };
        }

        // Living area
        if (areaMin.HasValue || areaMax.HasValue)
        {
            var range = new Dictionary<string, int>();
            if (areaMin.HasValue) range["from"] = areaMin.Value;
            if (areaMax.HasValue) range["to"] = areaMax.Value;
            @params["floor_area"] = range;
        }

        // Plot area
        if (plotMin.HasValue || plotMax.HasValue)
        {
            var range = new Dictionary<string, int>();
            if (plotMin.HasValue) range["from"] = plotMin.Value;
            if (plotMax.HasValue) range["to"] = plotMax.Value;
            @params["plot_area"] = range;
        }

        if (energyLabels?.Length > 0)
            @params["energy_label"] = energyLabels;

        if (constructionType?.Length > 0)
            @params["construction_type"] = constructionType;

        // Map construction years to Funda's predefined period strings (matches Python exactly)
        if (constructionYearMin.HasValue || constructionYearMax.HasValue)
        {
            var yMin = constructionYearMin ?? 0;
            var yMax = constructionYearMax ?? 9999;
            var periods = ConstructionPeriods
                .Where(p => p.YearMax >= yMin && p.YearMin <= yMax)
                .Select(p => p.Period)
                .ToArray();
            if (periods.Length > 0)
                @params["construction_period"] = periods;
        }

        // NDJSON body (Elasticsearch _msearch format)
        var body = new StringBuilder();
        body.AppendLine(JsonSerializer.Serialize(new { index = SearchIndex }));
        body.AppendLine(JsonSerializer.Serialize(new { id = SearchTemplateId, @params }));
        var bodyStr = body.ToString();

        // Retry up to 3× on 400 — matches Python: "Retry on intermittent 400 errors from API"
        HttpResponseMessage? response = null;
        for (int attempt = 0; attempt < 3; attempt++)
        {
            response?.Dispose();
            response = await PostAsync(ApiSearch, bodyStr, forSearch: true, ct: ct);
            if (response.StatusCode == HttpStatusCode.OK) break;
            if (response.StatusCode != HttpStatusCode.BadRequest || attempt == 2)
            {
                var err = await response.Content.ReadAsStringAsync(ct);
                response.Dispose();
                throw new InvalidOperationException($"Search failed ({(int)response.StatusCode}): {err}");
            }
            await Task.Delay(100 * (attempt + 1), ct);
        }

        using (response)
        {
            var json = await response!.Content.ReadAsStringAsync(ct);
            return ParseSearchResults(json);
        }
    }

    /// <summary>Returns the highest globalId currently in the search index.</summary>
    public async Task<long> GetLatestIdAsync(CancellationToken ct = default)
    {
        var results = await SearchListingsAsync(offeringType: "buy", sort: "newest", page: 0, ct: ct);
        if (results.Count == 0)
            throw new InvalidOperationException("No listings returned from latest-id search.");
        return results.Max(r => r.GlobalId ?? 0);
    }

    /// <summary>Async generator polling new listings by incrementing globalIds.</summary>
    public async IAsyncEnumerable<FundaListing> PollNewListingsAsync(
        long sinceId,
        int maxConsecutive404s = 20,
        string? offeringType = null,
        int delayMs = 200,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        long currentId = sinceId + 1;
        int misses = 0;

        while (!ct.IsCancellationRequested && misses < maxConsecutive404s)
        {
            HttpResponseMessage? response = null;
            FundaListing? listing = null;
            try
            {
                response = await GetAsync($"{ApiBase}/{currentId}", ct: ct);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    misses = 0;
                    var json = await response.Content.ReadAsStringAsync(ct);
                    using var doc = JsonDocument.Parse(json);
                    listing = ParseListingDetail(doc.RootElement);
                }
                else { misses++; }
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
            {
                await Task.Delay(5000, ct);
                continue;
            }
            catch { misses++; }
            finally { response?.Dispose(); }

            if (listing != null)
            {
                bool include = offeringType == null ||
                    string.Equals(listing.OfferingType, offeringType, StringComparison.OrdinalIgnoreCase);
                if (include) yield return listing;
            }

            currentId++;
            if (delayMs > 0) await Task.Delay(delayMs, ct);
        }
    }

    /// <summary>Fetch historical price data via Walter Living API.</summary>
    public async Task<List<PriceHistory>> GetPriceHistoryAsync(FundaListing listing, CancellationToken ct = default)
    {
        if (listing.Url == null || listing.Title == null || listing.Postcode == null)
            throw new ArgumentException("Listing must have Url, Title, and Postcode.");

        var payload = JsonSerializer.Serialize(new
        {
            url = listing.Url,
            address = listing.Title,
            zipcode = listing.Postcode,
        });

        using var response = await PostAsync(ApiWalter, payload, forSearch: false, ct: ct);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Could not fetch price history ({(int)response.StatusCode})");

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);

        if (doc.RootElement.TryGetProperty("status", out var statusEl) && statusEl.GetString() != "ok")
            throw new InvalidOperationException("Price history not available for this listing.");

        var list = new List<PriceHistory>();
        if (doc.RootElement.TryGetProperty("changes", out var changes))
        {
            foreach (var item in changes.EnumerateArray())
            {
                list.Add(new PriceHistory
                {
                    Price = item.TryGetLong("price"),
                    HumanPrice = item.TryGetStr("human_price"),
                    Date = item.TryGetStr("date"),
                    Timestamp = item.TryGetStr("timestamp"),
                    Source = item.TryGetStr("source"),
                    Status = TranslatePriceStatus(item.TryGetStr("status")),
                });
            }
        }
        return list;
    }

    // Dutch status → English (Python's status_map)
    private static string? TranslatePriceStatus(string? s) => s switch
    {
        "Vraagprijs" => "asking_price",
        "Verkocht" => "sold",
        "WOZ" => "woz",
        _ => s,
    };

    // ── Parsing: detail endpoint ───────────────────────────────────────────────

    private static FundaListing ParseListingDetail(JsonElement data)
    {
        var identifiers = data.Obj("Identifiers");
        var address = data.Obj("AddressDetails");
        var priceData = data.Obj("Price");
        var coords = data.Obj("Coordinates");
        var media = data.Obj("Media");
        var fastView = data.Obj("FastView");
        var ads = data.Obj("Advertising").Obj("TargetingOptions");
        var insights = data.Obj("ObjectInsights");

        var listing = new FundaListing
        {
            GlobalId = identifiers.TryGetLong("GlobalId"),
            TinyId = identifiers.TryGetLong("TinyId"),
            Title = address.TryGetStr("Title"),
            City = address.TryGetStr("City"),
            Postcode = address.TryGetStr("PostCode"),
            Province = address.TryGetStr("Province"),
            Neighbourhood = address.TryGetStr("NeighborhoodName"),
            HouseNumber = address.TryGetStr("HouseNumber"),
            HouseNumberExt = address.TryGetStr("HouseNumberExtension"),
            Municipality = ads.TryGetStr("gemeente"),
            Price = priceData.TryGetLong("NumericSellingPrice") ?? priceData.TryGetLong("NumericRentalPrice"),
            PriceFormatted = priceData.TryGetStr("SellingPrice") ?? priceData.TryGetStr("RentalPrice"),
            OfferingType = data.TryGetStr("OfferingType"),
            ObjectType = data.TryGetStr("ObjectType"),
            ConstructionType = data.TryGetStr("ConstructionType"),
            Status = data.TryGetBool("IsSoldOrRented") == true ? "sold" : "available",
            EnergyLabel = fastView.TryGetStr("EnergyLabel"),
            LivingAreaFormatted = fastView.TryGetStr("LivingArea"),
            PlotAreaFormatted = fastView.TryGetStr("PlotArea"),
            Description = data.Obj("ListingDescription").TryGetStr("Description"),
            Highlight = data.Obj("Promo").Obj("Blikvanger").TryGetStr("Text"),
            PublicationDate = data.TryGetStr("PublicationDate"),
            HasGarden = ads.TryGetStr("tuin") == "true",
            HasBalcony = ads.TryGetStr("balkon") == "true",
            HasSolarPanels = ads.TryGetStr("zonnepanelen") == "true",
            HasHeatPump = ads.TryGetStr("warmtepomp") == "true",
            HasRoofTerrace = ads.TryGetStr("dakterras") == "true",
            HasParkingOnSite = ads.TryGetStr("parkeergelegenheidopeigenterrein") == "true",
            HasParkingEnclosed = ads.TryGetStr("parkeergelegenheidopafgeslotenterrein") == "true",
            OpenHouse = ads.TryGetStr("openhuis") == "true",
            IsAuction = priceData.TryGetBool("IsAuction") ?? false,
            IsEnergyEfficient = ads.TryGetStr("energiezuinig") == "true",
            IsMonument = ads.TryGetStr("monumentalestatus") == "true",
            IsFixerUpper = ads.TryGetStr("kluswoning") == "true",
            HouseType = ads.TryGetStr("soortwoning"),
            GoogleMapsUrl = data.TryGetStr("GoogleMapsObjectUrl"),
            BrochureUrl = media.Obj("Brochure").TryGetStr("CdnUrl"),
            Views = insights.TryGetLong("Views"),
            Saves = insights.TryGetLong("Saves"),
        };

        // Living area: prefer raw int from ads ("woonoppervlakte"), fall back to formatted string
        var woon = ads.TryGetStr("woonoppervlakte");
        listing.LivingArea = woon != null && int.TryParse(woon, out int la)
            ? la : ParseAreaString(listing.LivingAreaFormatted);

        var perceel = ads.TryGetStr("perceeloppervlakte");
        listing.PlotArea = perceel != null && int.TryParse(perceel, out int pa)
            ? pa : ParseAreaString(listing.PlotAreaFormatted);

        listing.Bedrooms = fastView.TryGetInt("NumberOfBedrooms");

        var kamers = ads.TryGetStr("aantalkamers");
        listing.Rooms = kamers != null && int.TryParse(kamers, out int rooms) ? rooms : null;

        var bouwjaar = ads.TryGetStr("bouwjaar");
        listing.ConstructionYear = bouwjaar != null && int.TryParse(bouwjaar, out int yr) ? yr : null;

        // Coordinates (may be number or string in JSON)
        listing.Latitude = ParseCoord(coords, "Latitude");
        listing.Longitude = ParseCoord(coords, "Longitude");

        // Photos
        ParseMediaSection(media.Obj("Photos"), "MediaBaseUrl", "Items", "Id", null,
            listing.Photos, listing.PhotoUrls);

        // Floorplans (ThumbnailId for URL, Id for id)
        var fpSection = media.Obj("LegacyFloorPlan");
        var fpBase = fpSection.TryGetStr("ThumbnailBaseUrl")?.Replace("{id}", "{}");
        if (fpSection.TryGetProperty("Items", out var fpItems) && fpItems.ValueKind == JsonValueKind.Array)
        {
            foreach (var f in fpItems.EnumerateArray())
            {
                var fid = f.TryGetStr("Id");
                if (fid != null) listing.Floorplans.Add(fid);
                var tid = f.TryGetStr("ThumbnailId");
                if (tid != null && fpBase != null) listing.FloorplanUrls.Add(fpBase.Replace("{}", tid));
            }
        }

        // Videos
        ParseMediaSection(media.Obj("Videos"), "MediaBaseUrl", "Items", "Id", null,
            listing.Videos, listing.VideoUrls);

        // URL (constructed from city/title slugs, matches Python _parse_listing)
        var citySlug = (listing.City ?? "").ToLowerInvariant().Replace(" ", "-");
        var titleSlug = (listing.Title ?? "").ToLowerInvariant().Replace(" ", "-");
        var offering = data.TryGetStr("OfferingType") == "Sale" ? "koop" : "huur";
        listing.Url = $"https://www.funda.nl/detail/{offering}/{citySlug}/{titleSlug}/{listing.TinyId}/";

        // Characteristics (KenmerkSections)
        if (data.TryGetProperty("KenmerkSections", out var sections) && sections.ValueKind == JsonValueKind.Array)
        {
            foreach (var section in sections.EnumerateArray())
            {
                if (!section.TryGetProperty("KenmerkenList", out var items)) continue;
                foreach (var item in items.EnumerateArray())
                {
                    var label = item.TryGetStr("Label");
                    var value = item.TryGetStr("Value");
                    if (label != null && value != null)
                        listing.Characteristics[label] = value;
                }
            }
        }
        listing.OfferedSince = listing.Characteristics.GetValueOrDefault("Aangeboden sinds");
        listing.Acceptance = listing.Characteristics.GetValueOrDefault("Aanvaarding");
        listing.PricePerM2 = listing.Characteristics.GetValueOrDefault("Vraagprijs per m²");

        // Broker
        if (data.TryGetProperty("Tracking", out var tracking) &&
            tracking.TryGetProperty("Values", out var tv) &&
            tv.TryGetProperty("brokers", out var brokers) &&
            brokers.ValueKind == JsonValueKind.Array &&
            brokers.GetArrayLength() > 0)
        {
            var broker = brokers[0];
            listing.BrokerId = broker.TryGetStr("broker_id");
            listing.BrokerAssociation = broker.TryGetStr("broker_association");
        }

        return listing;
    }

    // ── Parsing: search endpoint ───────────────────────────────────────────────

    private static List<FundaListing> ParseSearchResults(string json)
    {
        var results = new List<FundaListing>();
        using var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("responses", out var responses) ||
            responses.GetArrayLength() == 0)
            return results;

        var first = responses[0];
        if (!first.TryGetProperty("hits", out var outer)) return results;
        if (!outer.TryGetProperty("hits", out var hits)) return results;

        foreach (var hit in hits.EnumerateArray())
        {
            if (!hit.TryGetProperty("_source", out var src)) continue;

            var addrEl = src.Obj("address");
            var priceEl = src.Obj("price");

            // price arrays (selling_price / rent_price hold arrays of values)
            long? price = FirstLong(priceEl, "selling_price") ?? FirstLong(priceEl, "rent_price");
            string? priceCondition = FirstStr(priceEl, "selling_price_condition")
                                  ?? FirstStr(priceEl, "rent_price_condition");

            string? offeringType = null;
            if (src.TryGetProperty("offering_type", out var ot) &&
                ot.ValueKind == JsonValueKind.Array && ot.GetArrayLength() > 0)
                offeringType = ot[0].GetString();

            JsonElement? agentEl = null;
            if (src.TryGetProperty("agent", out var agArr) &&
                agArr.ValueKind == JsonValueKind.Array && agArr.GetArrayLength() > 0)
                agentEl = agArr[0];

            var street = addrEl.TryGetStr("street_name") ?? "";
            var hnum = addrEl.TryGetStr("house_number") ?? "";
            var title = $"{street} {hnum}".Trim();

            var listing = new FundaListing
            {
                GlobalId = hit.TryGetLong("_id"),
                Title = title,
                HouseNumber = hnum,
                HouseNumberExt = addrEl.TryGetStr("house_number_suffix"),
                City = addrEl.TryGetStr("city"),
                Postcode = addrEl.TryGetStr("postal_code"),
                Province = addrEl.TryGetStr("province"),
                Neighbourhood = addrEl.TryGetStr("neighbourhood"),
                Price = price,
                PriceFormatted = priceCondition,
                LivingArea = FirstInt(src, "floor_area"),
                PlotArea = PlotAreaFromRange(src),
                Bedrooms = src.TryGetInt("number_of_bedrooms"),
                Rooms = src.TryGetInt("number_of_rooms"),
                EnergyLabel = src.TryGetStr("energy_label"),
                ObjectType = src.TryGetStr("object_type"),
                OfferingType = offeringType,
                ConstructionType = src.TryGetStr("construction_type"),
                PublicationDate = src.TryGetStr("publish_date"),
                Url = src.TryGetStr("object_detail_page_relative_url"),
                BrokerId = agentEl?.TryGetStr("id"),
                BrokerAssociation = agentEl?.TryGetStr("association"),
            };

            // First 5 thumbnail IDs (matches Python [:5])
            if (src.TryGetProperty("thumbnail_id", out var thumbs) && thumbs.ValueKind == JsonValueKind.Array)
            {
                foreach (var t in thumbs.EnumerateArray().Take(5))
                {
                    var tid = t.ValueKind == JsonValueKind.String ? t.GetString() : null;
                    if (tid != null) listing.Photos.Add(tid);
                }
            }

            results.Add(listing);
        }
        return results;
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private static string ExtractId(string idOrUrl)
    {
        if (!idOrUrl.Contains('/')) return idOrUrl.Trim();
        var match = TinyIdPattern.Match(idOrUrl);
        if (match.Success) return match.Groups[1].Value;
        throw new ArgumentException($"Cannot extract listing ID from: {idOrUrl}");
    }

    // Parse "200 m²" or "2.960 m²" → int (Dutch locale: '.' is thousands separator)
    private static int? ParseAreaString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var cleaned = value.Replace(" m²", "").Replace(".", "");
        return int.TryParse(cleaned, out int v) ? v : null;
    }

    private static double? ParseCoord(JsonElement el, string key)
    {
        if (!el.TryGetProperty(key, out var v)) return null;
        if (v.ValueKind == JsonValueKind.Number) return v.GetDouble();
        if (v.ValueKind == JsonValueKind.String &&
            double.TryParse(v.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
            return d;
        return null;
    }

    private static void ParseMediaSection(
        JsonElement section, string baseKey, string itemsKey,
        string idProp, string? urlProp,
        List<string> ids, List<string> urls)
    {
        var baseUrl = section.TryGetStr(baseKey)?.Replace("{id}", "{}");
        if (!section.TryGetProperty(itemsKey, out var items) || items.ValueKind != JsonValueKind.Array) return;
        foreach (var item in items.EnumerateArray())
        {
            var id = item.TryGetStr(idProp);
            if (id == null) continue;
            ids.Add(id);
            var url = urlProp != null ? item.TryGetStr(urlProp) : null;
            if (url != null) urls.Add(url);
            else if (baseUrl != null) urls.Add(baseUrl.Replace("{}", id));
        }
    }

    // Helpers for reading first element of ES array fields
    private static long? FirstLong(JsonElement el, string key)
    {
        if (!el.TryGetProperty(key, out var arr)) return null;
        if (arr.ValueKind == JsonValueKind.Array && arr.GetArrayLength() > 0)
        {
            var first = arr[0];
            if (first.ValueKind == JsonValueKind.Number && first.TryGetInt64(out var n)) return n;
        }
        if (arr.ValueKind == JsonValueKind.Number && arr.TryGetInt64(out var n2)) return n2;
        return null;
    }

    private static string? FirstStr(JsonElement el, string key)
    {
        if (!el.TryGetProperty(key, out var v)) return null;
        if (v.ValueKind == JsonValueKind.String) return v.GetString();
        return null;
    }

    private static int? FirstInt(JsonElement el, string key)
    {
        if (!el.TryGetProperty(key, out var arr)) return null;
        if (arr.ValueKind == JsonValueKind.Array && arr.GetArrayLength() > 0)
        {
            var first = arr[0];
            if (first.ValueKind == JsonValueKind.Number && first.TryGetInt32(out var n)) return n;
        }
        if (arr.ValueKind == JsonValueKind.Number && arr.TryGetInt32(out var n2)) return n2;
        return null;
    }

    private static int? PlotAreaFromRange(JsonElement src)
    {
        if (!src.TryGetProperty("plot_area_range", out var r) || r.ValueKind != JsonValueKind.Object) return null;
        if (r.TryGetProperty("gte", out var gte) && gte.ValueKind == JsonValueKind.Number)
            return gte.GetInt32();
        return null;
    }

    public void Dispose()
    {
        _client?.Dispose();
        _initLock.Dispose();
    }
}


// ── FundaListing ──────────────────────────────────────────────────────────────

public class FundaListing
{
    public long? GlobalId { get; set; }
    public long? TinyId { get; set; }
    public string? Title { get; set; }
    public string? Address => Title;
    public string? City { get; set; }
    public string? Postcode { get; set; }
    public string? Province { get; set; }
    public string? Neighbourhood { get; set; }
    public string? HouseNumber { get; set; }
    public string? HouseNumberExt { get; set; }
    public string? Municipality { get; set; }
    public long? Price { get; set; }
    public string? PriceFormatted { get; set; }
    public string? OfferingType { get; set; }
    public string? ObjectType { get; set; }
    public string? ConstructionType { get; set; }
    public string? Status { get; set; }
    public string? EnergyLabel { get; set; }
    public int? LivingArea { get; set; }
    public string? LivingAreaFormatted { get; set; }
    public int? PlotArea { get; set; }
    public string? PlotAreaFormatted { get; set; }
    public int? Bedrooms { get; set; }
    public int? Rooms { get; set; }
    public int? ConstructionYear { get; set; }
    public string? Description { get; set; }
    public string? Highlight { get; set; }
    public string? PublicationDate { get; set; }
    public bool HasGarden { get; set; }
    public bool HasBalcony { get; set; }
    public bool HasSolarPanels { get; set; }
    public bool HasHeatPump { get; set; }
    public bool HasRoofTerrace { get; set; }
    public bool HasParkingOnSite { get; set; }
    public bool HasParkingEnclosed { get; set; }
    public bool OpenHouse { get; set; }
    public bool IsAuction { get; set; }
    public bool IsEnergyEfficient { get; set; }
    public bool IsMonument { get; set; }
    public bool IsFixerUpper { get; set; }
    public string? HouseType { get; set; }
    public string? GoogleMapsUrl { get; set; }
    public string? Url { get; set; }
    public string? ShareUrl => Url;
    public string? BrochureUrl { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public List<string> Photos { get; set; } = new();
    public List<string> PhotoUrls { get; set; } = new();
    public int PhotoCount => Photos.Count;
    public List<string> Floorplans { get; set; } = new();
    public List<string> FloorplanUrls { get; set; } = new();
    public List<string> Videos { get; set; } = new();
    public List<string> VideoUrls { get; set; } = new();
    public string? BrokerId { get; set; }
    public string? BrokerAssociation { get; set; }
    public long? Views { get; set; }
    public long? Saves { get; set; }
    public Dictionary<string, string> Characteristics { get; set; } = new();
    public string? OfferedSince { get; set; }
    public string? Acceptance { get; set; }
    public string? PricePerM2 { get; set; }

    public string Summary()
        => $"{Title}, {City} {Postcode} | {OfferingType} | €{Price:N0} | {LivingArea}m²";

    public override string ToString() => Summary();
}


// ── PriceHistory ──────────────────────────────────────────────────────────────

public class PriceHistory
{
    public long? Price { get; init; }
    public string? HumanPrice { get; init; }
    public string? Date { get; init; }
    public string? Timestamp { get; init; }
    public string? Source { get; init; }
    public string? Status { get; init; }
    public override string ToString() => $"{Date}  {HumanPrice}  ({Status})";
}


// ── JsonElement extension helpers ─────────────────────────────────────────────

internal static class JsonEx
{
    // Safely navigate into a nested object; returns default(JsonElement) on miss.
    public static JsonElement Obj(this JsonElement el, string key)
    {
        if (el.ValueKind == JsonValueKind.Object &&
            (el.TryGetProperty(key, out var v) ||
             el.TryGetProperty(key.ToLowerInvariant(), out v)) &&
            v.ValueKind == JsonValueKind.Object)
            return v;
        return default;
    }

    public static string? TryGetStr(this JsonElement el, string key)
    {
        if (el.ValueKind != JsonValueKind.Object) return null;
        return el.TryGetProperty(key, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString() : null;
    }

    public static long? TryGetLong(this JsonElement el, string key)
    {
        if (el.ValueKind != JsonValueKind.Object) return null;
        if (!el.TryGetProperty(key, out var v)) return null;
        if (v.ValueKind == JsonValueKind.Number && v.TryGetInt64(out var n)) return n;
        if (v.ValueKind == JsonValueKind.String && long.TryParse(v.GetString(), out var ns)) return ns;
        return null;
    }

    public static int? TryGetInt(this JsonElement el, string key)
    {
        if (el.ValueKind != JsonValueKind.Object) return null;
        if (!el.TryGetProperty(key, out var v)) return null;
        if (v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out var n)) return n;
        if (v.ValueKind == JsonValueKind.String && int.TryParse(v.GetString(), out var ns)) return ns;
        return null;
    }

    public static bool? TryGetBool(this JsonElement el, string key)
    {
        if (el.ValueKind != JsonValueKind.Object) return null;
        if (!el.TryGetProperty(key, out var v)) return null;
        if (v.ValueKind == JsonValueKind.True) return true;
        if (v.ValueKind == JsonValueKind.False) return false;
        return null;
    }
}

using LTC2.Shared.Models.Mapdefinitions;
using LTC2.Shared.Models.Settings;
using LTC2.Shared.Repositories.Interfaces;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Union;
using Newtonsoft.Json;
using ProjNet.CoordinateSystems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LTC2.Shared.Repositories.Mapdefinitions
{
    public class PlacesRepository : IPlacesRepository
    {

        private readonly GenericSettings _genericSettings;
        private readonly NtsGeometryServices _factory;

        private List<PlaceDefination> _places;
        private List<NeighbourDefinition> _neighbourDefinitions;

        public PlacesRepository(GenericSettings genericSettings)
        {
            var wgs84 = GeographicCoordinateSystem.WGS84 as CoordinateSystem;
            _factory = new NtsGeometryServices(new PrecisionModel(), Convert.ToInt32(wgs84.AuthorityCode));

            _genericSettings = genericSettings;
        }

        public List<PlaceDefination> GetPlaces()
        {
            if (_places == null)
            {
                _places = LoadPlacesFromJson();
            }

            return _places;
        }

        public List<NeighbourDefinition> GetNeighbourDefinitions()
        {
            if (_neighbourDefinitions == null)
            {
                _neighbourDefinitions = LoadNeigboursFromJson();
            }

            return _neighbourDefinitions;
        }

        public string GetHitAreaForPlaceAsWkt(PlaceDefination place, List<PlaceDefination> places, List<NeighbourDefinition> neighbourDefinitions)
        {
            var polygon = GetHitAreaForPlace(place, places, neighbourDefinitions);
            var wktWriter = new WKTWriter();

            return wktWriter.Write(polygon);
        }

        public void FillHitAreaFieldsForPlace(PlaceDefination place, List<PlaceDefination> places, List<NeighbourDefinition> neighbourDefinitions)
        {
            var hitArea = GetHitAreaForPlace(place, places, neighbourDefinitions);

            var wktWriter = new WKTWriter();
            var hitAreaAsWkt = wktWriter.Write(hitArea);

            place.HitArea = hitArea;
            place.HitAreaAsWkt = hitAreaAsWkt;
        }


        public Polygon GetHitAreaForPlace(PlaceDefination place, List<PlaceDefination> places, List<NeighbourDefinition> neighbourDefinitions)
        {
            var polyFactory = _factory.CreateGeometryFactory();
            var neighbourDefinition = neighbourDefinitions.FirstOrDefault(p => p.Place.ID == place.ID);

            if (neighbourDefinitions == null)
            {
                return place.BorderPolygons[0][0];
            }
            else
            {
                var polygonList = new List<Geometry>();

                foreach (var border in place.BorderPolygons)
                {
                    polygonList.Add(border[0]);
                }

                foreach (var neighbour in neighbourDefinition.Neighbours)
                {
                    var neighbourPlace = places.FirstOrDefault(p => p.ID == neighbour.ID);

                    if (neighbourPlace != null)
                    {
                        foreach (var border in neighbourPlace.BorderPolygons)
                        {
                            polygonList.Add(border[0]);
                        }
                    }
                }

                var cascadedPolygonUnion = new CascadedPolygonUnion(polygonList);
                var polygon = (cascadedPolygonUnion.Union() as Polygon);

                if (polygon == null)
                {
                    var alternativeCascadedPolygonUnion = new CascadedPolygonUnion(polygonList);
                    polygon = alternativeCascadedPolygonUnion.Union().Envelope as Polygon;
                }
                else
                {
                    polygon = polyFactory.CreatePolygon(polygon.Shell.Coordinates);
                }

                return polygon;
            }
        }

        private List<PlaceDefination> LoadPlacesFromJson()
        {
            var result = new List<PlaceDefination>();

            var placesFile = Path.Combine(_genericSettings.ResourceFolder, _genericSettings.PlacesFile);

            var json = File.ReadAllText(placesFile);

            var places = JsonConvert.DeserializeObject<List<PlaceDefination>>(json);

            foreach (var place in places)
            {
                CreatePolygons(place);

                result.Add(place);
            }

            return result;
        }

        private void CreatePolygons(PlaceDefination place)
        {
            var descriptions = place.Borders;
            var wktReader = new WKTReader(_factory);
            var wktWriter = new WKTWriter();
            var polyFactory = _factory.CreateGeometryFactory();

            foreach (var description in descriptions)
            {
                description.Border = wktReader.Read(description.BorderAsString) as Polygon;

                foreach (var exclude in description.ExcludesAsString)
                {
                    description.Excludes.Add(wktReader.Read(exclude) as Polygon);
                }

                var shell = description.Border.Shell;

                var holes = description.Excludes.Select(x => (x as Polygon).Shell).ToArray();
                var standardizedPolygon = polyFactory.CreatePolygon(shell, holes);

                description.Wkt = wktWriter.Write(standardizedPolygon);
                description.StandardizedizedPolygon = standardizedPolygon;

                description.ExcludesAsString.Clear();
            }

            if (place.HitAreaAsWkt != null)
            {
                place.HitArea = wktReader.Read(place.HitAreaAsWkt) as Polygon;
            }
        }

        private List<NeighbourDefinition> LoadNeigboursFromJson()
        {
            var placesFile = Path.Combine(_genericSettings.ResourceFolder, _genericSettings.NeighboursFile);
            var json = File.ReadAllText(placesFile);

            var neighbourDefinitions = JsonConvert.DeserializeObject<List<NeighbourDefinition>>(json);

            return neighbourDefinitions;
        }

        public List<PlaceDefination> GetPlacesFromCache()
        {
            try
            {
                var cacheFolder = Path.Combine($"{_genericSettings.CacheFolder}", "Places");
                var cacheFile = Path.Combine(cacheFolder, "places.json");

                if (File.Exists(cacheFile))
                {
                    var json = File.ReadAllText(cacheFile);

                    var places = JsonConvert.DeserializeObject<List<PlaceDefination>>(json);

                    foreach (var place in places)
                    {
                        CreatePolygons(place);
                    }

                    return places;
                }
            }
            catch (Exception)
            {

            }

            return new List<PlaceDefination>();
        }

        public void CachePlaces(List<PlaceDefination> places)
        {
            var cacheFolder = Path.Combine($"{_genericSettings.CacheFolder}", "Places");

            if (!Directory.Exists(cacheFolder))
            {
                Directory.CreateDirectory(cacheFolder);
            }

            var json = JsonConvert.SerializeObject(places);
            var cacheFile = Path.Combine(cacheFolder, "places.json");

            File.WriteAllText(cacheFile, json);
        }
    }
}

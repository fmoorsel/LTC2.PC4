using Elasticsearch.Net;
using LTC2.Shared.Models.Settings;
using LTC2.Shared.Repositories.Interfaces;
using Nest;
using Nest.JsonNetSerializer;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace LTC2.Shared.Repositories.Clients
{
    public class ElasticSearchClient : IElasticSearchClient
    {
        private ElasticClient _client;

        private readonly ConnectionSettings _settings;
        private readonly ElasticSearchConfiguration _configuration;

        public ElasticClient ElasticClient
        {
            get
            {
                return _client;
            }
        }

        public ElasticSearchClient(ElasticSearchConfiguration config)
        {
            _configuration = config;
            var node = new SingleNodeConnectionPool(new Uri(config.Uri));
            _settings = new ConnectionSettings(node, (builtin, settings) => new JsonNetSerializer(builtin, settings, GetSettings));
            _settings.ThrowExceptions();

            _settings.DisableDirectStreaming();

            _client = new ElasticClient(_settings);
        }

        private JsonSerializerSettings GetSettings()
        {
            var JsonSettings = new JsonSerializerSettings();
            JsonSettings.Converters.Add(new StringEnumConverter());

            return JsonSettings;
        }

        public T GetDocument<T>(string index, string identifier) where T : class
        {
            var response = _client.Get<T>(identifier, idx => idx.Index(index));
            return response.Source;
        }

        public string InsertDocument<T>(string index, T document) where T : class
        {
            var response = _client.Index(document, idx => idx.Index(index));
            return response.Id;
        }

        public void BulkInsertDocuments<T>(string index, IEnumerable<T> documents) where T : class
        {
            var waitHandle = new CountdownEvent(1);

            var bulkAllRq = _client.BulkAll(documents, x => x
                .Index(index)
                .BackOffRetries(2)
                .BackOffTime(new Time(new TimeSpan(0, 0, 100)))
                .MaxDegreeOfParallelism(1)
            );

            bulkAllRq.Subscribe(new BulkAllObserver(
                onNext: (n) => { },
                onCompleted: () => waitHandle.Signal(),
                onError: (e) => { throw e; }
            ));

            waitHandle.Wait();
        }

        public bool DeleteDocument<T>(string index, string identifier) where T : class
        {
            var response = _client.Delete<T>(identifier, idx => idx.Index(index));

            return true;
        }

        public bool DeleteQuery<T>(string index, Expression<Func<QueryContainerDescriptor<T>, QueryContainer>> searchCriteria) where T : class
        {
            var query = searchCriteria.Compile();

            var result = _client.DeleteByQuery<T>(del => del
                .Index(index)
                .Query(query)
            );

            return true;
        }

        public bool IndexExists(string index)
        {
            var result = _client.Indices.Exists(new IndexExistsRequest(index));

            return result.Exists;
        }

        public IEnumerable<string> GetCollectionOfIndices()
        {
            var response = _client.Indices.Get(new GetIndexRequest(Indices.All));

            return response.Indices.Select(index => index.Key.Name);
        }

        public bool DeleteIndex<T>(string index) where T : class
        {
            var result = _client.Indices.Delete(new DeleteIndexRequest(index));
            _client.Map<T>(null);
            return result.Acknowledged;
        }

        public bool DeleteIndex(string index)
        {
            var result = _client.Indices.Delete(new DeleteIndexRequest(index));

            return result.Acknowledged;
        }

        public bool CreateIndex<T>(string index, Func<IndexSettingsDescriptor, IPromise<IIndexSettings>> settings, Func<TypeMappingDescriptor<T>, ITypeMapping> mappings) where T : class
        {
            var result = _client.Indices.Create(index, c => c
                .Settings(settings)
                .Map(mappings)
            );

            return result.Acknowledged;
        }

        public ISearchResponse<T> SearchDocument<T>(string index, Func<QueryContainerDescriptor<T>, QueryContainer> searchCriteria,
            Expression<Func<AggregationContainerDescriptor<T>, IAggregationContainer>> aggregations, int? from = null, int resultSize = 10) where T : class
        {
            var searchFunc = searchCriteria;
            var aggregrationsFunc = aggregations.Compile();

            var searchResponse = _client.Search<T>(add => add
                .Index(index)
                .Query(searchFunc)
                .From(from)
                .Aggregations(aggregrationsFunc)
                .Size(resultSize)
            );

            return searchResponse;
        }

        public ISearchResponse<T> SearchDocument<T>(string index, Func<QueryContainerDescriptor<T>, QueryContainer> searchCriteria,
            Func<SortDescriptor<T>, IPromise<IList<ISort>>> sortFunc = null, int? from = null, int resultSize = 10) where T : class
        {
            var searchFunc = searchCriteria;

            ISearchRequest func(SearchDescriptor<T> add) => add
                .Index(index)
                .Sort(sortFunc)
                .Query(searchFunc)
                .From(from)
                .Size(resultSize);

            var searchResponse = _client.Search<T>(func);

            return searchResponse;
        }

        public ISearchResponse<T> SearchDocument<T>(string index, Func<QueryContainerDescriptor<T>, QueryContainer> searchCriteria, int resultSize = 10) where T : class
        {
            var searchFunc = searchCriteria;

            ISearchRequest func(SearchDescriptor<T> add) => add
                .Index(index)
                .Query(searchFunc)
                .Size(resultSize);

            var searchResponse = _client.Search<T>(func);

            return searchResponse;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ElasticSearchClient() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

}

using Nest;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LTC2.Shared.Repositories.Interfaces
{
    public interface IElasticSearchClient : IDisposable
    {
        public ElasticClient ElasticClient { get; }

        public ISearchResponse<T> SearchDocument<T>(string index, Func<QueryContainerDescriptor<T>, QueryContainer> searchCriteria, Func<SortDescriptor<T>, IPromise<IList<ISort>>> sortFunc = null, int? from = null, int resultSize = 10) where T : class;

        public ISearchResponse<T> SearchDocument<T>(string index, Func<QueryContainerDescriptor<T>, QueryContainer> searchCriteria, int resultSize = 10) where T : class;

        public ISearchResponse<T> SearchDocument<T>(string index, Func<QueryContainerDescriptor<T>, QueryContainer> searchCriteria, Expression<Func<AggregationContainerDescriptor<T>, IAggregationContainer>> aggregations, int? from = null, int resultSize = 10) where T : class;

        public T GetDocument<T>(string index, string identifier) where T : class;

        public string InsertDocument<T>(string index, T document) where T : class;

        public void BulkInsertDocuments<T>(string index, IEnumerable<T> documents) where T : class;

        public bool DeleteDocument<T>(string index, string identifier) where T : class;

        public bool DeleteQuery<T>(string index, Expression<Func<QueryContainerDescriptor<T>, QueryContainer>> searchCriteria) where T : class;

        public bool DeleteIndex<T>(string index) where T : class;

        public bool DeleteIndex(string index);

        public bool CreateIndex<T>(string index, Func<IndexSettingsDescriptor, IPromise<IIndexSettings>> settings, Func<TypeMappingDescriptor<T>, ITypeMapping> mappings) where T : class;

        public bool IndexExists(string index);

        public IEnumerable<string> GetCollectionOfIndices();
    }

}

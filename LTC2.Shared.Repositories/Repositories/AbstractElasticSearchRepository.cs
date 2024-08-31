using LTC2.Shared.Models.Dtos.Elastic;
using LTC2.Shared.Models.Settings;
using LTC2.Shared.Repositories.Clients;
using LTC2.Shared.Repositories.Interfaces;
using LTC2.Shared.Utils.Generic;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace LTC2.Shared.Repositories.Repositories
{
    public abstract class AbstractElasticSearchRepository : IBaseRepository
    {
        private readonly ElasticUploaderSettings _properties;
        private readonly ILogger<AbstractElasticSearchRepository> _logger;
        private readonly string _elasticUrl;
        private readonly string _indexManagementIndexName;
        private readonly int _maxHits = 50;

        protected IElasticSearchClient _elasticSearchClient;

        public AbstractElasticSearchRepository(ElasticUploaderSettings properties, ILogger<AbstractElasticSearchRepository> logger)
        {
            _properties = properties;
            _logger = logger;

            _elasticUrl = FileUtils.GetPathWithEndingString(properties.ElasticSearchUrl, "/");
            _indexManagementIndexName = properties.IndexManagementIndexName;
        }

        public virtual void Close()
        {
            _elasticSearchClient = null;
        }

        public virtual void Open()
        {
            _elasticSearchClient = new ElasticSearchClient(new ElasticSearchConfiguration()
            {
                Uri = new Uri(_properties.ElasticSearchUrl).GetLeftPart(UriPartial.Authority),
            });
        }

        public void UpdateManagementIndex(string name, string indexName, bool removeStale)
        {
            if (!_elasticSearchClient.IndexExists(_indexManagementIndexName))
            {
                CreateIndexManagementIndex();
            }

            var indexInformation = new IndexInformation()
            {
                Name = name,
                IndexName = indexName,
                CreationTime = DateTime.UtcNow,
                IsActive = true
            };

            InsertDocument<IndexInformation>(_indexManagementIndexName, indexInformation, true);

            Func<QueryContainerDescriptor<IndexInformation>, QueryContainer> queryContainer = (QueryContainerDescriptor<IndexInformation> exp) => exp.MatchAll();

            var records = SearchDocuments<IndexInformation>(_indexManagementIndexName, queryContainer, _maxHits);

            if (records.Hits.Any())
            {
                if (records.Total > _maxHits)
                {
                    var total = Convert.ToInt32(records.Total);
                    records = SearchDocuments<IndexInformation>(_indexManagementIndexName, queryContainer, total);
                }

                var indexInformationRecords = records.Hits.ToList().OrderBy(i => i.Source.Name).ThenByDescending(i => i.Source.CreationTime);
                var indexCountCurrentIndex = 0;
                var currentIndex = "";

                foreach (var indexInformationRecord in indexInformationRecords)
                {
                    if (indexInformationRecord.Source.Name != currentIndex)
                    {
                        indexCountCurrentIndex = 1;
                        currentIndex = indexInformationRecord.Source.Name;
                    }
                    else
                    {
                        indexCountCurrentIndex++;
                    }

                    if (indexCountCurrentIndex > 1)
                    {
                        DeleteDocument<IndexInformation>(_indexManagementIndexName, indexInformationRecord.Id, true);
                    }

                    if (indexCountCurrentIndex == 2)
                    {
                        indexInformationRecord.Source.IsActive = false;
                        InsertDocument<IndexInformation>(_indexManagementIndexName, indexInformationRecord.Source, true);
                    }

                    if (indexCountCurrentIndex > 2)
                    {
                        if (_elasticSearchClient.IndexExists(indexInformationRecord.Source.IndexName))
                        {
                            DeleteIndex<IndexInformation>(indexInformationRecord.Source.IndexName);
                        }

                    }
                }
            }
            else
            {
                _logger.LogWarning("Tried to update management index but upon read 0 records were returned, which should not happen....");
            }

            if (removeStale)
            {
                RemoveStaleIndices();
            }
        }

        public void RemoveStaleIndices()
        {
            Func<QueryContainerDescriptor<IndexInformation>, QueryContainer> queryContainer = (QueryContainerDescriptor<IndexInformation> exp) => exp.MatchAll();

            var records = SearchDocuments<IndexInformation>(_indexManagementIndexName, queryContainer, _maxHits);

            if (records.Hits.Any())
            {
                if (records.Total > _maxHits)
                {
                    var total = Convert.ToInt32(records.Total);
                    records = SearchDocuments<IndexInformation>(_indexManagementIndexName, queryContainer, total);
                }
            }

            var allIndices = _elasticSearchClient.GetCollectionOfIndices();

            foreach (var indexName in allIndices)
            {
                if (indexName != _indexManagementIndexName && records.Hits.FirstOrDefault(s => s.Source.IndexName == indexName) == null)
                {
                    _elasticSearchClient.ElasticClient.Indices.Delete(indexName);
                }
            }

        }

        protected void InsertDocument<T>(string index, T document, bool refresh = false) where T : class
        {
            _elasticSearchClient.InsertDocument<T>(index, document);

            if (refresh)
            {
                RefreshIndex(index);
            }
        }

        public void DeleteIndex<T>(string indexName) where T : class
        {
            if (_elasticSearchClient.IndexExists(indexName))
            {
                _elasticSearchClient.ElasticClient.Indices.Delete(indexName);

            }
        }

        public bool DeleteDocument<T>(string index, string identifier, bool refresh = false) where T : class
        {
            _elasticSearchClient.DeleteDocument<T>(index, identifier);

            if (refresh)
            {
                RefreshIndex(index);
            }

            return true;
        }

        protected void RefreshIndex(string index)
        {
            _elasticSearchClient.ElasticClient.Indices.Refresh(index);
        }

        protected ISearchResponse<T> SearchDocuments<T>(string index, Func<QueryContainerDescriptor<T>, QueryContainer> searchCriteria, int resultSize) where T : class
        {
            var searchResponse = _elasticSearchClient.ElasticClient.Search<T>(add => add
                                    .Index(index)
                                    .Query(searchCriteria)
                                    .Size(resultSize)
                                 );

            return searchResponse;
        }

        public void CreateIndex(string name, string indexName = null)
        {
            var indexNameToUse = indexName == null ? name : indexName;
            var indexDefinitionsFolder = FileUtils.GetFolderInEntryAssembly("IndexDefinitions");
            var indexMappingFileName = Path.Combine(indexDefinitionsFolder, $"{name}.json");
            var indexMapping = File.ReadAllText(indexMappingFileName);

            var uri = $"{_elasticUrl}{indexNameToUse}";

            using (var httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Put,
                    RequestUri = new Uri(uri),
                    Content = new StringContent(indexMapping, Encoding.UTF8, "application/json"),
                };

                var responseTask = httpClient.SendAsync(request);
                responseTask.Wait();

                var response = responseTask.Result;
                response.EnsureSuccessStatusCode();
            }
        }

        private void CreateIndexManagementIndex()
        {
            CreateIndex(_indexManagementIndexName);

            var indexSettingsFolder = FileUtils.GetFolderInEntryAssembly("IndexSettings");
            var indexSettingsFileName = Path.Combine(indexSettingsFolder, $"settings.json");
            var indexSettings = File.ReadAllText(indexSettingsFileName);

            var settingsUri = $"{_elasticUrl}_settings";
            using (var httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Put,
                    RequestUri = new Uri(settingsUri),
                    Content = new StringContent(indexSettings, Encoding.UTF8, "application/json"),
                };

                var responseTask = httpClient.SendAsync(request);
                responseTask.Wait();

                var response = responseTask.Result;
                response.EnsureSuccessStatusCode();
            }
        }
    }
}

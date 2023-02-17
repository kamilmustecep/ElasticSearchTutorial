using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchApi.BLL.BaseService
{
    public interface IGenericService<T> where T : class
    {
        Task<int> CreateIndex(T Model, string indexName);
        Task<bool> AddOrUpdateElasticIndex(T Model, string indexName);
        Task<bool> Insert(T Model, string indexName);
        Task<bool> Update(DocumentPath<T> ModelID, T Model, string indexName);
        Task<T> FirstOrDefault(string indexName, int id);
        Task<ISearchResponse<T>> DetailSearch(string indexName, SearchDescriptor<T> searchQuery, int skipItemCount, int maxItemCount, string[] includeFields, bool isHtml, string[] highField);
    }
}

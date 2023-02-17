using ElasticSearchApi.BLL.BaseEntity;
using ElasticSearchApi.BLL.DTO;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchApi.BLL.BaseService
{
    public class GenericService<T> : IGenericService<T> where T : ElasticEntity
    {
        public IElasticClient _client { get; set; }
        private ElasticClient GetClient()
        {
            var str = "http://localhost:9200/";
            var connectionString = new ConnectionSettings(new Uri(str))
                .DisablePing()
                .SniffOnStartup(false)
                .SniffOnConnectionFault(false);

            //connectionString.BasicAuthentication("Kullanıcı Adı", "Şifre");

            return new ElasticClient(connectionString);
        }



        public GenericService()
        {
            _client = GetClient();
        }




        public async Task<bool> AddOrUpdateElasticIndex(T Model, string indexName)
        {

            var ModelID = DocumentPath<T>.Id(new Id(Model));

            var exist = _client.DocumentExists(ModelID, t => t.Index(indexName));

            if (exist.Exists) //Kayıt varsa güncelle
            {
                return await Update(ModelID, Model, indexName);
            }
            else //Kayıt yoksa ekle
            {
                return await Insert(Model, indexName);
            }


        }

        [Obsolete]
        public async Task<int> CreateIndex(T Model, string indexName)
        {
            var exist = await _client.Indices.ExistsAsync(indexName);
            if (exist.Exists)
            {
                return 0;
            }
            else
            {
                var result = await _client.Indices.CreateAsync(indexName, t => t.Index(indexName)
                    .Settings(t => t.NumberOfShards(4).NumberOfReplicas(2)
                        .Setting("max_result_window", int.MaxValue)
                            .Analysis(t => t.TokenFilters(t => t.AsciiFolding("my_ascii_folding", af => af.PreserveOriginal(true)))
                                .Analyzers(a => a.Custom("turkish_analyzer", ca => ca
                                    .Filters("lowercase", "my_ascii_folding")
                                    .Tokenizer("standard")))))
                    .Mappings(t => t.Map<T>(map => map.AutoMap())));



                //var result = await _client.Indices.CreateAsync(indexName, 
                //    t => t.Index(indexName)
                //        .Settings(t => t.NumberOfShards(4).NumberOfReplicas(2)
                //        .Setting("max_result_window", int.MaxValue)));
                return 1;
            }
        }

        public async Task<T> FirstOrDefault(string indexName, int id)
        {

            var response = await _client.SearchAsync<T>(x => x.Index(indexName).Query(q => q.Term(t => t.id, id)));

            if (response.Documents.Count() > 0)
            {
                return response.Documents.First();
            }
            else
            {
                return null;
            }
        }


        public async Task<bool> DeleteSpecific(string indexName, int id,int pubid,string idFieldName,string pubIdFieldName)
        {

            //idFieldName alanı id'ye eşit ve küçük olup pubIdFieldName'si pubid olan tüm kayıtları silmek
            var response = await _client.DeleteByQueryAsync<object>(d => d
                                        .Index(indexName)
                                        .Query(q => q
                                            .Bool(b => b
                                                .Must(
                                                    m => m.Range(r => r.Field(idFieldName).LessThanOrEquals(id)),
                                                    m => m.Match(ma => ma.Field(pubIdFieldName).Query(pubid.ToString()))
                                                )
                                            )
                                        )
                                    );


            return true;
            
        }

        public async Task<bool> Insert(T Model, string indexName)
        {
            var result = await _client.IndexAsync(Model, t => t.Index(indexName));
            if (result.ServerError == null) return true;
            throw new Exception($"Kayıt eklenirken hata oluştu. Hatalı Index => {indexName} :" + result.ServerError.Error.Reason);
        }

        public async Task<bool> Update(DocumentPath<T> ModelID, T Model, string indexName)
        {
            var result = await _client.UpdateAsync(ModelID, t => t.Index(indexName).Doc(Model).RetryOnConflict(3));
            if (result.ServerError == null) return true;
            throw new Exception($"Kayıt güncellenirken hata oluştu. Hatalı Index => {indexName} :" + result.ServerError.Error.Reason);
        }

        public async Task<ISearchResponse<T>> DetailSearch(string indexName, SearchDescriptor<T> searchQuery, int skipItemCount, int maxItemCount, string[] includeFields, bool isHtml, string[] highField)
        {

            searchQuery.Index(indexName);

            if (isHtml)
            {
                string preTags = "<strong style=\"color: red;\">", postTags = "</strong>";
                var hfs = new List<Func<HighlightFieldDescriptor<T>, IHighlightField>>();

                foreach (var s in highField)
                {
                    hfs.Add(f => f.Field(s));
                }

                var highdes = new HighlightDescriptor<T>();
                highdes.PreTags(preTags).PostTags(postTags);
                highdes.Fields(hfs.ToArray());
                searchQuery.Highlight(t => highdes);
            }

            searchQuery.Skip(skipItemCount).Take(maxItemCount);


            if (includeFields != null)
            {
                searchQuery.Source(ss => ss.Includes(ff => ff.Fields(includeFields.ToArray())));
            }



        
            var response = await _client.SearchAsync<T>(searchQuery);

            return response;

        }



    }
}

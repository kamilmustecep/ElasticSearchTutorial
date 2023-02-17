using ElasticSearchApi.BLL.BaseService;
using ElasticSearchApi.BLL.DTO;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchApi.BLL.DataService
{
    public class NewsService : GenericService<NewsDTO>
    {
        public NewsService()
        {

        }

        public async Task<List<NewsDTO>> DetailSearchGetAsync(string indexName, string searchText, int skipItemCount = 0, int maxItemCount = 100)
        {
            var searchQuery = new SearchDescriptor<NewsDTO>();
            var searchArray = searchText.Split(' ');
            //BAŞLAYAN(*)/(*)BİTEN
            if (searchText.StartsWith("*") || searchText.EndsWith("*"))
            {
                searchQuery = searchQuery
                .Query(q => q
                    .QueryString(qs => qs
                    .Query(searchText)
                        .Fields(f => f
                            .Field(p => p.FullNews)
                            .Field(p => p.Spot)
                            .Field(p => p.Title)
                         )
                    )
                );
            }
            //İÇEREN (-)İÇERMEYEN
            else if (searchArray.Where(t => t.StartsWith("-")).ToList().Count > 0)
            {

                string excludeWord = string.Join(" ", searchArray.Where(t => t.StartsWith("-"))).Replace("-", "");
                string includeWords = string.Join(" ", searchArray.Where(t => !t.StartsWith("-")));

                searchQuery = searchQuery
                .Query(q => q
                    .Bool(b => b
                        .Must(sh =>
                            sh.Match(mt => mt
                                .Field("spot")
                                .Query(includeWords)
                            ) ||
                            sh.Match(mt => mt
                                .Field("fullNews")
                                .Query(includeWords)
                            ) ||
                            sh.Match(mt => mt
                                .Field("title")
                                .Query(includeWords)
                            )
                        )
                        .MustNot(mn => mn
                        .Bool(bb => bb
                            .Should(ssh => ssh
                                .Match(mmt => mmt
                                    .Field("spot")
                                    .Query(excludeWord)
                                ) || ssh
                                .Match(mmt => mmt
                                    .Field("fullNews")
                                    .Query(excludeWord)
                                ) || ssh
                                .Match(mmt => mmt
                                    .Field("title")
                                    .Query(excludeWord)
                                )
                            )
                        )
                        ))
                );
            }
            //ÇOKLU BULANIK ARAMA
            else
            {
                searchQuery = searchQuery
                .Query(q => q
                    .MultiMatch(m => m
                    .Fields(f => f
                        .Field(f => f.Spot)
                        .Field(ff => ff.FullNews)
                        .Field(ff => ff.Title))
                    .Query(searchText)
                    .Fuzziness(Fuzziness.Auto)
                    )
                );
            }

            string[] highField = { "spot", "fullNews", "title" };
            string[] includeFields = { "newsId", "spot", "fullNews", "title" };
            //var searchResultData2 = await _elasticSearchService.DetailSearchAsync<NewsDTO, int>(indexName, searchQuery, skipItemCount, maxItemCount, includeFields, false, highField);

            var result = await DetailSearch(indexName, searchQuery,skipItemCount,maxItemCount, includeFields, false, highField);
            var data = JsonConvert.SerializeObject(result.Documents);
            var responseData = JsonConvert.DeserializeObject<List<NewsDTO>>(data);
            return responseData;
        }




    }
}

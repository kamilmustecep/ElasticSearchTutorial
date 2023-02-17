using ElasticSearchApi.BLL.DataService;
using ElasticSearchApi.BLL.DTO;
using ElasticSearchApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ElasticSearchApi.Controllers
{

    [ApiController]
    public class ApiController : ControllerBase
    {

        public readonly NewsService _newsService;
        public CategoryService _categoryService;

        public ApiController(NewsService newsService, CategoryService categoryService)
        {
            _newsService = newsService;
            _categoryService = categoryService;
        }

        [HttpGet]
        [Route("api/test")]
        public async Task<IActionResult> Test()
        {
            var model = new NewsDTO();
            return Ok(model);
        }


        [HttpGet]
        [Route("api/createIndex")]
        public IActionResult CreateIndex(string indexName)
        {
            NewsDTO newsModel = new NewsDTO();
            _newsService.CreateIndex(newsModel, indexName);
            return null;
        }


        [HttpPost]
        [Route("api/insertrow")]
        public IActionResult InsertRow(NewsDTO model, string indexName)
        {
            NewsDTO newsModel = new NewsDTO();

            var isRow = _newsService.AddOrUpdateElasticIndex(model, indexName);

            if (isRow.Result)
            {
                return Content("eklendi");
            }
            else
            {
                return Content("hata");
            }

        }

        [HttpGet]
        [Route("api/getfirstdata")]
        public IActionResult GetFirstData(int id,string indexName)
        {

            var response = _newsService.FirstOrDefault(indexName, id);
            if (response.Result == null)
            {
                return Content("Data yok");
            }
            else
            {
                return Content(response.Result.Title);
            }

        }


        [HttpGet]
        [Route("api/getsearchdata")]
        public ActionResult GetSearchData(string keywords, string indexName)
        {


            var qq = _newsService.DetailSearchGetAsync(indexName, keywords).Result;

            int a = 5;
            return Ok(qq);

        }


        [HttpGet]
        [Route("api/deletespesific")]
        public IActionResult DeleteSpesific(int id, string indexName,int pubid)
        {

            var response = _newsService.DeleteSpecific(indexName, id, pubid, "pkNewsId", "fkCategoryId");
            if (response.Result == null)
            {
                return Content("Data yok");
            }
            else
            {
                return Content(response.Result.ToString());
            }

        }

    }
}

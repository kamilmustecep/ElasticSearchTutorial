using ElasticSearchApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ElasticSearchApi.Controllers
{
    public class HomeController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }


    }
}
using Microsoft.AspNetCore.Mvc;

namespace CSGO_Float_Api.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

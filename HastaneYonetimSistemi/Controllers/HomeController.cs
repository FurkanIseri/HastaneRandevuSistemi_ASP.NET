using Microsoft.AspNetCore.Mvc;

namespace HastaneYonetimSistemi.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

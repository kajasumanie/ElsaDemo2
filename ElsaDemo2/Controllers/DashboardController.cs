using Microsoft.AspNetCore.Mvc;

namespace ElsaDemo2.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

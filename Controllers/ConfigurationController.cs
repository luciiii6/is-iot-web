using Microsoft.AspNetCore.Mvc;

namespace IsIoTWeb.Controllers
{
    public class ConfigurationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

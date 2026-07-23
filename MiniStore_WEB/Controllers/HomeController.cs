using Microsoft.AspNetCore.Mvc;
using ProgramacionAvanzadaWebProyecto.Models;
using System.Diagnostics;

namespace ProgramacionAvanzadaWebProyecto.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return Redirect("/ministore/index.html");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

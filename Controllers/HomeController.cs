using bytech_boards.Models;
using bytech_boards.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace bytech_boards.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDBService _dBService;

        public HomeController(ILogger<HomeController> logger, IDBService dBService)
        {
            _logger = logger;
            _dBService = dBService;
        }

        public async Task<IActionResult> Index()
        {
            List<BoardModel> boards = await _dBService.GetData<BoardModel>("boards");

            return View(boards);
        }

        [HttpGet]
        public IActionResult AddBoard()
        {
            return View();
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

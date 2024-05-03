using bytech_boards.Models;
using bytech_boards.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace bytech_boards.Controllers
{
    public class BoardController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDBService _dBService;

        public class AddBoardModel
        {
            [Required]
            public string name { get; set; }
        }
        public BoardController(ILogger<HomeController> logger, IDBService dBService)
        {
            _logger = logger;
            _dBService = dBService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string id)
        {
            BoardModel? board = (await _dBService.GetData<BoardModel>("boards", $"id = '{id}'"))?.FirstOrDefault();

            if(board == null) { NotFound(); }

            return View(board);
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromForm]AddBoardModel form)
        {
            if(!ModelState.IsValid)
            {
                return RedirectToAction("addboard", "home");
            }

            BoardModel board = new BoardModel()
            {
                Id = Guid.NewGuid().ToString().Replace("-",""),
                Name = form.name,
                Created = DateTime.Now,
                lastUpdateId = Guid.NewGuid().ToString().Replace("-",""),
                LastUpdate = DateTime.Now,
            };


            board.PathToFile = "/app/wwwroot/data/" + board.Id.ToString().Replace("-", "") + ".bmp";

            await _dBService.AddData("boards", board);

            return RedirectToAction("index", "home");
        }
    }
}

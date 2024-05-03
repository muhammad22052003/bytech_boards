using bytech_boards.Models;
using bytech_boards.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace bytech_boards.Controllers
{
    public class ApiController : Controller
    {
        private IDBService _iDBService;

        public ApiController
        (
            IDBService dBService
        )
        {
            _iDBService = dBService;
        }

        [HttpPost]
        public async Task<IActionResult> AddBoard([FromForm] IFormFile image)
        {
            string id = Request.Headers["id"];
            string updateId = Request.Headers["updateId"];
            string name = Request.Headers["name"];

            if (name == null || name.Length == 0) { return NotFound(); }

            BoardModel board = new BoardModel()
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Created = DateTime.Now,
                lastUpdateId = Guid.NewGuid().ToString(),
                LastUpdate = DateTime.Now,
            };

            board.PathToFile = "..\\data\\boards\\" + board.Id.ToString().Replace("-","") + ".bmp";

            try
            {
                using (FileStream fileStream = new FileStream(board.PathToFile, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }
            }
            catch (Exception)
            {
                return Problem(statusCode: 500);
            }

            await _iDBService.AddData("boards", board);

            Response.Headers.Add("id", board.Id);
            Response.Headers.Add("updateId", board.lastUpdateId);
            Response.Headers.Add("name", board.Name);

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetBoard(string id, string updateId)
        {
            if(id == null || id.Length == 0)
            {
                return Empty;
            }

            string name = Request.Headers["name"];

            BoardModel? board = (await _iDBService.GetData<BoardModel>("boards", condition: $"id = '{id}'"))
                                ?.FirstOrDefault();

            if(board == null) { return NotFound(); }

            Response.Headers.Add("id", board.Id);
            Response.Headers.Add("updateId", board.lastUpdateId);
            Response.Headers.Add("name", board.Name);

            if (board.lastUpdateId == updateId || !System.IO.File.Exists(board.PathToFile)) { return Empty; }

            byte[] fileData = System.IO.File.ReadAllBytes(board.PathToFile);

            return File(fileData, System.Net.Mime.MediaTypeNames.Application.Octet, "data.bmp");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteBoard(string id)
        {
            BoardModel? board = (await _iDBService.GetData<BoardModel>("boards", $"id = '{id}'"))?.FirstOrDefault();

            if(board != null)
            {
                if (System.IO.File.Exists(board.PathToFile))
                {
                    //System.IO.File.Delete(board.PathToFile);
                }

                await _iDBService.DeleteData("boards", board);
            }

            return RedirectToAction("index", "home");
        }

        [HttpPost]
        public async Task<IActionResult> SetBoard([FromForm]IFormFile image)
        {
            string id = Request.Headers["id"];
            string updateId = Request.Headers["updateId"];
            string name = Request.Headers["name"];

            if (id == null || updateId == null) { return NotFound(); }

            BoardModel? board;

            try
            {
                board = (await _iDBService.GetData<BoardModel>("boards", $"id = '{id}'"))
                                 ?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }


            if(board == null ) { return NotFound(); }

            board.lastUpdateId = Guid.NewGuid().ToString();
            board.LastUpdate = DateTime.Now;

            try
            {
                using (FileStream fileStream = new FileStream(board.PathToFile, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Problem(statusCode : 500);
            }

            await _iDBService.EditData(board, "boards");

            Response.Headers.Add("id", board.Id);
            Response.Headers.Add("updateId", board.lastUpdateId);
            Response.Headers.Add("name", board.Name);

            Console.WriteLine(Response.Headers["id"]);
            Console.WriteLine(Response.Headers["updateId"]);
            Console.WriteLine(Response.Headers["name"]);

            return Ok();
        }
    }
}

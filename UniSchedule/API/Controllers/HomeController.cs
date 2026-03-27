// ┌────────────────────────────────────────────────────────────────────────────┐
// │ UniSchedule                                                                │
// │ WebAPI расширение для отслеживания расписания учебных занятий              │
// ├────────────────────────────────────────────────────────────────────────────┤
// │ Файл: HomeController.cs                                                    │
// │ Описание: Контроллер для вывода верстки                                    │
// └────────────────────────────────────────────────────────────────────────────┘
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Пространство имен контроллеров api
/// </summary>
namespace UniSchedule.API.Controllers{
    public class HomeController : Controller{
        private readonly API _api;
        
        public HomeController(API api){
            _api = api;
        }
        
        [HttpGet("/index.html")]
        public async Task<IActionResult> Index(){
            var data = await _api.Main();
            return Content(data, "text/html");
        }
        
        [HttpGet("/")]
        public IActionResult Root(){
            return Redirect("/index.html");
        }
    }
}

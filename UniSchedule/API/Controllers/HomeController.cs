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
    /// <summary>
    /// Домашняя страница
    /// </summary>
    public class HomeController : Controller{
        
        /// <summary>
        /// Конструктор
        /// </summary>
        public HomeController(){}
        
        /// <summary>
        /// Открыть домашнюю страницу
        /// </summary>
        /// <returns>HTML верстку index.html в webroot</returns>
        [HttpGet("/index.html")]
        public async Task<IActionResult> Index(){
            string data = "";
            using (StreamReader reader = new StreamReader(@"./wwwroot/index.html")){
                string? line;
                while ((line = await reader.ReadLineAsync()) != null){
                data += line;
                }
            }
            return Content(data, "text/html");
        }
        
        /// <summary>
        /// Открыть корень
        /// </summary>
        /// <returns>Редирект на index.html</returns>
        [HttpGet("/")]
        public IActionResult Root(){
            return Redirect("/index.html");
        }
    }
}

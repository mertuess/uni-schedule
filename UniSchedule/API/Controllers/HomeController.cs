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
        /// Экземпляр API
        /// </summary>
        private readonly API _api;
        
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="api">Экземпляр API</param>
        public HomeController(API api){
            _api = api;
        }
        
        /// <summary>
        /// Открыть домашнюю страницу
        /// </summary>
        /// <returns>HTML верстку index.html в webroot</returns>
        [HttpGet("/index.html")]
        public async Task<IActionResult> Index(){
            var data = await _api.Main();
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

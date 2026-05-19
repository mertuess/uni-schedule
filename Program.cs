// ┌────────────────────────────────────────────────────────────────────────────┐
// │ UniSchedule                                                                │
// │ WebAPI расширение для отслеживания расписания учебных занятий              │
// ├────────────────────────────────────────────────────────────────────────────┤
// │ Файл: Program.cs                                                           │
// │ Описание: Точка входа в приложение                                         │
// └────────────────────────────────────────────────────────────────────────────┘

using Microsoft.OpenApi;
using UniSchedule;
using UniSchedule.API;
using UniSchedule.DataBase;
using UniSchedule.Json;
using UniSchedule.System;
using UniSchedule.Services;

var builder = WebApplication.CreateBuilder(args); // Создаем builder для настройки webapi

// Задаем прослушивание адреса сервера
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000);
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(_ => true)  // Разрешить любые источники
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddControllers(); // Добавляем поддержку контроллеров
builder.Services.AddOpenApi(); // Добавляем поддержку OpenAPI для генерации документации
builder.Services.AddScoped<ICalendarService, CalendarService>();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "UniSchedule API", Version = "v1" });
    c.OperationFilter<SwaggerHeaderFilter>(); // Добавляем фильтр для заголовков
});

// Регистрируем сервисы в режиме singleton, то есть один экземплр на все приложение
builder.Services.AddSingleton<Localization>(); // для текста и локализации
builder.Services.AddSingleton<Debug>(); // для логгирования
builder.Services.AddSingleton<DataBaseManager>(); // для работы с базой данных внутри нашего api
builder.Services.AddSingleton<OutAPI>(); // для работы с внешним api
builder.Services.AddSingleton<JsonParser>(); // для обработки json строк

builder.Services.AddHostedService<InitializationService>(); // Инициализируем БД

var app = builder.Build(); // Собираем приложение с настройками builder

// При запуске в development добавляем endpoint для Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.MapOpenApi(); // Оставляем OpenAPI для релизной версии

app.UseHttpsRedirection(); // Используем редирект на https с http
app.UseStaticFiles(); // Позволяем использовать js, css и прочие ресурсы верстки
app.MapControllers(); // Подключаем контроллеры запросов

app.UseMiddleware<AuthenticationMiddleware>(); // Добавляем проверку авторизации в каждом запросе

app.Run(); // Запускаем приложение
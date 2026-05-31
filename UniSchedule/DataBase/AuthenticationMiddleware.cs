using UniSchedule;
using UniSchedule.DataBase;
using UniSchedule.System;

public class AuthenticationMiddleware
{
    private readonly Debug _dbg;
    private readonly Localization _loc;
    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(RequestDelegate next, Debug dbg, Localization loc)
    {
        _next = next;
        _dbg = dbg;
        _loc = loc;
    }

    public async Task InvokeAsync(HttpContext context, DataBaseManager db)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        var method = context.Request.Method.ToUpperInvariant();

        // ПУБЛИЧНЫЕ ПУТИ (доступны всем без авторизации) 
        var isPublicPath = false;

        if (method == "GET")
        {
            // Статика и главные страницы
            if (path == "/" || path == "" || path == "/index.html" ||
                path == "/login.html" || path == "/pages/login.html" ||
                path.StartsWith("/js/") || path.StartsWith("/css/") ||
                path.StartsWith("/images/") || path.StartsWith("/swagger") ||
                path.StartsWith("/openapi"))
            {
                isPublicPath = true;
            }

            // Публичные API эндпоинты
            if (path.StartsWith("/api/database/tryauth") ||
                path.StartsWith("/api/buildings") ||
                path.StartsWith("/api/rooms") ||
                path.StartsWith("/api/teachers") ||
                path.StartsWith("/api/schedule") ||
                path.StartsWith("/api/calendar") ||
                path == "/api/database/departments" ||
                (path.StartsWith("/api/database/departments/") &&
                 (path.EndsWith("/teachers") || path.EndsWith("/schedule"))))
            {
                isPublicPath = true;
            }
        }

        if (isPublicPath)
        {
            await _next(context);
            return;
        }

        // === ВСЕ ОСТАЛЬНЫЕ ЗАПРОСЫ ТРЕБУЮТ АВТОРИЗАЦИИ ===
        var hasEmail = context.Request.Headers.TryGetValue("Uni-Email", out var emailHeader);
        var hasPassword = context.Request.Headers.TryGetValue("Uni-Password", out var passwordHeader);

        if (!hasEmail || !hasPassword)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Требуется авторизация" });
            return;
        }

        var user = await db.AuthenticateUserAsync(emailHeader.ToString(), passwordHeader.ToString());
        if (user == null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Неверные учетные данные" });
            return;
        }

        context.Items["User"] = user;
        context.Items["UserRole"] = user.Role;
        await _next(context);
    }
}
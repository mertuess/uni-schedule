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
        var path = context.Request.Path.ToString().ToLower();
        
        if (path == "/index.html" || path == "/" || path == "/api/database/tryauth")
        {
            await _next(context);
            return;
        }
        
        // Публичные пути для iCal календаря
        if (path.StartsWith("/api/calendar"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("Uni-Email", out var email) ||
            !context.Request.Headers.TryGetValue("Uni-Password", out var password))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            _dbg.Error(string.Format(_loc.Text["req_err"], context.Request.Method, path));
            await context.Response.WriteAsJsonAsync(new { error = "Missing authentication headers" });
            return;
        }

        var user = await db.AuthenticateUserAsync(email.ToString(), password.ToString());

        if (user == null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            _dbg.Warning(string.Format(_loc.Text["req_warn"], context.Request.Method, email.ToString(), path));
            await context.Response.WriteAsJsonAsync(new { error = "Invalid credentials" });
            return;
        }

        context.Items["User"] = user;
        context.Items["UserRole"] = user.Role;

        _dbg.Log(string.Format(_loc.Text["req_info"], context.Request.Method, email.ToString(), path));
        await _next(context);
    }
}
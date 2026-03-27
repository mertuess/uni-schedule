using UniSchedule.DataBase;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, DataBaseManager db)
    {
        var path = context.Request.Path.ToString().ToLower();
        if (path == "/index.html" || path == "/")
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("Uni-Email", out var email) ||
            !context.Request.Headers.TryGetValue("Uni-Password", out var password))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Missing authentication headers" });
            return;
        }

        Console.WriteLine($"{email.ToString()}: {context.Request.Method} {path}");
        var user = await db.AuthenticateUserAsync(email.ToString(), password.ToString());

        if (user == null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid credentials" });
            return;
        }

        context.Items["User"] = user;
        context.Items["UserRole"] = user.Role;

        await _next(context);
    }
}

using UniSchedule;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.MapGet("/", async context =>
{
    string data = "";

    

    using (StreamReader reader = new StreamReader(@"./wwwroot/index.html"))
    {
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            data += line;
            if(line.Contains("data-type=\"group\"")){
                
            }
        }
    }
    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(data);
});

app.Run();

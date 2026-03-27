// ┌────────────────────────────────────────────────────────────────────────────┐
// │ UniSchedule                                                                │
// │ WebAPI расширение для отслеживания расписания учебных занятий              │
// ├────────────────────────────────────────────────────────────────────────────┤
// │ Файл: SwaggerHeaderFilter.cs                                               │
// │ Описание: Класс реализующий ввод данных авторизации в запросы Swagger      │
// └────────────────────────────────────────────────────────────────────────────┘
// Подключения
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

/// <summary>
/// Класс фильтра для swagger
/// </summary>
public class SwaggerHeaderFilter : IOperationFilter
{
  /// <summary>
  /// Применение фильтра, добавление двух параметров в окна запросов
  /// </summary>
  public void Apply(OpenApiOperation operation, OperationFilterContext context)
  {
    if (operation.Parameters == null)
      operation.Parameters = new List<OpenApiParameter>() as IList<IOpenApiParameter>?? 
        throw new Exception("");

    operation.Parameters.Add(new OpenApiParameter
    {
      Name = "Uni-Email",
      In = ParameterLocation.Header,
      Required = true,
      Description = "Email пользователя",
      Schema = new OpenApiSchema
      {
        Type = Microsoft.OpenApi.JsonSchemaType.String
      }
    });

    operation.Parameters.Add(new OpenApiParameter
    {
      Name = "Uni-Password",
      In = ParameterLocation.Header,
      Required = true,
      Description = "Пароль пользователя",
      Schema = new OpenApiSchema
      {
        Type = Microsoft.OpenApi.JsonSchemaType.String
      }
    });
  }
}

namespace UniSchedule.API.Responses;

/// <summary>
///     Абстрактный базовый класс для обработки API-ответов.
/// </summary>
public abstract class Response
{
    protected OutAPI _o_api;

    /// <summary>
    ///     Инициализирует новый экземпляр класса Response.
    /// </summary>
    /// <param name="o_api">Экземпляр OutAPI для отправки запросов</param>
    public Response(OutAPI o_api)
    {
        _o_api = o_api;
    }
}
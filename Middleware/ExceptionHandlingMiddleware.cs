using System.Text.Json;
using PixApiRest.Exceptions;

namespace PixApiRest.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorResponse
        {
            Timestamp = DateTime.UtcNow,
            Path = context.Request.Path
        };

        switch (exception)
        {
            case ResourceNotFoundException ex:
                _logger.LogWarning("Recurso não encontrado: {Message}", ex.Message);
                response.StatusCode = StatusCodes.Status404NotFound;
                errorResponse.Status = StatusCodes.Status404NotFound;
                errorResponse.Error = "Not Found";
                errorResponse.Message = ex.Message;
                break;

            case BusinessException ex:
                _logger.LogWarning("Erro de negócio: {Message}", ex.Message);
                response.StatusCode = StatusCodes.Status400BadRequest;
                errorResponse.Status = StatusCodes.Status400BadRequest;
                errorResponse.Error = "Bad Request";
                errorResponse.Message = ex.Message;
                break;

            default:
                _logger.LogError(exception, "Erro interno: {Message}", exception.Message);
                response.StatusCode = StatusCodes.Status500InternalServerError;
                errorResponse.Status = StatusCodes.Status500InternalServerError;
                errorResponse.Error = "Internal Server Error";
                errorResponse.Message = "Ocorreu um erro interno no servidor";
                break;
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
    }
}

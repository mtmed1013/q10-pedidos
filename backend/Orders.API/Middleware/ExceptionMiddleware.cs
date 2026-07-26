using System.Text.Json;
using Orders.API.Exceptions;
using Orders.API.Responses;

namespace Orders.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (CustomException ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex.Code;

            var response = new ApiResponse<object>(
                false,
                ex.Message,
                null
            );

            await context.Response.WriteAsJsonAsync(response);
        }
        catch (Exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 500;

            var response = new ApiResponse<object>(
                false,
                "Error interno del servidor",
                null
            );

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
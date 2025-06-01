using Common;
using System.Net;
using System.Text.Json;

namespace Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (KeyNotFoundException ex)
            {
                await HandleExceptionAsync(context, HttpStatusCode.NotFound, "Resource not found");
            }
            catch (UnauthorizedAccessException ex)
            {
                await HandleExceptionAsync(context, HttpStatusCode.Unauthorized, "Unauthorized access");
            }
            catch (ArgumentException ex)
            {
                await HandleExceptionAsync(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, string errorMessage)
        {
                // Yo'naltirish
                context.Response.Redirect($"/Error?message={WebUtility.UrlEncode(errorMessage)}");
        }
    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
using EntityCore.Tools.Middlewares;

namespace EntityCore.Test.Middlewares
{
    public class ExceptionHandlerMiddlewareTest
    {
        [Fact]
        public void Should_Generate_Exception_Handler_Middleware()
        {
            // Arrange
            var exceptionHandlerMiddleware = new ExceptionHandlerMiddleware();
            var expectedCode = $@"using Common;
using System.Net;
using System.Text.Json;

namespace Middlewares
{{
    public class ExceptionHandlingMiddleware
    {{
        private readonly RequestDelegate _next;
        public ExceptionHandlingMiddleware(RequestDelegate next)
        {{
            _next = next;
        }}

        public async Task InvokeAsync(HttpContext context)
        {{
            try
            {{
                await _next(context);
            }}
            catch (KeyNotFoundException ex)
            {{
                await HandleExceptionAsync(context, HttpStatusCode.NotFound, ""Resource not found"");
            }}
            catch (UnauthorizedAccessException ex)
            {{
                await HandleExceptionAsync(context, HttpStatusCode.Unauthorized, ""Unauthorized access"");
            }}
            catch (ArgumentException ex)
            {{
                await HandleExceptionAsync(context, HttpStatusCode.BadRequest, ex.Message);
            }}
            catch (Exception ex)
            {{
                await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, ex.Message);
            }}
        }}

        private async Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, string errorMessage)
        {{
            context.Response.ContentType = ""application/json"";
            context.Response.StatusCode = (int)statusCode;
            var result = Result.Fail(errorMessage);
            var jsonResult = JsonSerializer.Serialize(result);
            await context.Response.WriteAsync(jsonResult);
        }}
    }}

    public static class ExceptionHandlingMiddlewareExtensions
    {{
        public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder app)
        {{
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }}
    }}
}}";

            // Act
            string generatedCode = exceptionHandlerMiddleware.Generate();

            // Assert
            Assert.NotEmpty(generatedCode); // Ensure the code is not empty

            Assert.Contains(expectedCode, generatedCode);
        }
    }
}

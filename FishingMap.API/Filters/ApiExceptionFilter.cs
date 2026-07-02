using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FishingMap.API.Filters
{
    /// <summary>
    /// Maps service-layer exceptions to HTTP responses so controllers don't need
    /// per-action try/catch: KeyNotFoundException → 404, ArgumentException → 400
    /// with the message, anything else → generic 500.
    /// </summary>
    public class ApiExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<ApiExceptionFilter> _logger;

        public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            switch (context.Exception)
            {
                case KeyNotFoundException:
                    context.Result = new NotFoundResult();
                    break;
                case ArgumentException argumentException:
                    context.Result = new BadRequestObjectResult(argumentException.Message);
                    break;
                default:
                    _logger.LogError(context.Exception,
                        "Unhandled exception handling {Method} {Path}",
                        context.HttpContext.Request.Method,
                        context.HttpContext.Request.Path);
                    context.Result = new ObjectResult("An error occurred while processing your request.")
                    {
                        StatusCode = StatusCodes.Status500InternalServerError
                    };
                    break;
            }

            context.ExceptionHandled = true;
        }
    }
}

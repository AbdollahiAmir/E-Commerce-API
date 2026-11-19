using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Shop_API.Exceptions;

namespace Shop_API.Handlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
            {
                httpContext.Response.StatusCode = 499;

                return true;
            }

            var (statusCode, title, detail) = exception switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, "یافت نشد", exception.Message),
                ConflictException => (StatusCodes.Status409Conflict, "تداخل در درخواست", exception.Message),
                ArgumentException => (StatusCodes.Status400BadRequest, "درخواست نامعتبر", exception.Message),
                BadHttpRequestException badRequest => (badRequest.StatusCode, "درخواست نامعتبر", "درخواست نامعتبر است یا حجم آن بیش از حد مجاز است."),
                InvalidDataException => (StatusCodes.Status400BadRequest, "درخواست نامعتبر", "داده‌های ارسال‌شده معتبر نیستند."),
                _ => (StatusCodes.Status500InternalServerError, "خطای سرور", "خطای داخلی سرور رخ داد. لطفاً دوباره تلاش کنید.")
            };

            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception, "خطای مدیریت‌نشده در پردازش درخواست رخ داد.");
            }

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail
            }, cancellationToken);

            return true;
        }
    }
}
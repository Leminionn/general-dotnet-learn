using FirstAPIProject.Application.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FirstAPIProject.API.Middlewares
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

            var (statusCode, title) = exception switch
            {
                ValidationException => (StatusCodes.Status400BadRequest, "Validation error"),
                ArgumentException => (StatusCodes.Status400BadRequest, "Invalid argument"),
                EmailNotWhitelistedException => (StatusCodes.Status403Forbidden, "Email not allowed"),
                UserAlreadyExistsException => (StatusCodes.Status409Conflict, "User already exists"),
                WhitelistAlreadyExistsException => (StatusCodes.Status409Conflict, "Whitelist entry already exists"),
                InvalidCredentialsException => (StatusCodes.Status401Unauthorized, "Invalid credentials"),
                InvalidRefreshTokenException => (StatusCodes.Status401Unauthorized, "Invalid refresh token"),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized access"),
                UserNotFoundException => (StatusCodes.Status404NotFound, "User not found"),
                WhitelistNotFoundException => (StatusCodes.Status404NotFound, "Whitelist entry not found"),
                _ => (StatusCodes.Status500InternalServerError, "Internal server error")
            };

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = statusCode == StatusCodes.Status500InternalServerError
                    ? "An unexpected error occurred. Please try again later."
                    : exception.Message,
                Instance = httpContext.Request.Path
            };

            if (exception is ValidationException validationEx)
            {
                var errors = validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray());

                problemDetails.Extensions["errors"] = errors;
            }

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}

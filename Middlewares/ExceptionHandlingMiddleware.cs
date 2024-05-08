using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace api.Middlewares
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }

    public class ForbiddenAccessException : Exception
    {
        public ForbiddenAccessException(string message) : base(message) { }
    }

    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }

    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message) { }
    }

    public class ExceptionHandlingMiddleware
    {
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger, RequestDelegate next)
        {
            _logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation($"---- Exception Handling Requests: {context.Request.Method} {context.Request.Path} ----");
            try
            {
                await _next(context); // controller - service 
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unhandled excpetion: {ex}");
                // that will handle specific exception
                await HandleExceptionAsync(context, ex);
            }
            finally
            {
                _logger.LogInformation($"---- Finished Exception Handling Requests: Response status {context.Response.StatusCode} ----");
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            // Default response for an unexpected error
            var responseCode = StatusCodes.Status500InternalServerError;
            var message = "An unexpected error has occurred.";

            // Handling specific exceptions
            switch (exception)
            {
                case NotFoundException notFoundException:
                    responseCode = StatusCodes.Status404NotFound;
                    message = notFoundException.Message;
                    break;

                case ValidationException validationException:
                    responseCode = StatusCodes.Status400BadRequest;
                    message = validationException.Message;
                    break;

                case UnauthorizedAccessException unauthorizedAccessException:
                    responseCode = StatusCodes.Status401Unauthorized;
                    message = unauthorizedAccessException.Message;
                    break;

                case ForbiddenAccessException forbiddenAccessException:
                    responseCode = StatusCodes.Status403Forbidden;
                    message = forbiddenAccessException.Message;
                    break;

                case ConflictException conflictException:
                    responseCode = StatusCodes.Status409Conflict;
                    message = conflictException.Message;
                    break;

                case BadRequestException badRequestException:
                    responseCode = StatusCodes.Status400BadRequest;
                    message = badRequestException.Message;
                    break;

                case ApplicationException applicationException:
                    // Handling generic application exceptions that might be used for business logic errors
                    responseCode = StatusCodes.Status400BadRequest;
                    message = applicationException.Message;
                    break;

                default:
                    // Log the exception if it's not one of the expected types
                    Console.WriteLine("Unhandled exception type: ", exception.GetType());
                    break;
            }

            context.Response.StatusCode = responseCode;

            var response = new
            {
                StatusCode = responseCode,
                Message = message
            };

            var jsonResponse = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(jsonResponse);
        }

    }
}
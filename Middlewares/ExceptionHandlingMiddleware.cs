using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace api.Middlewares
{
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

            Console.WriteLine($"----------Hello1");

            context.Response.ContentType = "application/json";
            var responseCode = StatusCodes.Status500InternalServerError;
            var message = "Internal Server Error from the middleware";

            Console.WriteLine($"----------Hello2");


            switch (exception)
            {
                case DllNotFoundException notFoundException:
                    responseCode = StatusCodes.Status404NotFound;
                    message = notFoundException.Message;
                    break;

                case ValidationException validationException:
                    responseCode = StatusCodes.Status400BadRequest;
                    message = validationException.Message;
                    break;
                default:
                    if (exception is ApplicationException)
                    {
                        responseCode = StatusCodes.Status400BadRequest;
                        message = exception.Message;
                    }
                    break;
            }

            context.Response.StatusCode = responseCode;

            var response = new
            {
                StatusCode = responseCode,
                Message = message
            };

            Console.WriteLine("------ Response Message : " + response.Message);



            var jsonResponse = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(jsonResponse);
        }

    }
}
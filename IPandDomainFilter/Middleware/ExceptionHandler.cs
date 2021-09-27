using IPandDomainFilter.Abstraction;
using IPandDomainFilter.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static IPandDomainFilter.Middleware.DomainException;

namespace IPandDomainFilter.Middleware
{
    public class ExceptionHandler
    {
        private readonly RequestDelegate _next;
        private IExceptionHandlingInDatabase _exceptionHandlingInDatabase;

        public ExceptionHandler(RequestDelegate next, IExceptionHandlingInDatabase exceptionHandlingInDatabas)
        {
            _next = next;
            _exceptionHandlingInDatabase = exceptionHandlingInDatabas;
        }
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (DomainNotFoundException exception)
            {
                int statusCode = (int)HttpStatusCode.InternalServerError;
                var message = new ErrorMessage
                {
                    statusCode = statusCode,
                    errorMessage = exception.Message
                };
                _exceptionHandlingInDatabase.StoreException(message);
                string jsonString = JsonConvert.SerializeObject(message);
                await context.Response.WriteAsync(jsonString);
            }
            catch (DomainValidationException exception)
            {
                int statusCode = (int)HttpStatusCode.Forbidden;
                var message = new ErrorMessage
                {
                    statusCode = statusCode,
                    errorMessage = exception.Message
                };
                _exceptionHandlingInDatabase.StoreException(message);
                string jsonString = JsonConvert.SerializeObject(message);
                await context.Response.WriteAsync(jsonString);
            }
            catch (Exception exception)
            {
                await HandleExceptionMessageAsync(context, exception).ConfigureAwait(false);
            }
        }

        public Task HandleExceptionMessageAsync(HttpContext context, Exception exception)
        {
            int statusCode = (int)HttpStatusCode.InternalServerError;

            var commanMessage = new ErrorMessage
            {
                statusCode = statusCode,
                errorMessage = exception.Message
            };

            _exceptionHandlingInDatabase.StoreException(commanMessage);

            string jsonString = JsonConvert.SerializeObject(commanMessage);

            return context.Response.WriteAsync(jsonString);
        }
    }
}

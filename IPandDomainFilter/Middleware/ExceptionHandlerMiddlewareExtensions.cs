using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IPandDomainFilter.Middleware
{
    public static class ExceptionHandlerMiddlewareExtensions
    {

        public static void UseExceptionHandlerMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionHandler>();
        }
    }
}

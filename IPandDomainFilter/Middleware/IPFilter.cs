using IPandDomainFilter.Abstraction;
using IPandDomainFilter.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;



namespace IPandDomainFilter.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class IPFilter
    {
        private readonly RequestDelegate _next;
        private IConfiguration _iconfiguration;
        private IExceptionHandlingInDatabase _exceptionHandlingInDatabase;

        public IPFilter(RequestDelegate next, IConfiguration iconfiguration, IExceptionHandlingInDatabase exceptionHandlingInDatabase)
        {
            _next = next;
            _iconfiguration = iconfiguration;
            _exceptionHandlingInDatabase = exceptionHandlingInDatabase;
        }

        //Encrypt url
        public static string EncryptString(string key, string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }   
                        array = memoryStream.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(array);
        }

        public bool DataCheck(HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress;

            var domain = context.Response.HttpContext.Request.Host;

            string urlPath = context.Response.HttpContext.Request.Path;

            string url = domain + urlPath;

            string key = "b14ca5898a4e4133bbce2ea2315a1916";

            string encryptedurl = EncryptString(key, url);

            List<string> whitelistIpList = _iconfiguration.GetSection("ApplicationOptions:WhiteList").Get<List<string>>();
            List<string> whitelisturlList = _iconfiguration.GetSection("ApplicationOptions:WhiteUrl").Get<List<string>>();
            
            var isInwhiteListIPList = whitelistIpList
                .Where(a => IPAddress.Parse(a)
                .Equals(ipAddress))
                .Any();

            var isInwhiteListDomain = whitelisturlList
                .Where(a => a
                .Equals(encryptedurl))
                .Any();

            if (!isInwhiteListDomain || !isInwhiteListIPList)
            {
                if (!isInwhiteListIPList)
                {
                    var message = new ErrorMessage
                    {
                        statusCode = context.Response.StatusCode = (int)HttpStatusCode.Forbidden,
                        errorMessage = "You don't have valid IP"
                    };
                    _exceptionHandlingInDatabase.StoreException(message);
                    string jsonString = JsonConvert.SerializeObject(message);
                    context.Response.WriteAsync(jsonString);
                }

                if (!isInwhiteListDomain)
                {
                    var message = new ErrorMessage
                    {
                        statusCode = context.Response.StatusCode = (int)HttpStatusCode.Forbidden,
                        errorMessage = "You don't have valid domain"
                    };
                    _exceptionHandlingInDatabase.StoreException(message);
                    string jsonString = JsonConvert.SerializeObject(message);
                    context.Response.WriteAsync(jsonString);
                }
                return false;
            }
            return true;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            bool check = DataCheck(httpContext);
            if (check)
                await _next(httpContext);
            
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class IPFilterExtensions
    {
        public static IApplicationBuilder UseIPFilter(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<IPFilter>();
        }
    }
}

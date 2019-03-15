using System;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Context;

namespace LoggingDemo.Controllers
{
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ILogger logger;

        public HomeController(ILogger logger)
        {
            this.logger = logger.ForContext<HomeController>();
        }

        [HttpGet, Route("")]
        public string Home()
        {
            // Use LogContext to automatically properties to all log entries
            // written within a block of code.  
            using (LogContext.PushProperty("requestMethod", Request.Method))
            using (LogContext.PushProperty("requestUrl", Request.GetDisplayUrl()))
            {
                logger.Information("This is a .NET example of structured logging for customer ID {customerId}.", 42);
                logger.Warning("Ooooo, this is a warning! (from .NET)");

                try
                {
                    var foo = new Foo();
                    foo.Crash();
                }
                catch (Exception e)
                {
                    logger.Error(e, "Look: An intentional error. (from .NET)");
                }

                return "Hello, World. I'm a .NET Core app.";
            }
        }

        private class Foo
        {
            public void Crash()
            {
                throw new ArgumentException("Oh noes, it's broken (intentionally)");
            }
        }
    }
}
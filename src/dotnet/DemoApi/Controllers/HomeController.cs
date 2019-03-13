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
            this.logger = logger?.ForContext<HomeController>();
        }

        [HttpGet, Route("")]
        public string Home(ILogger logger)
        {
            logger.Warning("Ooooo, this is a warning!");

            try
            {
                var foo = new Foo();
                foo.Crash();
            }
            catch (Exception e)
            {
                logger.Error(e, "Look: An intentional error.");
            }

            return "Hello, World. I'm a .NET Core app.";
        }

        [HttpGet, Route("loop")]
        public string LogLoop()
        {
            int i = 0;

            while(i < 1000000)
            {
                Log.Information("Hello. This is log entry #{logIndexNumber}", i++);
            }

            while(i < 2000000)
            {
                Log.Error(new Exception("Intentional error"), "Hello. This is log entry #{logIndexNumber}", i++);
            }

            return $"All done. Wrote {i} log entries.";
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
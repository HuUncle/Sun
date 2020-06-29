using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sample.WebApi.Events;
using Sun.EventBus.Abstractions;

namespace Sample.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            _logger.LogWarning("这里是测试Debug信息");
            //_logger.LogError("这里是测试异常信息");
            var rng = new Random();
            //throw new Exception("测试异常");
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet]
        public IActionResult PublishTestA([FromServices] IEventBus bus)
        {
            bus.Publish(new TestAEvent($"{new Random().Next(1, 100)}"));
            return Ok();
        }

        [HttpGet]
        public IActionResult PublishTestB([FromServices] IEventBus bus)
        {
            bus.Publish(new TestBEvent($"{new Random().Next(1, 100)}"));
            return Ok();
        }
    }
}
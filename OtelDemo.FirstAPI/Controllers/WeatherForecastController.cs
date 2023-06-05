using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OtelDemo.Commons.Services;

namespace OtelDemo.FirstAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private static readonly HttpClient HttpClient = new();

        private readonly Dictionary<string, string> _emptyDict;
        private readonly MetricService _metricService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, MetricService metricService)
        {
            _logger = logger;
            _metricService = metricService;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            using var scope = this._logger.BeginScope("{Id}", Guid.NewGuid().ToString("N"));

            // Making an http call here to serve as an example of
            // how dependency calls will be captured and treated
            // automatically as child of incoming request.
            var res = await HttpClient.GetStringAsync("http://google.com");
            var rng = new Random();
            var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)],
                })
                .ToArray();

            foreach (var f in forecast)
            {
                _metricService.RecordToHistogram("weather_forecast.temperature", 
                    f.TemperatureC, 
                    unit: "celsius",
                    description: "Wheather temperature", 
                    tags: new []
                    {
                        new KeyValuePair<string, object>("date", f.Date.ToShortDateString()),
                        new KeyValuePair<string, object>("summary", f.Summary),
                    });
                
                _metricService.AddToCounter("metricateste", 1);
                _logger.LogWarning(
                    "{forecasts}",
                    JsonSerializer.Serialize(f));
            }

            _logger.LogInformation("Aplicação inicializada.");
            
            _logger.LogInformation("Aplicação encerrada.");

            try
            {
                var a = _emptyDict["empty"];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado.");
            }
            
            return forecast;
        }
    }
}
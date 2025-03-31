using IdentityJwtWeather.Data;
using IdentityJwtWeather.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace IdentityJwtWeather.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SolarPlantProductionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWeatherService _weatherService;
        private readonly ILogger<SolarPowerPlantsController> _logger;

        public SolarPlantProductionController(ApplicationDbContext context, IWeatherService weatherService, ILogger<SolarPowerPlantsController> logger)
        {
            _context = context;
            _weatherService = weatherService;
            _logger = logger;
        }

        [HttpPost("GetProductionData")]
        [Authorize]
        public async Task<IActionResult> GetProductionData([FromBody] ProductionRequest request)
        {
            var now = DateTime.Now;
            TimeSeriesWeatherData timeSeriesWeatherData = new();
            TimeseriesProductionData timeSeriesProductionData = new();

            if(request.TimeSpan > TimeSpan.FromDays(5))
            {
                _logger.LogError("GetProductionData -> failed with BadRequest because request timespan was longer then 5 days: {request}",
                    JsonSerializer.Serialize(request));
                return BadRequest("Do to api limitations there we cannot fetch forecast or actual data past 5 days");
            }

            var plant = await _context.SolarPowerPlants.FindAsync(request.PlantId);
            if (plant == null)
            {
                _logger.LogError("GetProductionData -> failed with NotFound because Solar Power Plant was not found by id: {request}",
                    JsonSerializer.Serialize(request));
                return NotFound();
            }

            // Forcast Data
            if(request.Type == ProductionType.Forecast)
            {
                var rawWeatherForecastData = await _weatherService.GetWeatherForecast(plant.Latitude, plant.Longitude);
                if (rawWeatherForecastData == null)
                {
                    _logger.LogError("GetProductionData -> failed with BadRequest no weather data was available");
                    return BadRequest("No weather data available.");
                }

                timeSeriesWeatherData = _weatherService.ForcastDataParser(rawWeatherForecastData, request.TimeSpan);

                foreach (var WeatherData in timeSeriesWeatherData.TimeSeriesData)
                {
                    var production = ((100 - WeatherData.Clouds) / 100m) *
                        _weatherService.GetSunshineInterpolation(WeatherData.Date) *
                        plant.InstalledPower;

                    var newproductionDate = new ProductionData
                    {
                        Date = WeatherData.Date,
                        Production = production
                    };

                    timeSeriesProductionData.TimeSeriesData.Add(newproductionDate);
                }
            }
            // Actual Data
            else
            {
                var productionData = await _context.SolarPowerPlantProduction
                    .Where(p => p.Date >= now - request.TimeSpan && p.Date <= now)
                    .ToListAsync();

                if (!productionData.Any())
                {
                    _logger.LogError("GetProductionData -> failed with BadRequest no production data was found");
                    return BadRequest("No production data found.");
                }

                timeSeriesProductionData.TimeSeriesData = productionData.Select(p => new ProductionData
                    {
                        Date = p.Date,
                        Production = p.Production
                    }).ToList();
            }

            // Granularity Hourly
            if (request.Granularity == TimeSeries.Hourly)
            {
                var hourlyGroups = timeSeriesProductionData.TimeSeriesData
                    .GroupBy(x => new DateTime(x.Date.Year, x.Date.Month, x.Date.Day, x.Date.Hour, 0, 0, x.Date.Kind))
                    .OrderBy(g => g.Key);

                var newTimeSeriesProductionData = new TimeseriesProductionData();
                foreach (var group in hourlyGroups)
                {
                    var production = group.Sum(x => x.Production);
                    newTimeSeriesProductionData.TimeSeriesData.Add(new ProductionData
                    {
                        Date = group.Key,
                        Production = production
                    });
                }
                timeSeriesProductionData = newTimeSeriesProductionData;
            }

            _logger.LogInformation("GetProductionData -> succeeded with request: {request}", JsonSerializer.Serialize(request));
            return Ok(timeSeriesProductionData.TimeSeriesData);
        }

        public enum TimeSeries
        {
            QuarterHourly,
            Hourly
        }
        public enum ProductionType
        {
            Actual,
            Forecast
        }
        public class ProductionRequest
        {
            public int PlantId { get; set; }
            public TimeSpan TimeSpan { get; set; }
            public ProductionType Type { get; set; } = ProductionType.Actual;
            public TimeSeries Granularity { get; set; } = TimeSeries.QuarterHourly; 
        }
        public class TimeSeriesWeatherData
        {
            public List<WeatherForcastData> TimeSeriesData { get; set; } = new();
        }
        public class WeatherForcastData
        {
            public DateTime Date { get; set; }
            public int Clouds { get; set; }
        }
        public class TimeseriesProductionData
        {
            public List<ProductionData> TimeSeriesData { get; set; } = new();
        }
        public class ProductionData
        {
            public DateTime Date { get; set; }
            public decimal Production { get; set; }
        }
    }
}

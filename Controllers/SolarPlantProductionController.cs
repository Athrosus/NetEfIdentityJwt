using IdentityJwtWeather.Data;
using IdentityJwtWeather.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography.X509Certificates;

namespace IdentityJwtWeather.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SolarPlantProductionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWeatherService _weatherService;

        public SolarPlantProductionController(ApplicationDbContext context, IWeatherService weatherService)
        {
            _context = context;
            _weatherService = weatherService;
        }

        [HttpPost("GetProductionData")]
        [Authorize]
        public async Task<IActionResult> GetProductionData([FromBody] ProductionRequest request)
        {
            DateTime now = DateTime.Now;
            TimeseriesWeatherData timeseriesWeatherData = new();
            TimeseriesProductionData timeseriesProductionData = new();
            WeatherApiRequest weatherApiRequest = new();

            if(request.TimeSpan > TimeSpan.FromDays(5))
            {
                return BadRequest("Do to api limitations there we cannot fetch forecast or actual data past 5 days");
            }

            var plant = await _context.SolarPowerPlants.FindAsync(request.PlantId);
            if (plant == null)
            {
                return NotFound();
            }

            if(request.Type == ProductionType.Forecast)
            {
                var rawWeatherForecastData = await _weatherService.GetWeatherForecast(plant.Latitude, plant.Longitude);
                if (rawWeatherForecastData == null)
                {
                    return BadRequest("No weather data available.");
                }
                timeseriesWeatherData = _weatherService.ForcastDataParser(rawWeatherForecastData, request.TimeSpan);

                foreach (var WeatherData in timeseriesWeatherData.TimeseriesData)
                {
                    var production = ((100 - WeatherData.Clouds) / 100m) *
                        _weatherService.GetSunshineInterpolation(WeatherData.Date) *
                        plant.InstalledPower;

                    ProductionData newproductionDate = new ProductionData
                    {
                        Date = WeatherData.Date,
                        Production = production
                    };
                    timeseriesProductionData.TimeseriesData.Add(newproductionDate);
                }
            }
            else
            {
                var productionData = await _context.SolarPowerPlantProduction
                    .Where(p => p.Date >= now - request.TimeSpan && p.Date <= now)
                    .ToListAsync();

                timeseriesProductionData.TimeseriesData = productionData.Select(p => new ProductionData
                    {
                        Date = p.Date,
                        Production = p.Production
                    }).ToList();
            }

            if (request.Granularity == TimeSeries.Hourly)
            {
                var hourlyGroups = timeseriesProductionData.TimeseriesData
                    .GroupBy(x => new DateTime(
                        x.Date.Year,
                        x.Date.Month,
                        x.Date.Day,
                        x.Date.Hour, 0, 0, x.Date.Kind))
                    .OrderBy(g => g.Key);
                var newTimeseriesProductionData = new TimeseriesProductionData();
                foreach (var group in hourlyGroups)
                {
                    var production = group.Sum(x => x.Production);
                    newTimeseriesProductionData.TimeseriesData.Add(new ProductionData
                    {
                        Date = group.Key,
                        Production = production
                    });
                }
                timeseriesProductionData = newTimeseriesProductionData;
            }



            return Ok(timeseriesProductionData);
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
        public class TimeseriesWeatherData
        {
            public List<WeatherForcastData> TimeseriesData { get; set; } = new();
        }
        public class WeatherForcastData
        {
            public DateTime Date { get; set; }
            public int Clouds { get; set; }
        }
        public class WeatherApiRequest
        {
            public TimeSeries Granularity { get; set; } = TimeSeries.QuarterHourly;
            public TimeSpan TimeSpan { get; set; }
        }
        public class TimeseriesProductionData
        {
            public List<ProductionData> TimeseriesData { get; set; } = new();
        }
        public class ProductionData
        {
            public DateTime Date { get; set; }
            public decimal Production { get; set; }
        }
    }
}

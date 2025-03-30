using IdentityJwtWeather.Data;
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

        public SolarPlantProductionController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> GetProductionData([FromBody] ProductionRequest request)
        {
            DateTime now = DateTime.Now;
            TimeSpan timeSpan;
            TimeseriesWeatherData timeseriesWeatherData = new();
            TimeseriesProductionData timeseriesProductionData = new();

            if (request.To > request.From)
            {
                return BadRequest("Start date needs to be before end date.");
            }

            var plant = await _context.SolarPowerPlants.FindAsync(request.PlantId);
            if (plant == null)
            {
                return NotFound();
            }

            timeSpan = request.To - request.From;
            if (request.Type == ProductionType.Actual && request.To <= now) 
            {
                // good
            }
            else if (request.Type == ProductionType.Forecast && request.From > now)
            {
                // good
            }
            else
            {
                return BadRequest("From or To dates were invalid for the selected ProductionType.\n Actual -> request.From <= now | Forecast -> request.From > now.");
            }

            foreach (var WeatherData in timeseriesWeatherData.TimeseriesData)
            {
                var production = ((100 - WeatherData.Clouds) / 100m) * GetSunshineInterpolation(WeatherData.Date) * plant.InstalledPower;
                ProductionData newproductionDate = new ProductionData
                {
                    Date = WeatherData.Date,
                    Production = production,
                    Clouds = WeatherData.Clouds
                };
                timeseriesProductionData.TimeseriesData.Add(newproductionDate);
            }

            return Ok(timeseriesProductionData);
        }

        public static decimal GetSunshineInterpolation(DateTime dateTime)
        {
            int hours = (int)dateTime.TimeOfDay.TotalHours;

            if (hours <= 12)
            {
                return hours / 12m;
            }
            else
            {
                return (24 - hours) / 12m;
            }
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
            public DateTime From { get; set; }
            public DateTime To { get; set; }
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
            public ProductionType Type { get; set; } = ProductionType.Actual;
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
            public int Clouds { get; set; }
        }
    }
}

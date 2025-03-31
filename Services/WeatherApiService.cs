using IdentityJwtWeather.Controllers;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using static IdentityJwtWeather.Controllers.SolarPlantProductionController;

namespace IdentityJwtWeather.Services
{
    public interface IWeatherService
    {
        Task<TimeseriesWeatherData> GetWeatherForecast(decimal latitude, decimal longitude);
        public TimeseriesWeatherData ForcastDataParser(TimeseriesWeatherData rawData, TimeSpan timeSpan);
        public decimal GetSunshineInterpolation(DateTime dateTime);
    }
    public class WeatherApiService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SolarPowerPlantsController> _logger;

        public WeatherApiService(HttpClient httpClient, IConfiguration configuration, IMemoryCache cache, ILogger<SolarPowerPlantsController> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
            if (string.IsNullOrEmpty(configuration["Weather:ApiKey"]))
            {
                _logger.LogError("WeatherApiService -> failed because WeatherApi key was not configured");
                throw new InvalidOperationException("OpenWeather API key is not configured.");
            }
            _apiKey = configuration["Weather:ApiKey"]!;
        }
        public async Task<TimeseriesWeatherData> GetWeatherForecast(decimal latitude, decimal longitude)
        {
            var cacheKey = $"WeatherForecast_{latitude}_{longitude}";

            if (_cache.TryGetValue(cacheKey, out TimeseriesWeatherData? cachedForecast))
            {
                if(cachedForecast != null)
                {
                    _logger.LogInformation("WeatherApiService.GetWeatherForecast -> succeeded and returned cached data");
                    return cachedForecast;
                }
            }

            HttpClient client = new();
            TimeseriesWeatherData forecast = new();
            var url = $"forecast?lat={latitude}&lon={longitude}&appid={_apiKey}&units=metric";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("WeatherApiService.GetWeatherForecast -> failed because OpenWeather API request failed with status code {statusCode}", response.StatusCode);
                throw new InvalidOperationException($"OpenWeather API request failed with status code {response.StatusCode}.");
            }
            var json = await response.Content.ReadAsStringAsync();

            var weatherResponse = JsonSerializer.Deserialize<WeatherResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (weatherResponse == null)
            {
                _logger.LogError("WeatherApiService.GetWeatherForecast -> failed because there was a problem with deserializing json");
                throw new InvalidOperationException($"There was a problem with deserializing json: {json}.");
            }
            var DateTimeAndClouds = weatherResponse.List.Select(item => new
            {
                item.Dt,
                CloudCoverage = item.Clouds.All
            }).ToList();
            foreach (var item in DateTimeAndClouds)
            {
                WeatherForcastData newWeatherData = new()
                {
                    Date = DateTimeOffset.FromUnixTimeSeconds(item.Dt).DateTime,
                    Clouds = item.CloudCoverage
                };
                forecast.TimeseriesData.Add(newWeatherData);
            }
            _cache.Set(cacheKey, forecast, TimeSpan.FromMinutes(120));
            _logger.LogInformation("WeatherApiService.GetWeatherForecast -> succeeded");
            return forecast;
        }
        public TimeseriesWeatherData ForcastDataParser(TimeseriesWeatherData rawData, TimeSpan timeSpan)
        {
            DateTime startTime, endTime;
            var now = DateTime.Now;

            startTime = now;
            endTime = now.Add(timeSpan);

            TimeseriesWeatherData parsedData = new();

            List<DateTime> intervals = new();

            for (DateTime current = startTime; current <= endTime; current = current.AddMinutes(15))
            {
                intervals.Add(current);
            }

            foreach (var item in intervals)
            {
                var closestCloudData = rawData.TimeseriesData.OrderByDescending(x => x.Date).Where(x => x.Date.Day == item.Day && x.Date.Hour <= item.Hour).FirstOrDefault();

                if (closestCloudData == null)
                {
                    continue;
                }

                var newWeatherForcastData = new WeatherForcastData
                {
                    Date = item,
                    Clouds = closestCloudData.Clouds
                };
                parsedData.TimeseriesData.Add(newWeatherForcastData);
            }
            return parsedData;
        }

        public decimal GetSunshineInterpolation(DateTime dateTime)
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

        public class WeatherResponse
        {
            public List<WeatherItem> List { get; set; } = new();
        }

        public class WeatherItem
        {
            public long Dt { get; set; }
            public Clouds Clouds { get; set; } = new();
        }

        public class Clouds
        {
            public int All { get; set; }
        }
    }
}

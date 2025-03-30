using System.Text.Json;
using static IdentityJwtWeather.Controllers.SolarPlantProductionController;

namespace IdentityJwtWeather.Services
{
    public interface IWeatherService
    {
        Task<TimeseriesWeatherData> GetWeatherForecast(double latitude, double longitude);
    }
    public class WeatherApiService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public WeatherApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Weather:ApiKey"]
                      ?? throw new InvalidOperationException("OpenWeather API key is not configured.");
        }
        public async Task<TimeseriesWeatherData> GetWeatherForecast(double latitude, double longitude)
        {
            var client = new HttpClient();
            TimeseriesWeatherData forecast = new();
            var url = $"forecast?lat={latitude}&lon={longitude}&appid={_apiKey}&units=metric";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"OpenWeather API request failed with status code {response.StatusCode}.");
            }
            var json = await response.Content.ReadAsStringAsync();

            var weatherResponse = JsonSerializer.Deserialize<WeatherResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (weatherResponse == null)
            {
                throw new InvalidOperationException($"There was a problem with deserializing json: {json}.");
            }
            var dtAndClouds = weatherResponse.List.Select(item => new
            {
                item.Dt,
                CloudCoverage = item.Clouds.All
            }).ToList();
            foreach (var item in dtAndClouds)
            {
                WeatherForcastData newWeatherData = new()
                {
                    Date = DateTimeOffset.FromUnixTimeSeconds(item.Dt).DateTime,
                    Clouds = item.CloudCoverage
                };
                forecast.TimeseriesData.Add(newWeatherData);
            }

            return forecast;
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

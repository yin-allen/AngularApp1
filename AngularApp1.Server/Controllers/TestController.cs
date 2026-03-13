using Microsoft.AspNetCore.Mvc;

namespace AngularApp1.Server.Controllers
{
    [ApiController] // 定義這是一個 API
    [Route("[controller]")] // 自動對應路徑為 /Test
    public class Test3Controller : ControllerBase
    {
        [HttpGet("[Action]")]
        public List<WeatherForecast> Weatherforecast()
        {
            var summaries = new[]
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };

            var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                )).ToList();
            return forecast;
        }

        public  record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
        {
            public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
        }
    }
}

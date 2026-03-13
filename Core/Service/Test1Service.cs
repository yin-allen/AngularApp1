
using Core.Interface;
using static Core.Service.TestService;

namespace Core.Service
{
    public class Test1Service : ITest1Service
    {

        public Test1Service
            (
            )
        {
        }

        public List<WeatherForecast> Weatherforecast1()
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

    }
}
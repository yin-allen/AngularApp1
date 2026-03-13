
using static AngularApp1.Server.Controllers.TestController;

namespace Core.Interface
{
    public interface ITestService
    {
        List<Service.TestService.WeatherForecast> Weatherforecast();
    }
}

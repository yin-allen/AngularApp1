using Core.Interface;
using Microsoft.AspNetCore.Mvc;
using static Core.Service.TestService;

namespace AngularApp1.Server.Controllers
{
    [ApiController] // 定義這是一個 API
    [Route("/Api/[controller]")] // 自動對應路徑為 /Test
    public class TestController : ControllerBase
    {
        private readonly ITestService _testService;
        private readonly ITest1Service _testService1;
        public TestController(ITestService testService, ITest1Service testService1)
        {
            _testService = testService;
            _testService1 = testService1;
        }

        [HttpGet("[Action]")]
        public List<WeatherForecast> Weatherforecast()
        {
            // 你可以在這裡使用 _testService
           var aa=_testService1.Weatherforecast1();
            return _testService.Weatherforecast();
        }
    }
}

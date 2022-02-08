using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OtelDemo.Commons.Services;

namespace OtelDemo.SecondAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CounterController : ControllerBase
    {
        private readonly RedisService _redisService;
        
        public CounterController(RedisService redisService)
        {
            _redisService = redisService;
        }
        
        [HttpGet]
        public async Task<object> Get([FromQuery]string key)
        {
            return new
            {
                Key = await _redisService.GetCurrentValue(key)
            };
        }
        
        [HttpPost]
        public async void Add([FromQuery]string key)
        { 
            _redisService.IncrementValue(key);
        }
    }
}
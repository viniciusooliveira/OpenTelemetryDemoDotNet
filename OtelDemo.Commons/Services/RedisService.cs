using System.Threading.Tasks;
using StackExchange.Redis;

namespace OtelDemo.Commons.Services
{
    public class RedisService
    {
        private ConnectionMultiplexer _redis;
        private IDatabase _db;
        
        public RedisService(string endpoint)
        {
            _redis = ConnectionMultiplexer.Connect(endpoint);
            _db = _redis.GetDatabase();
        }
        
        public async void IncrementValue(string key,
            long amount = 1)
        {
            try
            {
                var res = await _db.StringIncrementAsync(key, amount);
            }
            catch
            {
                // ignored
            }
        }
        
        public async Task<long> GetCurrentValue(string key)
        {
            try
            {
                var value = await _db.StringGetAsync(key);

                if (value.HasValue)
                    return (long)value;
            }
            catch
            {
                // ignored
            }

            return 0;
        }
    }
}
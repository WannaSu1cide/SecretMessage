using Messenger.SignUp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Messenger.Services.SIgnUp
{


    public interface IRedisOptions
    {
        public Task SetKeyValueForUserById(Guid id, string value, CancellationToken cancellationToken = default);
        public Task<string>? GetUserAutheKeyById(Guid id, CancellationToken cancellationToken = default);
    }
    public class RedisOptions : IRedisOptions
    {

        private readonly IDistributedCache _cache; 
        private readonly ILogger<RedisOptions> _logger;
        private readonly SignUpContext _ctx;
        public RedisOptions(IDistributedCache cache, ILogger<RedisOptions> logger, SignUpContext ctx)
        {
            _ctx = ctx;
            _cache = cache;
            _logger = logger;
        }


        public  async Task<string>? GetUserAutheKeyById(Guid id,CancellationToken cancellationToken = default)
        {
            string key = $"member:{id}";
            var result = await _cache.GetStringAsync(key);
            Console.WriteLine($"Đây là authe Key: {result}");
            return result;
        }

        public async Task SetKeyValueForUserById(Guid id , string value ,CancellationToken cancellationToken = default)
        {
            var key = $"member:{id}";
            await _cache.SetStringAsync(key, value, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3)
            });


        }



    }
}

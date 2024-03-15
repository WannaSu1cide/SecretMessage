using Messenger.SignUp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using static Messenger.Services.SIgnUp.RedisOptions;

namespace Messenger.Services.SIgnUp
{
    public enum When {
        NotExists,
        Exists
    }

    public interface IRedisOptions
    {
        Task<string>? GetUserAutheKeyById(Guid id, CancellationToken cancellationToken = default);
         Task<bool> SetKeyValueHaveTimeSpan(Guid id , string value , int time, CancellationToken cancellationToken = default);
        Task<bool> UpdateUserById(Guid Id, string value);
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

        // Will retrieve a value with Id Specify 
        public  async Task<string>? GetUserAutheKeyById(Guid id,CancellationToken cancellationToken = default)
        {
            string key = $"member:{id}";
            var result = await _cache.GetStringAsync(key);
          
            return result;
        }
       

        // Set a token that have TimeSpan specified by the function call it 
        public async  Task<bool> SetKeyValueHaveTimeSpan(Guid id, string value, int SetMinutes, CancellationToken cancellationToken = default)

        {
            try
            {
                var key = $"member:{id}";
                await _cache.SetStringAsync(key, value, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(SetMinutes)
                });
                return true;
            }catch(Exception e)
            {
                _logger.LogInformation("{message}", e.Message);
                return false;
            }

        }

        // Create or update a token that dont need timespan 
        public async Task<bool> UpdateUserById(Guid Id, string value)
        {
            try
            {
                var key = $"member:{Id}";
                await _cache.SetStringAsync(key, value);

                return true;
            }catch(Exception e)
            {
                _logger.LogInformation("{message}", e.Message);
                return false;
            }
        }

    }
}

using Messenger.Services.DatabaseHelper;
using Messenger.SignUp.Models.Friends;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.Extensions.Caching.Distributed;
using System.Runtime.InteropServices;

namespace Messenger.Services.FriendServices
{
    public record FriendRequest(Guid userId, Guid userRequest);
    public interface IRedisFriendServices
    {
        Task<bool> SetUserIdHaveFriendRequestAsync(Guid userId);
        Task<string> GetFriendRequestRedisAsync(Guid userId);
        Task<(bool, string)> SetFriendRequest(Guid userId, Guid userReceiveRequest);
        Task<bool> SetUserWhenHandledRequest(Guid UserId);
        Task<(bool, string)> AcceptFriendRequest(Guid userId, Guid userReceiveRequest);
    }


    public class RedisFriendServices : IRedisFriendServices
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<FriendsContext> _logger;
        public RedisFriendServices(IDistributedCache cache, ILogger<FriendsContext> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        //this funciton will make add friend api send Request  
        //1 is for pending 
        public async Task<(bool, string)> SetFriendRequest(Guid userId ,Guid userReceiveRequest)
        {
            var key = $"FriendRequest:UserRequest:{userId}:UserReceive:{userReceiveRequest}";
            var value = "pending";
            try
            {
                var result = _cache.SetStringAsync(key, value.ToString());
                await result;
                return (true, "Add Friend Successfully");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{message}", e);
                return (false, "Error ");
            }
        }

        public async Task<(bool,string)> AcceptFriendRequest(Guid userId, Guid userReceiveRequest)
        {
            var key = $"FriendRequest:UserRequest:{userId}:UserReceive:{userReceiveRequest}";
            try
            {
                var result = await _cache.GetStringAsync(key);
                if (String.IsNullOrEmpty(result))
                {
                    return (false, "Cannot found this user please try again ");

                }
                 if(result == "accept")
                {
                    return (true, "You have  had friend with this account");
                }
                 var value = "accept";
                 var ChangeFriendStateInRedis =  _cache.SetStringAsync(key, value);
                await ChangeFriendStateInRedis;

                if (ChangeFriendStateInRedis.IsCompletedSuccessfully)
                {
                    return (true, "Accept Successfully");
                } else
                {
                    return (false, "Something wrong with this services");
                }
            }

            catch (Exception ex)
            {
                _logger.LogError(ex,"{message}",ex);
                return (false, "Something wrong happend when handle this services please try again");
            }
        }
        //this funcion will notify the user have new friend request 
        public async Task<bool> SetUserIdHaveFriendRequestAsync(Guid UserId)
        {
            try
            {
                var key = $"FriendRequest:{UserId}";
                await _cache.SetStringAsync(key, "true");
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError("{message}", e);
                return false;
            }
        }


        // this function will get all the request at redis 
        public async Task<string> GetFriendRequestRedisAsync(Guid userId)
        {
            try
            {
                var key = $"FriendRequest:UserId:{userId}"; 
                var result = await _cache.GetStringAsync(key);
                return result ?? String.Empty;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while getting friend request for user {UserId}", userId);
                throw;
            }
        }

     

        //this function will turn off the red dot when user have new request 
        public async Task<bool> SetUserWhenHandledRequest(Guid UserId)
        {
            var key = $"FriendRequest:{UserId}";
            try
            {
                var result = _cache.RemoveAsync(key);
                await result;
                if (result.IsCompletedSuccessfully)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception e)
            {
                _logger.LogError("{message}", e);
                return false;
            }
        }

    }
}
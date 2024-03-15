using Messenger.Services.ElasticServices;
using Messenger.Services.FriendServices;
using Messenger.SignUp.Models;
using Messenger.SignUp.Models.Friend;
using Messenger.SignUp.Models.Friends;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Security.AccessControl;

namespace Messenger.SearchHub
{
   
    public  class FriendHub : Hub
    {

        private readonly SignUpContext _ctx;
        private readonly FriendsContext _friendCtx;
        private readonly IRedisFriendServices _cache;
        private readonly IsHaveFriendContext _isHaveFriendContext;

        private readonly IElasticSearchUsers _elastic;
        public FriendHub(SignUpContext context, FriendsContext friendContext, RedisFriendServices cache, ElasticSearchUsers elastic, IsHaveFriendContext isHaveFriendContext )
        {
            _ctx = context;
            _friendCtx = friendContext;
            _cache = cache;
            _isHaveFriendContext = isHaveFriendContext;
            _elastic = elastic;
        }
       
        public async Task SearchingStranger(string searchValue)
            {
            var result = await _elastic.GetUserAsync(searchValue);
        

            await Clients.Caller.SendAsync("ReceiveMessage", result);
            }

        //This will check the friendRequest 
        public async Task IsHaveFriendRequest(Guid UserId)
        {
            // Should we added to redis
            //Nếu có 1 request đến thì đầu tiên add vào redis trước thì cái request này tương ứng với key là UserId:{Id}:RequestFriend  :  value Id của user đó 
            //Nếu check có trong redis có và value khác null thì oke return true 
            var cacheCheck = await _cache.GetFriendRequestRedisAsync(UserId);
            if (String.IsNullOrEmpty(cacheCheck))
            {
                var hasPendingFriendRequest = await _isHaveFriendContext.IsHaveFriends
                                               .AnyAsync(a => a.UserId == UserId && a.IsHaveFriend == 1);

                    if(hasPendingFriendRequest is false)
                {
                    await Clients.Caller.SendAsync("ReceiveMessage", false);
                }
                
            }
            await Clients.Caller.SendAsync("ReceiveMessage", true);







        }
       
    }


}

using Messenger.SearchHub;
using Messenger.Services.DatabaseHelper;
using Messenger.Services.FriendServices;
using Messenger.Services.SIgnUp;
using Messenger.SignUp.Models.Friend;
using Messenger.SignUp.Models.Friends;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace Messenger.Controllers
{
    [Route("Friends")]
    public class FriendController : ControllerBase
    {

        private readonly DbServices<FriendsContext> _ctx;
        private readonly ILogger<FriendsContext> _logger;
        private readonly IRedisFriendServices _cache;
        private readonly DbServices<IsHaveFriendContext> _isHaveFriendCheck; 
        public FriendController(DbServices<FriendsContext> ctx, ILogger<FriendsContext> logger, IRedisFriendServices cache, DbServices<IsHaveFriendContext> isHaveFriendCheck)
        {
            _ctx = ctx;
            _logger = logger;
            _cache = cache;
            _isHaveFriendCheck = isHaveFriendCheck;
        }

        public record FriendRequest(Guid UserRecieveRerquest , Guid UserSendRequest);
        [HttpPost("friendRequest")]
        //This will send request to adding friend 
        public async Task<ActionResult<Result>> SendMakingFriendRequest([FromBody] FriendRequest request)
        {
            try
            {
                
                var requestInfo = new Friend
                {
                    UserId = request.UserSendRequest,
                    FriendId = request.UserRecieveRerquest,
                    FriendStatus = 1    //pending 
                };
                var addValueToIsHaveFriendCheck = new IsHaveFriendCheck
                {
                    UserId = request.UserRecieveRerquest,
                    IsHaveFriend = 1
                };

                var cacheOperation = _cache.SetFriendRequest(request.UserSendRequest,request.UserRecieveRerquest);
                var dbFriendsOperation =  _ctx.CreateAsync(requestInfo);
                var IsHaveFriendCheckOperation =  _isHaveFriendCheck.CreateAsync(addValueToIsHaveFriendCheck);
                await  Task.WhenAll(cacheOperation, dbFriendsOperation, IsHaveFriendCheckOperation);
                return Ok(new Result { Success = new CommonSuccess("Đã gửi yêu cầu thành công ") });
            } catch (Exception e)
            {
                _logger.LogError("{message}", e);
                return BadRequest(new Result {Error = new CommonError( "Server-side error please refresh the page") });
            }
        }

        
        //    [HttpPost("FriendAccept")]
        //public async Task<ActionResult<Result>> AcceptFriendRequest([FromBody] FriendRequest request)
        //{
        //    try
        //    {
        //        var addfriend = await _cache.AcceptFriendRequest(request.UserSendRequest,request.UserRecieveRerquest);
        //        if (addfriend.Item1)
        //        {
        //            return Ok(new Result { Success = new CommonSuccess($"{addfriend.Item2}") });
        //        }else
        //        {
        //            var result =  
        //                 _logger.LogError($"{addfriend.Item2}");
        //            return BadRequest(new Result { Error = new($"{addfriend.Item2}") });
        //        }
        //    }catch(Exception e)
        //    {
        //        _logger.LogError(e, "{message}", e);
        //        return BadRequest(new Result { Error = new CommonError("Error happended please retry addfriend") });
        //    }

        //}
        

        











    }
}

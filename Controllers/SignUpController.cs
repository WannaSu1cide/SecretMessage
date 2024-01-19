using Messenger.SignUp.Models;
using Microsoft.AspNetCore.Mvc;
using BCryptNet = BCrypt.Net.BCrypt;
using SharingModel;
using System.Net.WebSockets;
using Microsoft.EntityFrameworkCore;


using Messenger.Services.SignUp;

using Messenger.Services.DatabaseHelper;

using Microsoft.Extensions.Caching.Distributed;
using Messenger.Services.SIgnUp;
using System.Threading;
using static Messenger.Controllers.SignUpController;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

namespace Messenger.Controllers
{

    public class Result
    {
        public CommonSuccess? Success { get; set; }

        public CommonError? Error { get; set; }

        public DateTime? TimeSent { get; set; }
        public DateTime? expriryTime { get; set; }

        public string Email { get; set; }

    }

    public record CommonSuccess(string SuccessMessage);
    public record CommonError(string ErrorMessage);


    [ApiController]
    [Route("api/authe")]

    
    public class SignUpController : ControllerBase
    {
     

        private readonly ILogger _logger;
     
        private readonly DbServices<SignUpContext> _ctx;

        private readonly IRedisOptions _redisOptions;
      
        public SignUpController(DbServices<SignUpContext> ctx, ILogger<SignUpContext> logger, IRedisOptions redisOptions,CancellationToken  cancellationToken = default)
        {
            _ctx = ctx;
            _logger = logger;
            _redisOptions = redisOptions;

        }

        [Route("signup")]
        [HttpPost]
        public async Task<ActionResult> SignUp([FromBody] UserDTO userInfomation)
        {
            try
            {
                if (!CheckSignUp.SignUpValidate(userInfomation))
                {
                    return BadRequest(new CommonError("Phiếu đăng ký không hợp lệ"));
                }
                var item = await _ctx.GetUserByEmailAsync<UserAccount>(userInfomation.Email);
                if (item != null)
                {
                    _logger.LogWarning($"Đã đăng ký tài khoản {userInfomation.Email}");
                    return Ok(new CommonError("Tài khoản này đã được đăng ký "));
                }
              
                var SendEmailResult = await SendEmailToAutheticate.SendAutheticateKeyToUser(userInfomation.Username, userInfomation.Email);
                var SendEmailSuccess = SendEmailResult.Item1;
                var autheticateNumber = SendEmailResult.Item2;
                if ( !SendEmailSuccess)
                {
                    _logger.LogError("K thể gửi được email");
                    return Ok(new Result { Error = new("Đã có lỗi xảy ra khi gửi email vui lòng thử lại") });
                }
                var resultOfUserInfo = new UserAccount
                {
                    Id = Guid.NewGuid(),
                    Email = userInfomation.Email,
                    Password = BCryptNet.HashPassword(userInfomation.Password),
                    Username = userInfomation.Username
                };


               
                    await _ctx.CreateAsync(resultOfUserInfo);
                    await _redisOptions.SetKeyValueForUserById(resultOfUserInfo.Id, autheticateNumber.ToString());
                _logger.LogInformation($"Tạo thành công tài khoản {userInfomation.Username}");
                return Ok(new Result { Success = new CommonSuccess("Tài khoản đã được tạo thành công mời bạn xác thực ở trong gmail") });
                
            }   
            catch (Exception e)
            {
                _logger.LogError(e, "Có lỗi xảy ra: {Message}", e.Message);
                return BadRequest(new CommonError("Đã có lỗi xảy ra "));
            }
        }

        public record AccountNeedSendMail(Guid Id, string username, string email);

        [Route("SendAutheticationKey")]
        [HttpPost]
        public async Task<ActionResult<Result>> SendAutheKeyToEmail([FromBody] AccountNeedSendMail user)
        {

            try
            {
                var result = await SendEmailToAutheticate.SendAutheticateKeyToUser(user.username, user.email);
                if (result.Item1)
                {
                    await _redisOptions.SetKeyValueForUserById(user.Id, result.Item2);
                    var expiry = DateTime.Now.AddMinutes(3);
                    return Ok(new Result { Success = new CommonSuccess("Mã xác nhận đã được chuyển lại mong bạn kiểm tra Email của mình"), TimeSent = DateTime.Now, expriryTime = expiry });
                }
                return BadRequest(new Result { Error = new CommonError("Đã có lỗi không thể gửi email cho bạn") });
                ;
            }
            catch (Exception e)
            {
                _logger.LogError("{message}", e.Message);
                return BadRequest(new Result { Error = new CommonError($"Something wrong with server {e.Message}") });
            }

        }



        public record AccountActivate(Guid Id, string code);
        [Route("AccountActivate")]
        [HttpPost]
        public async Task<ActionResult<Result>> ActivateAccount([FromBody] AccountActivate requestActivateAccount)
        {
            try
            {
                var codeStoreInCache = await _redisOptions.GetUserAutheKeyById(requestActivateAccount.Id);
                if (codeStoreInCache is null)
                {
                    _logger.LogInformation($"User {requestActivateAccount.Id} đã nhập mã hết hạn");
                    return BadRequest(new Result { Error = new CommonError("Mã đã hết hạn bạn vui lòng ấn gửi lại để có mã mới") });
                }

                if (codeStoreInCache == requestActivateAccount.code)
                {
                    await _ctx.UpdateAsync<int, UserAccount>(requestActivateAccount.Id, 1, "AccountActive");
                    return Ok(new Result { Success = new CommonSuccess("Tài khoản của bạn đã được kích hoạt thành công !") });
                }

                return BadRequest(new Result { Error = new CommonError("Mã code sai mong bạn vui lòng thử lại ") });
            }catch(Exception e)
            {
                _logger.LogError("{message}", e.Message);
                return BadRequest("Đã có lỗi xảy ra với server");
            }
         
            
         }





    }





}







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
using Microsoft.AspNetCore.Authorization;

using Messenger.Services.ElasticServices;
using Nest;
using Microsoft.Extensions.Logging.Abstractions;

namespace Messenger.Controllers
{

    public class Result
    {
        public CommonSuccess? Success { get; set; } = null; 

        public CommonError? Error { get; set; } = null; 

        public DateTime? TimeSent { get; set; }
        public DateTime? expriryTime { get; set; }


    }

    public record CommonSuccess(string SuccessMessage);
    public record CommonError(string ErrorMessage);

   

    [ApiController]
    [Route("api/authe")]

    
    public class SignUpController : ControllerBase
    {
     

        private readonly ILogger<SignUpController> _logger;
     
        private readonly DbServices<SignUpContext> _ctx;

        private readonly IRedisOptions _redisOptions;
        private readonly IElasticSearchUsers _elastic;
        private readonly ICreateObject _object; 
        public SignUpController(DbServices<SignUpContext> ctx, ILogger<SignUpController> logger, IRedisOptions redisOptions, ICreateObject obj , IElasticSearchUsers client)
        {
            _ctx = ctx;
            _logger = logger;
            _redisOptions = redisOptions;
            _object = obj;
            _elastic = client;

        }

        [Route("signup")]
        [HttpPost]
        public async Task<ActionResult> SignUp([FromBody] UserDTO userInformation)
        {
            try
            {
                if (!CheckSignUp.SignUpValidate(userInformation))
                {
                    return BadRequest(new CommonError("Phiếu đăng ký không hợp lệ"));
                }

                // Check if user already exists
                var existingUser = await _ctx.GetUserByEmailAsync<UserAccount>(userInformation.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning($"Đã đăng ký tài khoản {userInformation.Email}");
                    return Ok(new CommonError("Tài khoản này đã được đăng ký "));
                }

                // Send email asynchronously
                var sendEmailResult = await SendEmailToAutheticate.SendAutheticateKeyToUser(userInformation.Username, userInformation.Email);
                var sendEmailSuccess = sendEmailResult.Item1;
                var authenticateNumber = sendEmailResult.Item2;
                if (!sendEmailSuccess)
                {
                    _logger.LogError("Không thể gửi được email");
                    return Ok(new Result { Error = new("Đã có lỗi xảy ra khi gửi email vui lòng thử lại") });
                }

                // Create user account
                var userAccount = _object.CreateObjectForDb(userInformation);

                
                Guid userId = userAccount.Id;

               
                var userRepo = new UserRepo
                {
                    Id = userId,
                    Username = userInformation.Username
                };

                var dbOperationTask = _ctx.CreateAsync(userAccount);
                var elasticOperationTask = _elastic.AddUserAsync(userRepo);
                var redisOperationTask = _redisOptions.SetKeyValueHaveTimeSpan(userId, authenticateNumber.ToString(), 3);

                await Task.WhenAll(dbOperationTask, elasticOperationTask, redisOperationTask);

                _logger.LogInformation($"Tạo thành công tài khoản {userInformation.Username}");

                return Ok(new Result { Success = new CommonSuccess("Tài khoản đã được tạo thành công. Mời bạn xác thực ở trong gmail"), expriryTime = DateTime.UtcNow.AddMinutes(3) });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Có lỗi xảy ra: {Message}", e.Message);
                return BadRequest(new CommonError("Đã có lỗi xảy ra " + e.Message));
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
                    await _redisOptions.SetKeyValueHaveTimeSpan(user.Id, result.Item2,3);
                    var expiry = DateTime.UtcNow.AddMinutes(3);
                    return Ok(new Result { Success = new CommonSuccess("Mã xác nhận đã được chuyển lại mong bạn kiểm tra Email của mình"), TimeSent = DateTime.UtcNow, expriryTime = expiry });
                }
                return BadRequest(new Result { Error = new CommonError("Đã có lỗi không thể gửi email cho bạn") });
                
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
                var codeStoreInCache = await _redisOptions.GetUserAutheKeyById(requestActivateAccount.Id)!;
                
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







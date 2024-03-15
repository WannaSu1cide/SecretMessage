using Messenger.Services.DatabaseHelper;
using Messenger.Services.SIgnUp;
using Messenger.SignUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Messenger.Services.SignIn;
using Microsoft.AspNetCore.Authorization;

namespace Messenger.Controllers
{
    public class AutheticationResult
    {
        public string? error { get; set; }

        public string? Token { get; set; }


    }
    public record EmailAndPassword(string email, string password);
   

    [ApiController]
    [Route("api/authe")]
    public class SignInController : ControllerBase  
    {


        private readonly ILogger<SignUpContext> _logger;


        private readonly IRedisOptions _redisOptions;

        private readonly CheckSignIn _signIn; 
      

        public SignInController(ILogger<SignUpContext> logger, IRedisOptions redisOptions, CheckSignIn signIn, CancellationToken cancellationToken = default)
        {
            
            _logger = logger;
            _redisOptions = redisOptions;
            _signIn = signIn;

        }
        [AllowAnonymous]
        [Route("sign-in")]
        [HttpPost]
        public async Task<ActionResult<AutheticationResult>> Authetication([FromBody] EmailAndPassword user)
        {
            try
            {
                var checkSignIn = await _signIn.SignInValidate(user);
                if (!checkSignIn.IsSuccess)
                {
                    _logger.LogInformation($"Email {user.email} are failed sign in ");
                    Console.WriteLine(checkSignIn.ErrorMessage);
                    return BadRequest(new AutheticationResult { error = checkSignIn.ErrorMessage, Token = null });
                }
                var UserInfo = new UserInfo(checkSignIn.User.Id.ToString(), checkSignIn.User.Email, checkSignIn.User.Username);
                string token = JWTToken.CreateToken(UserInfo);
                return Ok(new AutheticationResult { error = null ,  Token = token });

            }catch(Exception e)
            {
                _logger.LogInformation("Error with Server was in Sign-In Controller  : {message}", e.Message);
                return BadRequest(new AutheticationResult { error = "Something wrong with server"});
            }
        }
    }
}


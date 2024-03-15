using Messenger.Controllers;
using Messenger.Services.DatabaseHelper;
using Messenger.SignUp.Models;
using SharingModel;
using System.Threading.Tasks;

namespace Messenger.Services.SignIn
{
    public record SignInResult(bool IsSuccess , UserAccount? User , string? ErrorMessage);

    public class CheckSignIn
    {
        private readonly DbServices<SignUpContext> _ctx;

        public CheckSignIn(DbServices<SignUpContext> ctx)
        {
            _ctx = ctx;
        }

        public async Task<SignInResult> SignInValidate(EmailAndPassword user)
        {
            if (string.IsNullOrEmpty(user.email) || string.IsNullOrEmpty(user.password))
            {
                return new SignInResult(false, null, "Please fill in all input");
            }

            var result = await _ctx.GetUserByEmailAsync<UserAccount>(user.email);

            Console.WriteLine(result);
            if (result == null)
            {
                return new SignInResult(false, null, "Email or Password is incorrect");
            }

            if (result.AccountActive != 1)
            {
                return new SignInResult(false, null, "This account is not active");
            }

            if (BCrypt.Net.BCrypt.Verify(user.password, result.Password))
            {
                return new SignInResult(true, result, null);
            }
            else
            {
                return new SignInResult(false, null, "Email or Password is incorrect");
            }
        }
    }
}

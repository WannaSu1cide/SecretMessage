using SharingModel;

namespace Messenger.Services.SignUp
{
    public static class CheckSignUp
    {
        public static bool SignUpValidate(UserDTO user)
        {
            return user.Email.Contains("@")
                    && user.Email.Contains(".")
                    && user.Password.Length > 8
                    && user.ConfirmPassword == user.Password
                    && user.Username.Length > 8;
        }
    }
}

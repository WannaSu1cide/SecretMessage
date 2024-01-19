using SendGrid.Helpers.Mail;
using SendGrid;


namespace Messenger.Services.SignUp
{
    public static class SendEmailToAutheticate
    {
        
      
        public static async Task<(bool,string)> SendAutheticateKeyToUser(string username, string userEmail)
        {

            try
            {
                var autheticateNumber = RandomGeneratedNumber();
                var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
                var client = new SendGridClient(apiKey);
                var msg = new SendGridMessage()
                {
                    From = new EmailAddress("ndanh1012@gmail.com", "SercretMessage"),
                    Subject = "Xác thực tài khoản người dùng",
                    PlainTextContent = $"Chào {username}\n" +
                    "Chúng tôi rất cảm ơn bạn vì đã đăng ký vào dịch vụ của chúng tôi!\n" +
                    $" Đây là mã xác thực của bạn : {autheticateNumber}\n" +
                    "Vui lòng không cung cấp mã này cho bất kì ai để tránh rủi ro lộ thông tin cá nhân của bạn\n" +
                    "Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi"
                };  
                msg.AddTo(new EmailAddress($"{userEmail}", $"{username}"));
                var response = await client.SendEmailAsync(msg);


                if (response.IsSuccessStatusCode)
                {
                    return (true, autheticateNumber);
                }
                else
                {
                    return (false, "Cannot send mail to user");
                }
            }
            catch
            {
                return (false,"Something wrong with server");
            }

        }
        public static string RandomGeneratedNumber()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
  
}


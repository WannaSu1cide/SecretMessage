using Messenger.SignUp.Models;
using Microsoft.CodeAnalysis.FlowAnalysis;
using SharingModel;
using Newtonsoft.Json.Converters;
using BCryptNet = BCrypt.Net.BCrypt;
using Newtonsoft.Json;
using Messenger.Services.ElasticServices;
using NuGet.Configuration;

namespace Messenger.Services.SIgnUp
{
  

    public interface ICreateObject
    {
        UserAccount CreateObjectForDb(UserDTO userInfomation);

        //object CreateObjectForElastic(UserRepo user);
    }
    public class CreateObject : ICreateObject   
    {

        public UserAccount CreateObjectForDb(UserDTO userInfomation)
        {
           
            var resultOfUserInfo = new UserAccount
            {
                Id = Guid.NewGuid(),
                Email = userInfomation.Email,
                Password = BCryptNet.HashPassword(userInfomation.Password),
                Username = userInfomation.Username
            };

            return resultOfUserInfo;
        }


        //public object CreateObjectForElastic(UserRepo user)
        //{

        //    if( user == null || user.Username== null)

        //    {
        //        throw new ArgumentNullException(nameof(user.Username), "User information cannot be null.");

        //    }

        //    return new UserRepo
        //    {
        //        Id = user.Id,
        //        Username = user.Username
        //    };
        //}
    }
}

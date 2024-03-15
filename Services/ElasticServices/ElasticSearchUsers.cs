using Nest;
using NuGet.Protocol;
using System.Configuration;
using Newtonsoft.Json.Converters;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Messenger.SignUp.Models;
using Messenger.Services.SIgnUp;
using SharingModel;
namespace Messenger.Services.ElasticServices
{
    public record User(string username);





     public interface IElasticSearchUsers
    {

        Task<List<User>> GetUserAsync(string username);
        Task<(bool, string)> AddUserAsync(UserRepo user);

        
         
    }
    public class ElasticSearchUsers  : IElasticSearchUsers
    {
        private ElasticClient _client = CreateElasticSearch.Create();
        private readonly ILogger<SignUpContext> _logger;
        private readonly ICreateObject _object; 
        public ElasticSearchUsers( ILogger<SignUpContext> logger,ICreateObject obj)
        {
          
            _logger = logger;
            _object = obj;
        }

        public async Task<List<User>> GetUserAsync(string username)
        {
            try
            {
                if (username == null) return null;


                var response = await _client.SearchAsync<User>(s => s.Query(q => q.Match(m => m.Field("username").Query(username))).Size(10));
                if (response.IsValid)
                {
                    var result = response.Hits.Select(s => s.Source).OrderByDescending(s => s.username).ToList();
                    return result;

                }
                _logger.LogInformation("Khong timn thay username");
                return null;
            }catch(Exception e)
            {
                _logger.LogError("{message}", e.Message);
                return  null;

            }
          
        }


        public async Task<(bool,string)> AddUserAsync(UserRepo user)
        {

            if (user == null  ) 
            {
                _logger.LogError("No content to add to ElasticSearch");
                return (false, "Something happen with user");
                
            }

            try
            {
                var ExsistIndexResponseAsync = await _client.Indices.ExistsAsync("users");
                if (!ExsistIndexResponseAsync.Exists)
                {
                    var CreateIndexAsync = await _client.Indices.CreateAsync("users",c => c.Map<UserRepo>(m => m.AutoMap()));
                    if (!CreateIndexAsync.IsValid)
                    {
                        _logger.LogError("Failed to create index 'users' in Elasticsearch. Error: {error}", CreateIndexAsync.ServerError?.ToString() ?? "Unknown error");
                        return (false, "Something have been wrong please press F5 to refresh the page");
                    }
                }


                var response = await _client.IndexAsync(user,idx => idx.Index("users")); 
                if (response.IsValid)
                {
                    return (true,"Success");
                }
                else
                {
                    _logger.LogError("{message}", response.OriginalException.Message);
                    return (false, response.OriginalException.Message);
                }

            }catch(Exception e)
            {
                
                _logger.LogError("{message} something wrong with Server when added user value into elastic search ", e.Message);
                return (false,"Đã có lỗi xảy ra với server");
            }
           
        }
      


    }
}

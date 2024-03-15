using Microsoft.Extensions.Configuration;

using Serilog;
using Microsoft.EntityFrameworkCore;
using Messenger.Services.DatabaseHelper;
using StackExchange.Redis;




using Messenger.SignUp.Models;
using Messenger.Services.SIgnUp;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Messenger.SearchHub;
using Messenger.SignUp.Models.Friends;
using Messenger.Services.FriendServices;
using Messenger.SignUp.Models.Friend;
using Nest;
using Messenger.Services.ElasticServices;
using Messenger.Services.SignIn;
using Microsoft.AspNetCore.Authentication.Cookies;
using Messenger.UserRol.UserRoleDb;




namespace Messenger
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
           

           
            var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();


            var connectionString = Environment.GetEnvironmentVariable("Messenger-Database");


            //register  DbContext to working with db 
            builder.Services.AddDbContextPool<SignUpContext>(options => options.UseSqlServer(connectionString))
                            .AddDbContextPool<FriendsContext>(options => options.UseSqlServer(connectionString))
                            .AddDbContextPool<IsHaveFriendContext>(options => options.UseSqlServer(connectionString))
                            .AddDbContextPool<UserRoleContext>(options => options.UseSqlServer(connectionString));

            var result = Environment.GetEnvironmentVariables();
            var issuser = result["Messenger:Issuer"].ToString();
            var audience = result["Messenger:Audience"].ToString();
            var key = result["JWT:Messenger:Key"].ToString();
            var cache = result["Redis-Messenger"].ToString();
        

            Console.Write($"ConnectionString :  {connectionString} + CacheString: {cache}");


            builder.Services.AddStackExchangeRedisCache(x =>
            {
                x.Configuration = cache;

            }
                
            );
           
            builder.Services.AddScoped<IRedisOptions, RedisOptions>();
            builder.Services.AddScoped<IRedisFriendServices, RedisFriendServices>();
            builder.Services.AddScoped<CheckSignIn>();
            Log.Logger = new LoggerConfiguration()
           .ReadFrom.Configuration(configuration)
           .WriteTo.File("D:\\MessageLogging\\log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

            builder.Host.UseSerilog();
            //builder.Services.AddScoped<IRandomGeneratedNumber, GeneratedToken>();
            builder.Services.AddScoped(typeof(DbServices<>));
            builder.Services.AddScoped<ICreateObject,CreateObject>();
            builder.Services.AddScoped<IElasticSearchUsers,ElasticSearchUsers>();
;
            //builder.Services.AddAuthentication().AddCookie();
            builder.Services.AddControllers();


      
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

         
           


            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
         
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = issuser,
                    ValidAudience = audience ,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true

                };
            });

            builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
            }

            );


            //builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            //{
            //    options.Cookie.Name = 
            //})
            builder.Services.AddSignalR();
            builder.Services.AddAuthorization();

            var app = builder.Build();
          
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(); 
            }
            app.UseRouting();
            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

           
            app.UseEndpoints(epoints =>
            {
                epoints.MapControllers();

                app.MapHub<FriendHub>("Friends");
            });
            
        
            app.Run();
        }
    }
}
    
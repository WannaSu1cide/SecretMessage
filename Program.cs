using Microsoft.Extensions.Configuration;

using Serilog;
using Microsoft.EntityFrameworkCore;
using Messenger.Services.DatabaseHelper;
using StackExchange.Redis;
using Microsoft.AspNetCore.Identity;

using Microsoft.Extensions.Options;


using Messenger.SignUp.Models;
using Messenger.Services.SIgnUp;



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


           
            builder.Services.AddDbContext<SignUpContext>(options =>
                options.UseSqlServer(connectionString));





            var portCache = Environment.GetEnvironmentVariable("Redis-Messenger");



            builder.Services.AddStackExchangeRedisCache(x =>
            {
                x.Configuration = portCache;

            }
                
            );

            builder.Services.AddScoped<IRedisOptions, RedisOptions>();


            Log.Logger = new LoggerConfiguration()
           .ReadFrom.Configuration(configuration)
           .WriteTo.File("D:\\MessageLogging\\log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

            builder.Host.UseSerilog();
            //builder.Services.AddScoped<IRandomGeneratedNumber, GeneratedToken>();
            builder.Services.AddScoped(typeof(DbServices<>));


            //builder.Services.AddAuthentication().AddCookie();
            builder.Services.AddControllers();

          
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
         

            var app = builder.Build();
          
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
          

            app.UseHttpsRedirection();

            app.UseAuthorization();
            app.UseCors();

         
            app.MapControllers();

            app.Run();
        }
    }
}
    
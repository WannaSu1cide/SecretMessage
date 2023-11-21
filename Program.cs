
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SecretMessageApi.Models;
using Serilog;
namespace SecretMessageApi
{

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.


            builder.Host.UseSerilog((context, configuration) => configuration
             .ReadFrom.Configuration(context.Configuration));

            builder.Services.AddDbContext<SecretMessageContext>(options =>
             options.UseSqlServer(builder.Configuration.GetConnectionString("LogginDB")));

            
            builder.Services.AddControllers();
      
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
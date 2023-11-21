using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SecretMessageApi.Models;



namespace SecretMessageApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserAccountController : ControllerBase
    {
        private readonly SecretMessageContext _context;
        private readonly ILogger _logger;
        public UserAccountController(SecretMessageContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }


        








    }
}
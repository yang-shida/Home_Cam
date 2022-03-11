using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Home_Cam_Backend.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Home_Cam_Backend.Controllers
{
    [ApiController]
    [Route("api/system")]
    public class SystemController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public SystemController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpPost("restart")]
        public ActionResult<string> test(AuthDto authInfo)
        {
            using (SHA256 mySHA256 = SHA256.Create())
            {
                byte[] encodedPwd = Encoding.UTF8.GetBytes(authInfo.pwd);
                byte[] hashedPwd = mySHA256.ComputeHash(encodedPwd);
                string storedPwd = configuration.GetSection("Authentication").GetValue<string>("ServerHashedPwd");
                byte[] storedHashedPwd = Extensions.hexStrToByteArray(storedPwd);
                
                if (hashedPwd.SequenceEqual(storedHashedPwd))
                {
                    Program.Restart();
                    return Ok("Server program restarted.");
                }
                else
                {
                    return BadRequest("Incorrect password!");
                }
            }
        }
    }
}
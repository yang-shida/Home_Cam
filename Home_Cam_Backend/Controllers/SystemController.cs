using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Home_Cam_Backend.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Home_Cam_Backend.Controllers
{
    [ApiController]
    [Route("api/system")]
    public class SystemController : ControllerBase
    {
        private readonly IConfiguration configuration;

        private bool CheckPwd(string pwd)
        {
            using (SHA256 mySHA256 = SHA256.Create())
            {
                byte[] encodedPwd = Encoding.UTF8.GetBytes(pwd);
                byte[] hashedPwd = mySHA256.ComputeHash(encodedPwd);
                string storedPwd = configuration.GetSection("Authentication").GetValue<string>("ServerHashedPwd");
                byte[] storedHashedPwd = Extensions.hexStrToByteArray(storedPwd);

                if (hashedPwd.SequenceEqual(storedHashedPwd))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public SystemController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpPost("restart")]
        public ActionResult<string> restart(AuthDto authInfo)
        {
            bool pwdIsRight = CheckPwd(authInfo.CurrPwd);
            if (pwdIsRight)
            {
                Program.Restart();
                return Ok("Server program restarted.");
            }
            else
            {
                return BadRequest("Incorrect password!");
            }
        }

        [HttpPost("check-pwd")]
        public ActionResult<string> CheckPwd(AuthDto authInfo)
        {
            bool pwdIsRight = CheckPwd(authInfo.CurrPwd);
            if (pwdIsRight)
            {
                return Ok("Server program restarted.");
            }
            else
            {
                return BadRequest("Incorrect password!");
            }
        }

        [HttpGet]
        public ActionResult<SystemSettingsDto> getSettings()
        {
            string jsonStr = System.IO.File.ReadAllText("appsettings.json");
            JObject jsonObj = JObject.Parse(jsonStr);

            SystemSettingsDto res = new()
            {
                MaxSpaceGBs = (int)jsonObj["ImageStorageSettings"]["MaxSpaceGBs"],
                PercentToDeleteWhenFull = (int)jsonObj["ImageStorageSettings"]["PercentToDeleteWhenFull"],
                SearchCamerasMinutes = (int)jsonObj["BackgroundTasksTimingSettings"]["SearchCamerasMinutes"],
                ImageStorageSizeControlMinutes = (int)jsonObj["BackgroundTasksTimingSettings"]["ImageStorageSizeControlMinutes"]
            };

            return Ok(res);
        }

        [HttpGet("logs")]
        public async Task GetLogs()
        {
            var response = Response;
            response.Headers.Add("Content-Type", "text/event-stream");

            string currentLog = System.IO.File.ReadAllText(@configuration.GetSection("BackendLog").GetValue<string>("LogFilePath"));
            currentLog = currentLog.Replace("\r\n", "<br>");
            currentLog = currentLog.Substring(0, currentLog.LastIndexOf("<br>"));
            await response.Body.WriteAsync(Encoding.ASCII.GetBytes($"data: {currentLog}\r\n\r\n"));
            await response.Body.FlushAsync();

            while(!Request.HttpContext.RequestAborted.IsCancellationRequested)
            {
                Extensions.tcs = new();
                Task completedTask = await Task.WhenAny(Extensions.tcs.Task, Task.Delay(2000));
                if(completedTask==Extensions.tcs.Task)
                {
                    string newMsg = await Extensions.tcs.Task;
                    await response.Body.WriteAsync(Encoding.ASCII.GetBytes($"data: {newMsg}\r\n\r\n"));
                    await response.Body.FlushAsync();
                }
                
            }
        }

        [HttpPut]
        public ActionResult<string> changeSettings(SystemSettingsDto newSettings)
        {
            string jsonStr = System.IO.File.ReadAllText("appsettings.json");
            JObject jsonObj = JObject.Parse(jsonStr);

            if (newSettings.MaxSpaceGBs != (int)jsonObj["ImageStorageSettings"]["MaxSpaceGBs"])
            {
                int highBound = 50, lowBound = 1;
                if (newSettings.MaxSpaceGBs >= lowBound && newSettings.MaxSpaceGBs <= highBound)
                {
                    jsonObj["ImageStorageSettings"]["MaxSpaceGBs"] = newSettings.MaxSpaceGBs.ToString();
                }
                else
                {
                    return BadRequest($"MaxSpaceGBs needs to be between {lowBound} and {highBound}");
                }
            }

            if (newSettings.PercentToDeleteWhenFull != (int)jsonObj["ImageStorageSettings"]["PercentToDeleteWhenFull"])
            {
                int highBound = 90, lowBound = 5;
                if (newSettings.PercentToDeleteWhenFull >= lowBound && newSettings.PercentToDeleteWhenFull <= highBound)
                {
                    jsonObj["ImageStorageSettings"]["PercentToDeleteWhenFull"] = newSettings.PercentToDeleteWhenFull.ToString();
                }
                else
                {
                    return BadRequest($"PercentToDeleteWhenFull needs to be between {lowBound} and {highBound}");
                }
            }

            if (newSettings.SearchCamerasMinutes != (int)jsonObj["BackgroundTasksTimingSettings"]["SearchCamerasMinutes"])
            {
                int highBound = 60, lowBound = 1;
                if (newSettings.SearchCamerasMinutes >= lowBound && newSettings.SearchCamerasMinutes <= highBound)
                {
                    jsonObj["BackgroundTasksTimingSettings"]["SearchCamerasMinutes"] = newSettings.SearchCamerasMinutes.ToString();
                }
                else
                {
                    return BadRequest($"SearchCamerasMinutes needs to be between {lowBound} and {highBound}");
                }
            }

            if (newSettings.ImageStorageSizeControlMinutes != (int)jsonObj["BackgroundTasksTimingSettings"]["ImageStorageSizeControlMinutes"])
            {
                int highBound = 30, lowBound = 1;
                if (newSettings.ImageStorageSizeControlMinutes >= lowBound && newSettings.ImageStorageSizeControlMinutes <= highBound)
                {
                    jsonObj["BackgroundTasksTimingSettings"]["ImageStorageSizeControlMinutes"] = newSettings.ImageStorageSizeControlMinutes.ToString();
                }
                else
                {
                    return BadRequest($"ImageStorageSizeControlMinutes needs to be between {lowBound} and {highBound}");
                }
            }

            string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);

            System.IO.File.WriteAllText("appsettings.json", output);

            return Ok("Settings updated");
        }
    }
}
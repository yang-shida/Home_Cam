using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Home_Cam_Backend.BackgroundTasks;
using Home_Cam_Backend.Dtos;
using Home_Cam_Backend.Entities;
using Home_Cam_Backend.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Home_Cam_Backend.Controllers
{
    [ApiController]
    [Route("api/camSettings")]
    public class CamSettingsController : ControllerBase
    {
        private readonly ICamSettingsRepository repository;

        public CamSettingsController(ICamSettingsRepository repo)
        {
            this.repository = repo;
        }

        [HttpGet("{uniqueId}")]
        public async Task<ActionResult<CamSettingDto>> GetCamSettingAsync(string uniqueId, string ipAddr = null, long? camTime = null)
        {
            uniqueId = uniqueId.restoreMacAddr();

            // request comes from a camera, add it to ActiveCameras list if is not in the list
            if (ipAddr is not null && camTime is not null)
            {
                Extensions.WriteToLogFile($"[{DateTime.Now.ToString("MM/dd/yyyy-hh:mm:ss")}] GetCamSettingAsync with MAC = {uniqueId}, IP = {ipAddr ?? "Null"} and camTime = {camTime ?? -1}");
                if(CamController.ActiveCameras.Find(camInList => camInList.UniqueId==uniqueId) is null)
                {
                    Esp32Cam tempCam = new Esp32Cam(ipAddr, uniqueId, (long)camTime);
                    await tempCam.Streaming();
                    CamController.ActiveCameras.Add(tempCam);
                }
                else
                {
                    int camIndex = CamController.ActiveCameras.IndexOf(CamController.ActiveCameras.Find(camInList => camInList.UniqueId == uniqueId));
                    if (CamController.ActiveCameras[camIndex].IpAddr != ipAddr)
                    {
                        CamController.ActiveCameras[camIndex].IpAddr = ipAddr;
                        await CamController.ActiveCameras.Last().Streaming();
                    }
                }

            }

            var camSetting = await repository.GetCamSettingAsync(uniqueId);
            // if this is a new camera, create a new setting entry and return the default
            if (camSetting is null)
            {
                if (ipAddr is not null && camTime is not null)
                {
                    EEsp32CamSetting defaultCamSetting = new()
                    {
                        UniqueId = uniqueId,
                        Location = "Default Location",
                        FrameSize = 10,
                        FlashLightOn = false,
                        HorizontalMirror = false,
                        VerticalMirror = false
                    };
                    await repository.CreateCamSettingAsync(defaultCamSetting);

                    return defaultCamSetting.AsDto();
                }
                else
                {
                    return NotFound();
                }

            }
            // return existing setting
            else
            {
                return camSetting.AsDto();
            }
        }

        [HttpPost]
        public async Task<ActionResult<CamSettingDto>> CreateCamSettingAsync(CamSettingDto camSettingDto)
        {
            // check if the camera already exists
            var existingCamSetting = await repository.GetCamSettingAsync(camSettingDto.UniqueId);
            if (existingCamSetting is not null)
            {
                return Conflict($"{camSettingDto.UniqueId} already exists!");
            }

            // create new
            EEsp32CamSetting camSetting = new()
            {
                UniqueId = camSettingDto.UniqueId,
                Location = camSettingDto.Location,
                FrameSize = camSettingDto.FrameSize,
                FlashLightOn = camSettingDto.FlashLightOn,
                HorizontalMirror = camSettingDto.HorizontalMirror,
                VerticalMirror = camSettingDto.VerticalMirror
            };

            await repository.CreateCamSettingAsync(camSetting);

            return camSetting.AsDto();
        }

        [HttpPut]
        public async Task<ActionResult<CamSettingDto>> UpdateCamSettingAsync(CamSettingDto camSettingDto)
        {
            // check if the cam exists
            var existingCamSetting = await repository.GetCamSettingAsync(camSettingDto.UniqueId);
            if (existingCamSetting is null)
            {
                return NotFound($"{camSettingDto.UniqueId} does not exist!");
            }

            EEsp32CamSetting camSetting = new()
            {
                UniqueId = camSettingDto.UniqueId,
                Location = camSettingDto.Location,
                FrameSize = camSettingDto.FrameSize,
                FlashLightOn = camSettingDto.FlashLightOn,
                HorizontalMirror = camSettingDto.HorizontalMirror,
                VerticalMirror = camSettingDto.VerticalMirror
            };

            await repository.UpdateCamSettingAsync(camSetting);

            int camIndex = CamController.ActiveCameras.FindIndex(cam=>cam.UniqueId==camSettingDto.UniqueId);
            if(camIndex!=-1)
            {
                await CamController.ActiveCameras[camIndex].UpdateAllSettings(camSetting);
            }

            return camSetting.AsDto();


        }
    }
}
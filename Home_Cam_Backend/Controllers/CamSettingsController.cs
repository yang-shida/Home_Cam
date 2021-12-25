using System.Threading.Tasks;
using Home_Cam_Backend.Dtos;
using Home_Cam_Backend.Entities;
using Home_Cam_Backend.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Home_Cam_Backend.Controllers
{
    [ApiController]
    [Route("api/camSettings")]
    public class CamSettingsController: ControllerBase
    {
        private readonly ICamSettingsRepository repository;

        public CamSettingsController(ICamSettingsRepository repo)
        {
            this.repository=repo;
        }

        [HttpGet("{uniqueId}")]
        public async Task<ActionResult<CamSettingDto>> GetCamSettingAsync(string uniqueId, string ipAddr=null)
        {
            // request comes from a camera, add it to ActiveCameras list if is not in the list
            if(ipAddr is not null && CamController.ActiveCameras.Find(camInList => camInList.UniqueId==uniqueId) is null)
            {
                CamController.ActiveCameras.Add(new Esp32Cam(ipAddr, uniqueId));
            }

            var camSetting = await repository.GetCamSettingAsync(uniqueId);
            // if this is a new camera, create a new setting entry and return the default
            if(camSetting is null)
            {
                EEsp32CamSetting defaultCamSetting = new()
                {
                    UniqueId=uniqueId,
                    Location="Default Location",
                    FrameSize=6,
                    FlashLightOn=false,
                    HorizontalMirror=false,
                    VerticalMirror=false
                };
                await repository.CreateCamSettingAsync(defaultCamSetting);

                return defaultCamSetting.AsDto();
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
            if(existingCamSetting is not null)
            {
                return Conflict($"{camSettingDto.UniqueId} already exists!");
            }

            // create new
            EEsp32CamSetting camSetting = new()
            {
                UniqueId=camSettingDto.UniqueId,
                Location=camSettingDto.Location,
                FrameSize=camSettingDto.FrameSize,
                FlashLightOn=camSettingDto.FlashLightOn,
                HorizontalMirror=camSettingDto.HorizontalMirror,
                VerticalMirror=camSettingDto.VerticalMirror
            };

            await repository.CreateCamSettingAsync(camSetting);

            return camSetting.AsDto();
        }

        [HttpPut]
        public async Task<ActionResult<CamSettingDto>> UpdateCamSettingAsync(CamSettingDto camSettingDto)
        {
            // check if the cam exists
            var existingCamSetting = await repository.GetCamSettingAsync(camSettingDto.UniqueId);
            if(existingCamSetting is null)
            {
                return NotFound($"{camSettingDto.UniqueId} does not exist!");
            }

            EEsp32CamSetting camSetting = new()
            {
                UniqueId=camSettingDto.UniqueId,
                Location=camSettingDto.Location,
                FrameSize=camSettingDto.FrameSize,
                FlashLightOn=camSettingDto.FlashLightOn,
                HorizontalMirror=camSettingDto.HorizontalMirror,
                VerticalMirror=camSettingDto.VerticalMirror
            };

            await repository.UpdateCamSettingAsync(camSetting);

            return camSetting.AsDto();
            

        }
    }
}
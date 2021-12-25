using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home_Cam_Backend.Dtos;
using Home_Cam_Backend.Entities;
using Home_Cam_Backend.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Home_Cam_Backend.Controllers
{
    [ApiController]
    [Route("api/cam")]
    public class CamController: ControllerBase
    {
        private readonly ICamSettingsRepository repository;
        public static List<Esp32Cam> ActiveCameras=new();

        public CamController(ICamSettingsRepository repo)
        {
            this.repository=repo;
        }

        [HttpGet]
        public async Task<ActionResult<List<CamDto>>> GetActiveCameras()
        {
            List<Esp32Cam> camList = await Esp32Cam.FindCameras();

            foreach(Esp32Cam cam in camList)
            {
                // if camera is not in the active list
                if(ActiveCameras.Find(camInList=>camInList.UniqueId==cam.UniqueId) is null)
                {
                    var camSetting = await repository.GetCamSettingAsync(cam.UniqueId);
                    // if the camera's setting is in the database
                    if(camSetting is not null)
                    {
                        // update the camera's setting
                        await cam.UpdateAllSettings(camSetting);
                    }
                    // camera's setting is not in the databse
                    else
                    {
                        // create a default camera setting and update the camera setting
                        camSetting = new()
                        {
                            UniqueId=cam.UniqueId,
                            Location="Default Location",
                            FrameSize=6,
                            FlashLightOn=false,
                            HorizontalMirror=false,
                            VerticalMirror=false
                        };
                        await cam.UpdateAllSettings(camSetting);
                        await repository.CreateCamSettingAsync(camSetting);
                    }

                    // add the camera to active list
                    ActiveCameras.Add(cam);
                }
            }

            return ActiveCameras.Select(cam=>cam.AsDto()).ToList();
        }

        [HttpGet("{camId}")]
        public async Task<ActionResult<string>> Streaming(string camId, bool start)
        {
            if(start)
            {
                return $"{camId} streaming started!";
            }
            else
            {
                return $"{camId} streaming closed!";
            }
        }

    }
}
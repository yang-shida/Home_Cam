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
            List<Esp32Cam> camList = await Esp32Cam.FindCameras(repository);

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
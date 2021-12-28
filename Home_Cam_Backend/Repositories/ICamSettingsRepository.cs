using System.Threading.Tasks;
using Home_Cam_Backend.Entities;

namespace Home_Cam_Backend.Repositories
{
    public interface ICamSettingsRepository
    {
        Task CreateCamSettingAsync(EEsp32CamSetting setting);
        Task<EEsp32CamSetting> GetCamSettingAsync(string camId);
        Task UpdateCamSettingAsync(EEsp32CamSetting setting);
        // Task DeleteCamSettingAsync(string camId);
    }
}
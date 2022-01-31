using System.Threading.Tasks;
using Home_Cam_Backend.Entities;
using MongoDB.Driver;

namespace Home_Cam_Backend.Repositories
{
    public class MongoDbCamSettingsRepository : ICamSettingsRepository
    {
        private const string databaseName = "home_cam_backend";
        private const string collectionName = "camSettings";
        private readonly IMongoCollection<EEsp32CamSetting> camSettingsCollection;

        private readonly FilterDefinitionBuilder<EEsp32CamSetting> camSettingFilterBuilder = Builders<EEsp32CamSetting>.Filter;

        public MongoDbCamSettingsRepository(IMongoClient mongoClient)
        {
            IMongoDatabase database = mongoClient.GetDatabase(databaseName);
            camSettingsCollection = database.GetCollection<EEsp32CamSetting>(collectionName);
        }
        public async Task CreateCamSettingAsync(EEsp32CamSetting setting)
        {
            await camSettingsCollection.InsertOneAsync(setting);
        }

        // public async Task DeleteCamSettingAsync(string camId)
        // {
        //     throw new System.NotImplementedException();
        // }

        public async Task<EEsp32CamSetting> GetCamSettingAsync(string camId)
        {
            var filter = camSettingFilterBuilder.Eq(existingCamSetting => existingCamSetting.UniqueId, camId);
            return await (await camSettingsCollection.FindAsync(filter)).SingleOrDefaultAsync();
        }

        public async Task UpdateCamSettingAsync(EEsp32CamSetting setting)
        {
            var filter = camSettingFilterBuilder.Eq(existingCamSetting => existingCamSetting.UniqueId, setting.UniqueId);
            await camSettingsCollection.ReplaceOneAsync(filter, setting);
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FFMediaToolkit;
using FFMediaToolkit.Decoding;
using Home_Cam_Backend.BackgroundTasks;
using Home_Cam_Backend.Repositories;
using Home_Cam_Backend.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Newtonsoft.Json.Serialization;

namespace Home_Cam_Backend
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddHostedService<ImageCaptureBackgroundTask>();
            services.AddHostedService<SearchCamBackgroundTask>();
            services.AddHostedService<ImageStorageSizeControlBackgroundTask>();

            services.AddSingleton<ICamSettingsRepository, MongoDbCamSettingsRepository>();
            services.AddSingleton<ICapturedImagesRepository, MongoDbCapturedImagesRepository>();

            services.AddSingleton<IMongoClient>(ServiceProvider =>
            {
                var settings = Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
                return new MongoClient(settings.ConnectionString);
            });

            services.AddControllers().AddJsonOptions(jsonOptions =>
            {
                jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = null;
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Home_Cam_Backend", Version = "v1" });
            });

            Extensions.Init(Configuration);


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ICamSettingsRepository repository)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Home_Cam_Backend v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            FFmpegLoader.FFmpegPath = Directory.GetCurrentDirectory() + "/third_party_lib/ffmpeg_dlls";
        }
    }
}

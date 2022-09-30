using IsIoTWeb.Context;
using IsIoTWeb.Models;
using IsIoTWeb.Mqtt;
using IsIoTWeb.Repository;
using IsIoTWeb.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace IsIoTWeb
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
            var mongoDbSettings = Configuration.GetSection("MongoDbSettings");
            services.AddSession();
            services.Configure<MongoDbSettings>(mongoDbSettings);
            services.AddSingleton<IMongoDbSettings>(service => service.GetRequiredService<IOptions<MongoDbSettings>>().Value);
            services.Configure<MqttSettings>(Configuration.GetSection("MqttSettings"));
            services.AddSingleton<IMqttSettings>(service => service.GetRequiredService<IOptions<MqttSettings>>().Value);
            services.AddScoped<IMongoDbContext, MongoDbContext>();
            services.AddIdentity<User, Role>()
                .AddMongoDbStores<User, Role, ObjectId>(
                    mongoDbSettings["ConnectionString"],
                    mongoDbSettings["DatabaseName"]
                );
            services.AddScoped<ISinkRepository, SinkRepository>();
            services.AddScoped<IReadingRepository, ReadingRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IValveRepository, ValveRepository>();
            services.AddScoped<IIrrigationRepository, IrrigationRepository>();
            services.AddScoped<IScheduleRepository, ScheduleRepository>();
            services.AddSingleton<IMqttClient, MqttClient>();
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

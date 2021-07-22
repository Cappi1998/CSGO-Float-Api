using Coravel;
using CSGO_Float_Api.Database;
using CSGO_Float_Api.Database.Repositories;
using CSGO_Float_Api.Models;
using CSGO_Float_Api.Schedule;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace CSGO_Float_Api
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
            if (Program.UseSSL_Certificate == true && Program.UseCertificateFile == false)
            {
                services.AddLettuceEncrypt();
            }

            services.AddHttpContextAccessor();// for the Session class to have access to httpcontext
            services.AddControllersWithViews();
            services.AddRazorPages().AddRazorRuntimeCompilation();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CSGO_Float_Api", Version = "v1" });
            });
            
            services.AddTransient<Server>();

            #region BancoDeDados
            services.AddScoped<DbContextFactory>();
            services.AddScoped<ISteamAccountRepository, SteamAccountRepository>();
            services.AddScoped<ISkinRepository, SkinRepository>();
            services.AddScoped<IFloatRequestRepository, FloatRequestRepository>();
            services.AddDbContext<DatabaseContext>(options => options.UseSqlite(Program.connectionString));
            #endregion

            services.AddMemoryCache();
            services.AddSession(options => { });
            services.AddScoped<Session>();
            services.AddScoped<AdminLogin>();


            //Coravel Service
            services.AddTransient<StartSteamClients>();
            services.AddTransient<SyncSkinToDatabase>();
            services.AddScheduler();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CSGO_Float_Api v1"));
            }

            if (Program.UseSSL_Certificate == true)
            {
                app.UseHttpsRedirection();
            }

            app.UseCookiePolicy();
            app.UseSession();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}",
                    defaults: new { controller = "Home", action = "Index" });
            });

            //Coravel - Scheduler
            app.ApplicationServices.UseScheduler(scheduler => {
                scheduler.Schedule<SyncSkinToDatabase>().EverySecond().PreventOverlapping("SyncSkinToDatabase");
                scheduler.Schedule<StartSteamClients>().EveryThirtySeconds().PreventOverlapping("StartSteamClients");
            });
        }
    }
}

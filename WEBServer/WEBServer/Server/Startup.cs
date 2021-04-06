using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using WEBServer.Server.Services.Infrastructure;
using WEBServer.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace WEBServer.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            string connectionString = "server=192.168.178.20;database=dbIOTFC;user id=root;password=Ogpdmllf2!"; //Da portare su file di config.


            services.AddDbContext<WEBServer.Server.Services.Infrastructure.dbIOTFCContext>(options =>
                options.UseMySQL(connectionString));

            services.AddDefaultIdentity<IdentityUser>()
                .AddEntityFrameworkStores<WEBServer.Server.Services.Infrastructure.dbIOTFCContext>();

            services.AddTransient<IDatabaseAccessor, MySQLDatabaseAccessor>();
            services.AddTransient<IFlowService, ADOFlowService>();
            services.AddTransient<ICompanyService,ADOCompanyService>();
            services.AddTransient<IMovementsService,ADOMovementsService>();
            services.AddTransient<IDeviceService,ADODeviceService>();
            services.AddTransient<IProbeService, ADOProbeService>();

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = false;
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = 401;
                    return System.Threading.Tasks.Task.CompletedTask;
                };
            });
            services.AddControllers().AddNewtonsoftJson();

            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}

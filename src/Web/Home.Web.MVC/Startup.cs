using Home.Data;
using Home.Data.Abstractions;
using Home.Data.Repositories;
using Home.Services.Vk;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Home.Web
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<HomeContext>(options =>
                options.UseNpgsql(_configuration["ConnectionStrings:Default"])
                       .EnableDetailedErrors(true)
                       .EnableSensitiveDataLogging(true));

            services.AddScoped<IDbContextFactory<HomeContext>, HomeContextFactory>();
            services.AddScoped<IActivityLogItemsRepository, ActivityLogItemsRepository<HomeContext>>();
            services.AddScoped<IVkUsersRepository, VkUsersRepository<HomeContext>>();
            services.AddScoped<IActivityAnalyzerService, ActivityAnalyzerService>();
            //services.AddScoped<IVkUserActivityPresenterService, VkUserActivityPresenterService>();

            //services.AddDatabaseDeveloperPageExceptionFilter();
            //services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
            //    .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddCors();

            services.AddMvc().AddRazorRuntimeCompilation();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days.
                // You may want to change this for production scenarios, 
                // see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors(builder =>
            builder.WithOrigins("http://localhost:4200", "*")
                .AllowAnyHeader()
                .AllowAnyMethod());


            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // ��� ��������� � ���������
                endpoints.MapControllerRoute(
                    name: "AreaRoute",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                // ����� ����� ��� ��������� � ���� �������� - �������� https://docs.microsoft.com/en-us/aspnet/core/blazor/fundamentals/routing?view=aspnetcore-5.0
                endpoints.MapControllers();
                
                // ��� ��������� ��� ��������
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");


                //endpoints.MapRazorPages();
            });

        }
    }
}
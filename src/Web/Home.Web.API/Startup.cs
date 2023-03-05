using AutoMapper;
using Home.Data;
using Home.Data.Abstractions;
using Home.Data.Repositories;
using Home.Services.Vk;
using Home.Web.API.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Home.Web.API;

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
                .EnableDetailedErrors(true) // TODO: set from configuration
                .EnableSensitiveDataLogging(true)); // TODO: set from configuration

        services.AddScoped<IDbContextFactory<HomeContext>, HomeContextFactory>();
        services.AddScoped<IActivityLogItemsRepository, ActivityLogItemsRepository<HomeContext>>();
        services.AddScoped<IVkUsersRepository, VkUsersRepository<HomeContext>>();
        services.AddScoped<IActivityAnalyzerService, ActivityAnalyzerService>();

        var mapperConfig = MapperConfiguration.CreateMapperConfiguration();
        mapperConfig.AssertConfigurationIsValid();
        services.AddScoped<IMapper, Mapper>(sp => new Mapper(mapperConfig));

        services.AddScoped<ApiExceptionFilter>();

        services.AddCors();
        services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
                //options.SuppressMapClientErrors
                //options.ClientErrorMapping
            });


        services.AddSwaggerGen(options
            => options.SwaggerDoc(_configuration["Swagger:ApiVersion"], 
                new OpenApiInfo {
                    Title = _configuration["Swagger:ApiTitle"],
                    Version = _configuration["Swagger:ApiVersion"]
                })
        );
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
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint(
                _configuration["Swagger:EndpointUrl"],
                _configuration["Swagger:ApiTitle"] + " " + _configuration["Swagger:ApiVersion"])
            );
        }

        //app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseCors(builder =>
            builder.WithOrigins(_configuration.GetSection("CorsUrls").Get<string[]>())
                .AllowAnyHeader()
                .AllowAnyMethod());

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            // ��� ��������� � ���������
            endpoints.MapControllerRoute(
                name: "AreaRoute",
                pattern: "api/{area}/{controller}/{action}/{id?}");

            // ����� ����� ��� ��������� � ���� �������� - �������� https://docs.microsoft.com/en-us/aspnet/core/blazor/fundamentals/routing?view=aspnetcore-5.0
            endpoints.MapControllers();
        });
    }
}
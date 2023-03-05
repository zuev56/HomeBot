using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;

namespace Home.Data;

public class HomeContextFactory : IDbContextFactory<HomeContext>, IDesignTimeDbContextFactory<HomeContext>
{
    private readonly DbContextOptions<HomeContext> _options;

    public HomeContextFactory()
    {
    }

    public HomeContextFactory(DbContextOptions<HomeContext> options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    // For repositories
    public HomeContext CreateDbContext() => new HomeContext(_options);
        
    // For migrations
    public HomeContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(System.IO.Path.GetFullPath(@"../Home.Bot/appsettings.Development.json"))
            .Build();
        var connectionString = configuration["ConnectionStrings:Default"];

        var optionsBuilder = new DbContextOptionsBuilder<HomeContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new HomeContext(optionsBuilder.Options);
    }

}
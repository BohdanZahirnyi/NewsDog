using Infrastructure.EF;
using Infrastructure.Interfaces;
using Infrastructure.Models;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TelegramTest;
using TL;

namespace WTelegramClientTest
{
    public static class Program
    {
        static IConfiguration? Configuration { get; set; }
        public static void Main()
        {
            var hostBuilder = new HostBuilder()
            .ConfigureAppConfiguration(BuildAppConfiguration)
            .ConfigureServices(ConfigureServices)
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
            });

            var app = hostBuilder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var newsDogDB = scope.ServiceProvider.GetRequiredService<PostsDbContext>();
                newsDogDB.Database.Migrate();
            }

            app.Run();
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            if (Configuration == null)
            {
                throw new NullReferenceException(nameof(Configuration));
            }

            services.AddDbContext<PostsDbContext>(options =>
        options.UseSqlServer(Configuration["ConnectionStrings:NewsDogDB"]));
            services.AddScoped<IRepository<Post>, PostRepository>();
            services.AddHostedService<TelegramInfoService>();
            services.AddLogging();
        }

        public static void BuildAppConfiguration(HostBuilderContext ctx, IConfigurationBuilder builder)
        {
            var environmentName = Environment.GetEnvironmentVariable("Development") ?? string.Empty;
           var config = builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile($"appsettings.{environmentName}.json", true)
                .AddEnvironmentVariables();

            Configuration = config.Build();
        }
    }
}
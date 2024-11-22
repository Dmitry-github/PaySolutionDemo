// See https://aka.ms/new-console-template for more information

namespace ConsoleApp
{
    using Logic;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    class Program
    {
        //private const string Host = "https://acstopay.online";
        //private const string echoPath = "/api/echo";

        public static IServiceProvider ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            //services.AddHttpClient();

            //IConfigurationRoot configuration = new ConfigurationBuilder()
            //    .SetBasePath(Directory.GetCurrentDirectory())
            //    .AddJsonFile("config.json", false)
            //    .Build();

            //services.AddSingleton<IConfigurationRoot>(configuration);
            //services.AddSingleton<IConfiguration>(configuration);

            //configuration.GetSection()

            services.AddTransient<ICardCheckerLogic, CardCheckerLogic>();
            //services.AddSingleton<INetworkRequestHandler, NetworkRequestHandler>();
            services.AddSingleton<INetworkRequestHandler>(_ => new NetworkRequestHandler(ConsoleConfig.Host, ConsoleConfig.TestCardPath));
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddTransient<IApplication, Application>();

            return services.BuildServiceProvider();
        }

        private static async Task Main(string[] args)
        {
            var services = ConfigureServices();
            //var _configuration = services.GetRequiredService<IConfiguration>();

            //string URL = "https://base64.guru/standards/base64url/encode";
            //string Base64URL = "aHR0cHM6Ly9iYXNlNjQuZ3VydS9zdGFuZGFyZHMvYmFzZTY0dXJsL2VuY29kZQ";
            
            var application = services.GetRequiredService<IApplication>();
            await application.RunAsync();
        }
    }
}


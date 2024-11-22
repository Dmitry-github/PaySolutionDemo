namespace ConsoleApp
{
    using Logic;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    internal class Program
    {
        public static IServiceProvider ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddTransient<ICardCheckerLogic, CardCheckerLogic>();
            services.AddTransient<INetworkRequestHandler>(_ => new NetworkRequestHandler(ConsoleConfig.Host, ConsoleConfig.TestCardPath));
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton<IApplication, Application>();

            return services.BuildServiceProvider();
        }

        private static async Task Main(string[] args)
        {
            var services = ConfigureServices();
            var application = services.GetRequiredService<IApplication>();
            await application.RunAsync();
        }
    }
}


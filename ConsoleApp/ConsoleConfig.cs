namespace ConsoleApp
{
    using System.IO;
    using Microsoft.Extensions.Configuration;

    public static class ConsoleConfig
    {
        private static IConfiguration Configuration
        {
            get
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("config.json", optional: false, reloadOnChange: true);

                IConfiguration config = builder.Build();
                return config;
            }
        }

        public static List<string> CardNumbers()
        {
            return Configuration.GetSection("AppSettings:cardNumbers").GetChildren().Select(_ => _.Value).ToList()!;
        }

        public static string? Host => Configuration["AppSettings:Host"];
        public static string? TestCardPath => Configuration["AppSettings:TestCardPath"];
        public static string? EchoPath => Configuration["AppSettings:EchoPath"];
        public static bool UseTimer => Convert.ToBoolean(Configuration["AppSettings:UseTimer"]);
    }
};


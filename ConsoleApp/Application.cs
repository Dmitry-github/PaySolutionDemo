namespace ConsoleApp
{
    using Logic;
    using Newtonsoft.Json;
    using System.Diagnostics;
    using System.Text;
    using System.Net.Http.Headers;
    using Models;

    public interface IApplication
    {
        public Task RunAsync();
    }

    public class Application: IApplication
    {
        private readonly ICardCheckerLogic _checkerLogic; 

        public Application(ICardCheckerLogic cardCheckerLogic)
        {
            _checkerLogic = cardCheckerLogic;
        }

        public async Task RunAsync()
        {
            var continueRun = true;

            while (continueRun)
            {
                await RunLoopAsync();
                Console.Write("To run again press 'r' > ");
                continueRun = Console.ReadKey().Key == ConsoleKey.R;
                Console.WriteLine();
            }
        }

        private async Task RunLoopAsync()
        {
            Console.Write("To show card numbers press '1' > ");
            var showCardNumbers = Console.ReadKey().Key == ConsoleKey.D1;
            Console.WriteLine();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                var resultDict = await _checkerLogic.CheckCardsListAsync(ConsoleConfig.CardNumbers());
                
                foreach (var cardNumber in ConsoleConfig.CardNumbers())
                {
                    var numberToPrint = showCardNumbers ? $"***{cardNumber.Substring(cardNumber.Length - 4)} " : string.Empty;
                    Console.WriteLine(numberToPrint + (resultDict[cardNumber] ? "Successfully" : "Unsuccessfully"));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e); // TODO: Log
            }

            stopwatch.Stop();

            if(ConsoleConfig.UseTimer)
                Console.WriteLine($"Time from start - {stopwatch.ElapsedMilliseconds} ms");

            //await EchoCheckAsync();
        }

        private static async Task EchoCheckAsync()   //BadRequest, CorrelatioinId:0HN7K4CI9EMAT#api
        {
            using HttpClient client = new();
            client.BaseAddress = new Uri(ConsoleConfig.Host!);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var payload = JsonConvert.SerializeObject(new { EchoString = "TestString" });

            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(ConsoleConfig.EchoPath, content);
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();

                if (responseString.Contains("Error"))
                {
                    var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseString);
                    Console.WriteLine(
                        $"Error result from {ConsoleConfig.Host + ConsoleConfig.EchoPath}: {errorResponse?.Error?.Code}, {errorResponse?.Error?.Message}");
                    return;
                }
                if (responseString.Contains("EchoString"))
                {
                    var echoResponse = JsonConvert.DeserializeObject<EchoResponse>(responseString);
                    Console.WriteLine(
                        $"Success result from {ConsoleConfig.Host + ConsoleConfig.EchoPath}: {echoResponse?.EchoString}, {echoResponse?.ServerTime}");
                    return;
                }
            }
            Console.WriteLine($"Result {response.Content.ReadAsStringAsync()} from {ConsoleConfig.Host + ConsoleConfig.EchoPath}");
        }
    }
}

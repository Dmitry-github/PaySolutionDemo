namespace ConsoleApp.Logic
{
    using System.Collections.Concurrent;

    public interface ICardCheckerLogic
    {
        public Task<ConcurrentDictionary<string, bool>> CheckCardsListAsync(IList<string> cardNumbers);
    }

    public class CardCheckerLogic: ICardCheckerLogic
    {
        private readonly INetworkRequestHandler? _handler;

        public CardCheckerLogic(INetworkRequestHandler? handler)
        {
            _handler = handler;
        }

        public async Task<ConcurrentDictionary<string, bool>> CheckCardsListAsync(IList<string> cardNumbers)
        {
            var resultsDictionary = new ConcurrentDictionary<string, bool>();
            var tasksList = new List<Task>();

            try
            {
                foreach (var cardNumber in cardNumbers)
                {
                    var requestTask = new Task(() =>
                    {
                        var validateTask = ValidateCardNumberAsync(cardNumber);
                        resultsDictionary.TryAdd(cardNumber, validateTask.Result);
                    });
                    requestTask.Start();
                    tasksList.Add(requestTask);
                }

                await Task.WhenAll(tasksList);

                return resultsDictionary;
            }
            catch (AggregateException ae)
            {
                Console.WriteLine(ae); // TODO: Log
            }

            return resultsDictionary;
        }
        //---------------------------------------------------
        private async Task<bool> ValidateCardNumberAsync(string cardNumber)
        {
            var response = await _handler?.MakePostRequestAsync(JwsHelper.GetJwsStringContent(cardNumber))!;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JwsHelper.DecodeJwsResponse(responseContent)?.Status == "Success"; 
            }

            return false;
        }
    }
}

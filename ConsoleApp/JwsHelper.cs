namespace ConsoleApp
{
    using Models;
    using System.Security.Cryptography;
    using System.Text;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class JwsHelper
    {
        //TODO: to secrets
        private const string KeyId = "47e8fde35b164e888a57b6ff27ec020f";
        private const string SharedKey = "ac/1LUdrbivclAeP67iDKX2gPTTNmP0DQdF+0LBcPE/3NWwUqm62u5g6u+GE8uev5w/VMowYXN8ZM+gWPdOuzg==";

        public static StringContent GetJwsStringContent(string text)
        {
            CardInfoRequest cardInfo = new()
            {
                CardInfo = new CardInfo { Pan = text }
            };

            var jsonPayload = JsonConvert.SerializeObject(cardInfo);
            var jwsMessage = CreateJwsMessage(jsonPayload);

            var content = new StringContent(jwsMessage, Encoding.UTF8, "application/json");
            return content;
        }

        private static string CreateJwsMessage(string payload)
        {
            ProtectedHeader protectedHeader = new()
            {
                Alg = "HS256",
                Kid = KeyId,
                Signdate = DateTime.UtcNow.ToString("O"),
                Cty = "application/json"
            };

            JsonSerializerSettings jsonSerializerSettings = new()
            {
                ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
            };

            //var sharedKeyBytes = Encoding.UTF8.GetBytes(SharedKey); //88 байт длина

            var protectedHeaderJson = JsonConvert.SerializeObject(protectedHeader, jsonSerializerSettings);
            var protectedHeaderBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(protectedHeaderJson));
            var payloadBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payload));

            var signingInput = $"{protectedHeaderBase64}.{payloadBase64}";
            var signature = SignPayload(signingInput);

            return $"{signingInput}.{signature}";
        }

        private static string SignPayload(string signingInput)
        {
            using HMACSHA256 hmac = new HMACSHA256(Convert.FromBase64String(SharedKey));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signingInput));
            return Base64UrlEncode(hash);
        }

        public static CardInfoResponse? DecodeJwsResponse(string response)
        {
            var parts = response.Split('.');

            if (parts.Length != 3)
            {
                // TODO: Log
                Console.WriteLine($"Invalid JWS message format: {string.Join(' ', parts)}");
                return null;
            }

            var payloadJsonText = Base64UrlDecode(parts[1]);
            return JsonConvert.DeserializeObject<CardInfoResponse>(payloadJsonText);
        }

        private static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static string Base64UrlDecode(string encodedText)
        {
            var text = encodedText.Replace('-', '+').Replace('_', '/');
            
            switch (text.Length % 4)
            {
                case 2: text += "=="; break;
                case 3: text += "="; break;
            }
            return Encoding.UTF8.GetString(Convert.FromBase64String(text));
        }
    }
}

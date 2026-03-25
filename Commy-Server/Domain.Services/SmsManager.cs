using Domain.Services.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Domain.Services
{    
    public class SmsManager : ISmsManager
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly IConfiguration config;
        private readonly ILogger<SmsManager> logger;

        public SmsManager(IConfiguration config, ILogger<SmsManager> logger)
        {
            this.config = config;
            this.logger = logger;
        }

        public bool Send(string to, string from, string subject, string body)
        {
            var values = new Dictionary<string, string>
            {
                { "post", "2" },
                { "uid", config["Sms:SmsUid"] ?? "" },
                { "un", config["Sms:SmsUn"] ?? "" },
                { "msg", body },
                { "charset", "utf-8" },
                { "from", from },
                { "list", to }
            };

            var content = new FormUrlEncodedContent(values);
            try
            {
                var response = client.PostAsync(config["Sms:url"], content).Result;

                var responseString = response.Content.ReadAsStringAsync().Result;

                logger.LogInformation("Sms sent to: {0}, from: {1}, subject: {2}, body: {3} responseString: {4}", to, from, subject, body, responseString);

                return responseString.Contains("OK");

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> SendAsync(string to, string from, string subject, string body)
        {
            var values = new Dictionary<string, string>
            {
                { "post", "2" },
                { "uid", config["Sms:SmsUid"] ?? ""},
                { "un", config["Sms:SmsUn"] ?? ""},
                { "msg", body },
                { "charset", "utf-8" },
                { "from", from },
                { "list", to }
            };

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync(config["Sms:url"], content);

            var responseString = await response.Content.ReadAsStringAsync();

            logger.LogInformation("Sms sent to: {0}, from: {1}, subject: {2}, body: {3} responseString: {4}", to, from, subject, body, responseString);

            return responseString.Contains("OK");

        }

        public bool Send(string[] to, string from, string subject, string body)
        {
            var values = new Dictionary<string, string>
            {
                { "post", "2" },
                { "uid", config["Sms:SmsUid"] ?? "" },
                { "un", config["Sms:SmsUn"] ?? "" },
                { "msg", body },
                { "charset", "utf-8" },
                { "from", from },
                { "list", string.Join(',', to)}
            };

            var content = new FormUrlEncodedContent(values);

            var response = client.PostAsync(config["Sms:url"], content).Result;

            var responseString = response.Content.ReadAsStringAsync().Result;

            logger.LogInformation("Sms sent to: {0}, from: {1}, subject: {2}, body: {3} responseString: {4}", string.Join(',', to), from, subject, body, responseString);

            return responseString.Contains("OK");
        }

        public async Task<bool> SendAsync(string[] to, string from, string subject, string body)
        {
            var values = new Dictionary<string, string>
            {
                { "post", "2" },
                { "uid", config["Sms:SmsUid"] ?? "" },
                { "un", config["Sms:SmsUn"] ?? "" },
                { "msg", body },
                { "charset", "utf-8" },
                { "from", from },
                { "list", string.Join(',', to )}
            };

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync(config["Sms:url"], content);

            var responseString = await response.Content.ReadAsStringAsync();

            logger.LogInformation("Sms sent to: {0}, from: {1}, subject: {2}, body: {3} responseString: {4}", string.Join(',', to ), from, subject, body, responseString);

            return responseString.Contains("OK");
        }
    }
}
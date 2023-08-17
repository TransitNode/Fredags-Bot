using DSharpPlus.Net;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Fredags_Bot.scr.config
{
    internal class JSONReader
    {
        private static readonly string DEFAULT_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        private readonly Lazy<Task<JSONStructure>> jsonData;

        public string Token => jsonData.Value.Result.Token;
        public string Prefix => jsonData.Value.Result.Prefix;
        public string Hostname => jsonData.Value.Result.Hostname;
        public ConnectionEndpoint Endpoint => CreateEndpoint(jsonData.Value.Result.Port);
        public string Password => jsonData.Value.Result.Password;

        public JSONReader(string filePath = null)
        {
            jsonData = new Lazy<Task<JSONStructure>>(() => LoadJsonData(filePath ?? DEFAULT_PATH));
        }

        private async Task<JSONStructure> LoadJsonData(string filePath)
        {
            Console.WriteLine($"Looking for config.json at {filePath}");

            if (File.Exists(filePath))
            {
                Console.WriteLine("config.json found successfully.");
                return await ReadJSON(filePath);
            }
            else
            {
                Console.WriteLine("config.json not found. Please check the path.");
                return null;
            }
        }

        private async Task<JSONStructure> ReadJSON(string filePath)
        {
            try
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string json = await sr.ReadToEndAsync();
                    return JsonConvert.DeserializeObject<JSONStructure>(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while reading the JSON: {ex}");
                return null;
            }
        }

        private ConnectionEndpoint CreateEndpoint(string portString)
        {
            if (!int.TryParse(portString, out int port) || port < 1 || port > 65535)
            {
                throw new ArgumentException("Invalid port number.");
            }

            return new ConnectionEndpoint
            {
                Hostname = this.Hostname,
                Port = port
            };
        }

        private sealed class JSONStructure
        {
            [JsonProperty("token")]
            public string Token { get; set; }

            [JsonProperty("prefix")]
            public string Prefix { get; set; }

            [JsonProperty("Hostname")]
            public string Hostname { get; set; }

            [JsonProperty("Port")]
            public string Port { get; set; }

            [JsonProperty("Password")]
            public string Password { get; set; }
        }
    }
}

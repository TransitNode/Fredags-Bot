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

        public string Token { get; private set; }
        public string Prefix { get; private set; }
        public string Hostname { get; private set; }
        public ConnectionEndpoint Endpoint { get; private set; }
        public string Password { get; private set; }

        public JSONReader(string filePath = null)
        {
            if (filePath == null) filePath = DEFAULT_PATH;
            Console.WriteLine($"Looking for config.json at {filePath}"); // Debug print

            if (File.Exists(filePath))
            {
                Console.WriteLine("config.json found successfully.");
                ReadJSON(filePath).GetAwaiter().GetResult();
            }
            else
            {
                Console.WriteLine("config.json not found. Please check the path.");
            }
        }

        private async Task ReadJSON(string filePath)
        {
            try
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string json = await sr.ReadToEndAsync();
                    JSONStructure data = JsonConvert.DeserializeObject<JSONStructure>(json);

                    Token = data.Token;
                    Prefix = data.Prefix;
                    Hostname = data.Hostname;
                    Password = data.Password;
                    Endpoint = CreateEndpoint(data.Port);
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions here, perhaps by logging or rethrowing
                Console.WriteLine($"An error occurred while reading the JSON: {ex}");
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
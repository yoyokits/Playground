// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAICore.AI
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class ComfyUiApiClient
    {
        #region Methods

        public static readonly string ServerAddress = "127.0.0.1:8188";

        private static readonly HttpClient client = new HttpClient();

        private static readonly string ClientId = Guid.NewGuid().ToString();

        public async Task<Dictionary<string, List<byte[]>>> ConnectWebSocketAndFetchImages(string jsonFileName)
        {
            var ws = new ClientWebSocket();
            await ws.ConnectAsync(new Uri($"ws://{ServerAddress}/ws?clientId={ClientId}"), CancellationToken.None);

            // Assuming GetImages is a method in your class that takes a ClientWebSocket and a JObject as parameters
            var json = File.ReadAllText(jsonFileName);
            var jsonObject = JObject.Parse(json);

            var images = await GetImages(ws, jsonObject);
            return images;
        }

        public async Task<JObject> GetHistory(string promptId)
        {
            var response = await client.GetAsync($"http://{ServerAddress}/history/{promptId}");
            var responseString = await response.Content.ReadAsStringAsync();
            return JObject.Parse(responseString);
        }

        public async Task<byte[]> GetImage(string filename, string subfolder, string folderType)
        {
            var response = await client.GetAsync($"http://{ServerAddress}/view?filename={Uri.EscapeDataString(filename)}&subfolder={Uri.EscapeDataString(subfolder)}&type={Uri.EscapeDataString(folderType)}");
            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<Dictionary<string, List<byte[]>>> GetImages(ClientWebSocket ws, JObject prompt)
        {
            var promptResult = await QueuePrompt(prompt);
            var promptId = promptResult["prompt_id"].ToString();
            var outputImages = new Dictionary<string, List<byte[]>>();
            var buffer = new byte[1024];
            var iteration = 0;
            while (true)
            {
                Trace.WriteLine($"Iteration {iteration++}");
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                var outStr = Encoding.UTF8.GetString(buffer, 0, result.Count);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = JObject.Parse(outStr);
                    var type = message["type"];
                    Trace.WriteLine($"Message Type: {type}");
                    if ((string)message["type"] == "executing")
                    {
                        var data = (JObject)message["data"];
                        var node = data["node"];
                        var id = data["prompt_id"];
                        Trace.WriteLine($"Node: {node};Id: {id}");
                        if (string.IsNullOrEmpty(node.ToString()) && (string)id == promptId)
                        {
                            break; //Execution is done
                        }
                    }
                }
            }

            var historyJson = await GetHistory(promptId);
            var history = historyJson[promptId];
            foreach (var o in (JArray)history["outputs"])
            {
                foreach (var nodeId in (JArray)history["outputs"])
                {
                    var nodeOutput = (JObject)history["outputs"][nodeId.ToString()];
                    if (nodeOutput.ContainsKey("images"))
                    {
                        var imagesOutput = new List<byte[]>();
                        foreach (var image in (JArray)nodeOutput["images"])
                        {
                            var imageData = await GetImage((string)image["filename"], (string)image["subfolder"], (string)image["type"]);
                            imagesOutput.Add(imageData);
                        }
                        outputImages[nodeId.ToString()] = imagesOutput;
                    }
                }
            }

            return outputImages;
        }

        public string ModifyJsonSubNodeValue(string jsonString, string parentNode, string subNode, string newValue)
        {
            var jsonObject = JObject.Parse(jsonString);
            jsonObject[parentNode][subNode] = newValue;
            return jsonObject.ToString();
        }

        public HttpResponseMessage Post(string json)
        {
            var p = new { prompt = json, client_id = ClientId };
            var data = new StringContent(JsonConvert.SerializeObject(p), Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                var results = client.PostAsync($"http://{ServerAddress}/prompt", data);
                return results.Result;
            }
        }

        public HttpResponseMessage PostJsonFile(string jsonFileName)
        {
            var json = File.ReadAllText(jsonFileName);
            return this.Post(json);
        }

        public async Task<JObject> QueuePrompt(JObject prompt)
        {
            var p = new { prompt = prompt, client_id = ClientId };
            var data = new StringContent(JsonConvert.SerializeObject(p), Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"http://{ServerAddress}/prompt", data);
            var responseString = await response.Content.ReadAsStringAsync();
            return JObject.Parse(responseString);
        }

        #endregion Methods
    }
}
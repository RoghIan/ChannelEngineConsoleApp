using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ChannelEngine
{
    class Program
    {
        //every input is hardcoded, I don't have much time to add user input functionalty
        private static readonly HttpClient client = new HttpClient();
        private static string apiKey = "541b989ef78ccb1bad630ea5b85c6ebff9ca3322";
        private static string testProductForUpdate = "001201-s";

        static async Task Main(string[] args)
        {
            var result = await FetchOrders();

            foreach (var product in result)
            {
                Console.WriteLine(product.MerchantProductNo + " | " + product.Gtin + " | " + product.Quantity);
            }

            await UpdateStock();
        }

        //fetch Orders
        private static async Task<List<Line>> FetchOrders()
        {
            var streamTask = client.GetStreamAsync("https://api-dev.channelengine.net/api/v2/orders?apikey=" + apiKey + "&statuses=IN_PROGRESS");
            var orders = await System.Text.Json.JsonSerializer.DeserializeAsync<Orders>(await streamTask);

            var productResult = new List<Line>();

            foreach (var order in orders.Content)
            {
                foreach(var product in order.Lines)
                {
                    productResult.Add(product);
                }
            }

            return productResult.OrderByDescending(x => x.Quantity).Take(5).ToList();
        }

        // update stock
        private static async Task UpdateStock()
        {
            HttpResponseMessage response = await client.PatchAsync("https://api-dev.channelengine.net/api/v2/products/"+ testProductForUpdate + "?apikey=" + apiKey, BuildJsonPatch());
        }

        //helper for patch stock content
        private static ByteArrayContent BuildJsonPatch()
        {
            // update value hard coded
            var result = new List<JsonPatchDoc>
            {
                new JsonPatchDoc()
                {
                    op = "replace",
                    value = 25,
                    path = "Stock"
                }
            };

            var myContent = JsonConvert.SerializeObject(result);
            var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return byteContent;
        }
    }
}

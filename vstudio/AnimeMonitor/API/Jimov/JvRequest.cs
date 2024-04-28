using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

#pragma warning disable CS8603

namespace AnimeMonitor.API.Jimov {
    internal class JvRequest {

        public const string URL_JIMOV_SERVER = "https://jimov-api.vercel.app";
        private JvRequest() { }

        public static JArray GetLastEpisodes() {
            return GetRequest(URL_JIMOV_SERVER + "/anime/tioanime/last/episodes");
        }

        public static JArray GetRequest(string strUrl) {
            try {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

                HttpResponseMessage response = client.GetAsync(strUrl).Result;

                if (response.IsSuccessStatusCode) {
                    string jsonStr = response.Content.ReadAsStringAsync().Result;
                    return JArray.Parse(jsonStr);
                } else {
                    Console.WriteLine("Error en la solicitud: " + response.ReasonPhrase);
                }
            } catch (HttpRequestException e) {
                Console.WriteLine(e.Message);
            }

            return null;
        }
    }
}

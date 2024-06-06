using AnimeMonitor.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AnimeMonitor.API.Jimov {
    internal class JvRequest {

        public const string URL_JIMOV_SERVER = "https://jimov-api.vercel.app";
        private JvRequest() { }

        public static JArray GetLastEpisodes() {
            return GetRequest(URL_JIMOV_SERVER + "/anime/tioanime/last/episodes");
        }

        /* the name format must be expressed as no-game-no-life-5 containing the
         * name of the anime and the episode number 
         */
        public static string GetEpisodeMegaLink(string episode) {
            JArray jarray = GetRequest(URL_JIMOV_SERVER + "/anime/tioanime/episode/" + episode);
            if (jarray != null) {
                foreach (var item in jarray) {
                    if ("Mega".Equals(item["name"]?.ToString())) {
                        return item["file_url"]?.ToString();
                    }
                }
            }
            return null;
        }

        public static JArray GetRequest(string strUrl) {
            try {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
                HttpResponseMessage response = client.GetAsync(strUrl).Result;
                if (response.IsSuccessStatusCode)
                    return JArray.Parse(response.Content.ReadAsStringAsync().Result);
                else
                    throw new ApplicationException("Response error: " + response.ReasonPhrase);
            } catch (HttpRequestException e) {
                Logging.Error(e.Message);
                Logs.CreateReport(e);
            }
            return null;
        }
    }
}

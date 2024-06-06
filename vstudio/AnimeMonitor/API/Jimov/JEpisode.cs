using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeMonitor.API.Jimov {
    public class JEpisode {
        public string[] Servers  { get; private set; }
        public string   ImageUrl { get; private set; }
        public string   Url      { get; private set; }
        public string   Name     { get; private set; }
        public UInt32   Number   { get; private set; }

        private EpisodeUrl episodeUrl = null;

        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }

            JEpisode other = (JEpisode)obj;

            return Servers.SequenceEqual(other.Servers) &&
                   ImageUrl == other.ImageUrl &&
                   Url == other.Url &&
                   Name == other.Name &&
                   Number == other.Number;
        }

        public static JEpisode ToJEpisode(JToken jToken) {
            return new JEpisode() {
                Servers  = jToken["servers"].ToObject<string[]>(),
                ImageUrl = (string)jToken["image"],
                Name     = (string)jToken["name"],
                Number   = (UInt32)jToken["number"],
                Url      = (string)jToken["url"]
            };
        }

        public override int GetHashCode() {
            unchecked {
                int hash = 17;
                hash = hash * 23 + (Servers != null ? Servers.GetHashCode() : 0);
                hash = hash * 23 + (ImageUrl != null ? ImageUrl.GetHashCode() : 0);
                hash = hash * 23 + (Url != null ? Url.GetHashCode() : 0);
                hash = hash * 23 + (Name != null ? Name.GetHashCode() : 0);
                hash = hash * 23 + Number.GetHashCode();
                return hash;
            }
        }

        public EpisodeUrl ToEpisodeUrl() {
            if (episodeUrl == null)
                episodeUrl = new EpisodeUrl(Url);
            return episodeUrl;
        }

        public override string ToString() {
            return $"Name: {Name}, Number: {Number}, ImageUrl: {ImageUrl}, " +
                $"Url: {Url}, Servers: [{string.Join(", ", Servers)}]";
        }
    }
}

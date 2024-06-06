using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AnimeMonitor.API.Jimov {
    public sealed class EpisodeUrl {

        public string Url { get; private set; }
        public int Number { get; private set; }
        public string Name { get; private set; }
        public string Fullname { get; private set; }

        // example link https://tioanime.com/ver/blue-archive-the-animation-6
        public EpisodeUrl(string url) {
            Fullname = url.Substring(url.LastIndexOf('/') + 1);
            Url = url;
            Number = int.Parse(url.Substring(url.LastIndexOf('-') + 1));
            Name = Fullname.Substring(0, Fullname.LastIndexOf('-'));
        }

        public override string ToString() {
            return $"Name: { Name }, Url: { Url }, Number: { Number }";
        }
    }
}

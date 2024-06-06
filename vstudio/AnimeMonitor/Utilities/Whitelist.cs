using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeMonitor.Utilities {
    internal class Whitelist {

        private static Whitelist instance;
        public List<string> AnimesWhiteList { get; private set; } = new List<string>();

        private readonly string WHITELIST_FILEPATH = "./whitelist.json";

        private Whitelist() {
            OpenAnimesWhiteList();
        }

        public static Whitelist GetInstance() {
            if (instance == null)
                instance = new Whitelist();
            return instance;
        }

        private void OpenAnimesWhiteList() {
            try {
                if (File.Exists(WHITELIST_FILEPATH))
                    AnimesWhiteList = ((JArray)JObject.Parse(File.ReadAllText(WHITELIST_FILEPATH))["animes"])
                        .ToObject<List<string>>();
            } catch (Exception e) {
                throw e;
            }
        }

        public void AddAnime(string name) {
            if (!AnimesWhiteList.Exists(value => value.Equals(name))) {
                AnimesWhiteList.Add(name);
                Save();
            }
        }

        public void RemoveAnime(string name) {
            int index = AnimesWhiteList.IndexOf(name);
            if (index > -1) {
                AnimesWhiteList.RemoveAt(index);
                Save();
            }
        }

        private void Save() {
            if (AnimesWhiteList.Count == 0) {
                File.Delete(WHITELIST_FILEPATH);
            } else {
                string json = JsonConvert.SerializeObject(new { animes = AnimesWhiteList });
                File.WriteAllText(WHITELIST_FILEPATH, json);
            }
        }
    }
}

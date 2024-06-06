using AnimeMonitor.API.Jimov;
using AnimeMonitor.Components;
using AnimeMonitor.IO;
using AnimeMonitor.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AnimeMonitor.Services {
    internal class LoadNotifier {

        private JArray _oldEpisodes = null;
        private Thread _threadNtf = null;
        private static LoadNotifier _instance = null;

        /* _registeredAnimes */
        private readonly object _lockObject = new object();
        private readonly string ANIMESINFO_DIRECTORY = "./animesinfo";
        private readonly FileProperties _NotifierProps;
        private readonly IOController iOController = new IOController();

        private List<string> _registeredAnimes = null;

        public static LoadNotifier GetInstance() {
            if (_instance == null)
                _instance = new LoadNotifier();
            return _instance;
        }

        /********************************************************/

        public List<string> RegisteredANimes { 
            get {
                if (RegisteredANimes == null)
                    return null;
                lock (_lockObject) {
                    return _registeredAnimes.ToList();
                }
            } set {
                lock (_lockObject) {
                    _registeredAnimes = value.ToList();
                }
            }
        }

        private LoadNotifier() {
            _NotifierProps = new FileProperties("./notification_options.properties");
            if (!Directory.Exists(ANIMESINFO_DIRECTORY))
                Directory.CreateDirectory(ANIMESINFO_DIRECTORY);
            _registeredAnimes = Whitelist.GetInstance().AnimesWhiteList;
            if (_registeredAnimes.Count == 0) {
                _registeredAnimes = null;
            }
        }

        public void StopMonitor() {
            if (_threadNtf != null) {
                if (_threadNtf.IsAlive) {
                    _threadNtf.Interrupt();
                    _threadNtf.Join();
                }
                _threadNtf = null;
            }
        }

        public void RunMonitor() {
            if (_threadNtf == null) {
                _threadNtf = new Thread(() => {
                    while (true) {
                        try {
                            Thread.Sleep(60000);
                            ScanMissingAnimeEpisodes();
                            ScanEpisodes();
                        } catch (Exception e) {
                            Logs.CreateReport(e);
                            break;
                        }
                    }
                });
                _threadNtf.Start();
            }
        }

        private void ScanMissingAnimeEpisodes() {
            var filesToEnumerate = Directory.EnumerateFiles(ANIMESINFO_DIRECTORY)
                    .Where(filePath => !Path.GetFileName(filePath).Equals("queue.json"));
            foreach (var filePath in filesToEnumerate) {
                JArray jArray = iOController.ReadJArray(filePath);
                JToken lastToken = jArray.Last;
                if (lastToken != null) {
                    try {
                        bool isWritable = false;
                        for (int i = 0; i < ((int)lastToken["number"]); i++) {
                            if (((int)jArray[i]["number"]) != i + 1) {
                                string downloadLink = JvRequest.GetEpisodeMegaLink(
                                    $"{Path.GetFileNameWithoutExtension(Path.GetFileName(filePath))}-{i + 1}")
                                    ?? throw new Exception("Error in download link request");
                                isWritable = true;
                                jArray.Insert(i, new JObject {
                                    ["number"] = i + 1,
                                    ["downloaded"] = false,
                                    ["link"] = downloadLink
                                });
                            }
                        }

                        if (isWritable)
                            iOController.WriteJArray(filePath, jArray);
                    } catch (Exception) {
                        /* No read exception */
#if DEBUG
                        //Logging.Error(e.Message);
#endif
                    }
                }
            }
        }

        private bool IsOnlyRegisteredAnimeNotified(JEpisode jEpisode) {
            if (_registeredAnimes != null)
                return _registeredAnimes.Find(name => name == jEpisode.Name) != null;
            return false;
        }

        private List<JEpisode> GetNewEpisodes(JArray jArray) {
            List<JEpisode> episodes = null;
            if (_oldEpisodes != null) {
                JEpisode lastEpisode = JEpisode.ToJEpisode(_oldEpisodes[0]);
                for (int i = 0; i < jArray.Count; i++) {
                    JEpisode jEpisode = JEpisode.ToJEpisode(jArray[i]);
                    if (!lastEpisode.Equals(jEpisode)) {
                        lock (_lockObject) {
                            if (_registeredAnimes == null || IsOnlyRegisteredAnimeNotified(jEpisode)) {
                                if (episodes == null)
                                    episodes = new List<JEpisode>();
                                episodes.Add(jEpisode);
                            }
                        }
                        continue;
                    }
                    break;
                }
            }
            return episodes;
        }

        private void NotifyEpisode(JEpisode jEpisode) {
            if (jEpisode != null) {
                Logging.Log($"New episode published {jEpisode.Name}-{jEpisode.Number}");
                AppTrayMenu.GetInstance().Notify("Nuevo Capitulo", $"{jEpisode.Name}. " + 
                    $"Episodio {jEpisode.Number}");
            }
        }

        private void ScanEpisodes() {
            JArray jArray = JvRequest.GetLastEpisodes();
            if (jArray != null && jArray.Count > 0) {
                List<JEpisode> jEpisodes = GetNewEpisodes(jArray);
                if (_oldEpisodes == null) {
                    JEpisode episode = JEpisode.ToJEpisode(jArray[0]);
                    string lastEpisode = _NotifierProps["lastEpisode", null];
                    if (lastEpisode == null || !lastEpisode.Equals(episode.ToEpisodeUrl().Fullname)) {
                        _NotifierProps["lastEpisode"] = episode.ToEpisodeUrl().Fullname;
                        SaveDownloadLink(episode.ToEpisodeUrl());
                        NotifyEpisode(episode);
                    }
                }
                _oldEpisodes = jArray;
                jEpisodes?.ForEach(episode => {
                    SaveDownloadLink(episode.ToEpisodeUrl());
                    NotifyEpisode(episode);
                    Thread.Sleep(10000);
                });
            }
        }

        private void SaveDownloadLink(EpisodeUrl eUrl) {
            new Thread(() => {
                try {
                    string downloadLink = JvRequest.GetEpisodeMegaLink(eUrl.Fullname);
                    if (downloadLink != null) {
                        RegisterAnimeQueued(eUrl.Name);
                        string filename = $"{ANIMESINFO_DIRECTORY}/{eUrl.Name}.json";
                        JArray jArray = iOController.ReadJArray(filename);
                        if (jArray.ToList().FindIndex(token => ((int)token["number"]) == eUrl.Number) == -1) {
                            jArray.Add(new JObject {
                                ["number"] = eUrl.Number,
                                ["downloaded"] = false,
                                ["link"] = downloadLink
                            });
                            iOController.WriteJArray(filename, jArray);
                        }
                    }
                } catch (Exception e) {
                    Logs.CreateReport(e);
                }
            }).Start();
        }

        private void RegisterAnimeQueued(string urlName) {
            JArray jArray = iOController.ReadJArray(Downloads.QUEUE_ANIMES_FILE);
            if (jArray.ToList().Find(token => ((string)token) == urlName) == null) {
                Logging.Log($"New anime registered in the queue {urlName}");
                jArray.Add(urlName);
                iOController.WriteJArray(Downloads.QUEUE_ANIMES_FILE, jArray);
            }
        }
    }
}

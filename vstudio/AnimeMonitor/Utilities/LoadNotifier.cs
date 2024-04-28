using AnimeMonitor.API.Jimov;
using AnimeMonitor.Components;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AnimeMonitor.Utilities {
    internal class LoadNotifier {

        private JArray _oldEpisodes = null;
        private Thread _threadNtf = null;
        private static LoadNotifier _instance = null;

        /* _registeredAnimes */
        private readonly object _lockObject = new object();

        private List<string> _registeredAnimes = null;

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

        public static LoadNotifier GetInstance() {
            if (_instance == null)
                _instance = new LoadNotifier();
            return _instance;
        }

        private LoadNotifier() {}

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
                            ScanEpisodes();
                        } catch (Exception) {
                            break;
                        }
                    }
                });
                _threadNtf.Start();
            }
        }

        private bool IsOnlyRegisteredAnimeNotified(JEpisode jEpisode) {
            if (_registeredAnimes != null) {
                return _registeredAnimes.Find(name => name == jEpisode.Name) != null;
            }
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
                AppTrayMenu.GetInstance().Notify("Nuevo Capitulo", $"{jEpisode.Name}. " + 
                    $"Episodio {jEpisode.Number}");
            }
        }

        private void ScanEpisodes() {
            JArray jArray = JvRequest.GetLastEpisodes();
            if (jArray != null && jArray.Count > 0) {
                List<JEpisode> jEpisodes = GetNewEpisodes(jArray);
                if (_oldEpisodes == null)
                    NotifyEpisode(JEpisode.ToJEpisode(jArray[0]));
                _oldEpisodes = jArray;
                if (jEpisodes != null)  {
                    jEpisodes.ForEach(episode => {
                        NotifyEpisode(episode);
                        Thread.Sleep(10000);
                    });
                }
            }
        }
    }
}

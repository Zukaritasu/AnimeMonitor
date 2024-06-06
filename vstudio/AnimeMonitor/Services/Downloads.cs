using AnimeMonitor.IO;
using AnimeMonitor.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AnimeMonitor.Services {
    public class Downloads {
        private static Downloads instance;

        private static readonly string DOWNLOADS_DIRECTORY_NAME = "downloads";
        private static readonly string DOWNLOADS_DIRECTORY = $"./{DOWNLOADS_DIRECTORY_NAME}";
        private static readonly string TEMPORARY_FILES_DIRECTORY = $"{DOWNLOADS_DIRECTORY}/temp";
        public static readonly string QUEUE_ANIMES_FILE = "./animesinfo/queue.json";
        private bool isRunning = false;
        private Thread thread;
        private readonly IOController iOController = new IOController();

        public static Downloads GetInstance() {
            if (instance == null)
                instance = new Downloads();
            return instance;
        }

        private Downloads() {
            string[] directories = new string[] {
                DOWNLOADS_DIRECTORY,
                TEMPORARY_FILES_DIRECTORY
            };

            /* Create all directories */
            foreach (string directory in directories) {
                if (!Directory.Exists(directory)) {
                    if (!Directory.CreateDirectory(directory).Exists) {
                        throw new Exception($"Error creating the directory: {directory}");
                    }
                }
            }
        }

        public void InitService() {
            if (!isRunning) {
                isRunning = true;
                thread = new Thread(() => {
                    while (true) {
                        try {
                            DeleteTemporaryFiles();
                            List<JToken> objects = GetQueuedEpisodesForDownload();
                            if (objects != null) {
                                foreach (var token in objects) {
                                    Download(token);
                                }
                            }
                        } catch (Exception e) {
                            Logs.CreateReport(e);
                        }
                    }
                });
                thread.Start();
            }
        }

        private void DeleteTemporaryFiles() {
            Directory.EnumerateFiles(DOWNLOADS_DIRECTORY_NAME).Where((filePath) => {
                return !filePath.EndsWith(".mp4");
            }).Concat(Directory.EnumerateFiles(TEMPORARY_FILES_DIRECTORY, "*.json")).ToList()
            .ForEach(filePath => {
                try {
                    File.Delete(filePath);
                } catch (Exception e) {
                    Logging.Error(e.Message);
                }
            });
        }

        private string CreateTemporalFileInfo(JToken epInfo) {
            string tempFilename = $"{TEMPORARY_FILES_DIRECTORY}/{epInfo["nameUrl"]}-{epInfo["number"]}.json";
            File.WriteAllText(tempFilename, epInfo.ToString());
            return tempFilename;
        }

        private void ChangeDownloadStatusProperty(JToken epInfo) {
            string animeFileName = $"./animesinfo/{epInfo["nameUrl"]}.json";
            JArray jArray = iOController.ReadJArray(animeFileName);
            foreach (var token in jArray) {
                if (((int)token["number"]) == ((int)epInfo["number"])) {
                    token["downloaded"] = true;
                    iOController.WriteJArray(animeFileName, jArray);
                    break;
                }
            }
        }

        private bool ProcessingDownloadServiceEvents(JToken epInfo, JObject response, string tempFile) {
            switch ((ResponseStatus)(int)response["status"]) {
                case ResponseStatus.SUCCEEDED:
                    ChangeDownloadStatusProperty(epInfo);
                    File.Delete(tempFile);
                    Logging.Log("finished");
                    return true;
                case ResponseStatus.DOWNLOAD_PROGRESS:
                    Logging.Log(response.ToString());
                    return false;
                case ResponseStatus.FAILED:
                    if (response.ContainsKey("message"))
                        Logging.Error((string)response["message"]);
                    if (!response.ContainsKey("stop") || (bool)response["stop"])
                        return true;
                    return false;
                default:
                    break;
            }
            return false;
        }

        private void MonitorDownload(NodeServiceConnection nodeService, JToken epInfo, string tempFile) {
            try {
                JObject request = CreateRequestService(epInfo);
                nodeService.Write(request.ToString());
                JObject response = null;
                Logging.Log($"downloading file {epInfo["file"]["filename"]}...");
                while ((response = JObject.Parse(nodeService.Read())) != null) {
                    if (ProcessingDownloadServiceEvents(epInfo, response, tempFile)) {
                        break;
                    }
                }
            } catch (Exception e) {
                Logs.CreateReport(e);
            }
        }

        private JObject CreateRequestService(JToken epInfo) {
            return new JObject {
                ["command"] = "--download",
                ["link"] = epInfo["link"],
                ["output"] = $"{Application.StartupPath}/{DOWNLOADS_DIRECTORY_NAME}",
                ["filename"] = epInfo["file"]["filename"]
            };
        }

        private bool ProcessResponse(JToken epInfo, NodeServiceConnection nodeService, string fileInfoResponse) {
            JObject objResponse = JObject.Parse(fileInfoResponse);
            if (((int)objResponse["status"]) != (int)ResponseStatus.SUCCEEDED) {
                Logging.Error((string)objResponse["message"]);
                return false;
            } else {
                epInfo["file"] = objResponse["data"];
                MonitorDownload(nodeService, epInfo, CreateTemporalFileInfo(epInfo));
                return true;
            }
        }

        private bool Download(JToken epInfo) {
            try {
                using (NodeServiceConnection nodeService = new NodeServiceConnection()) {
                    using (NodeService service = new NodeService(nodeService).Run()) {
                        if (nodeService.Connect()) {
                            nodeService.Write($"{{\"command\": \"--fileinfo\", \"link\": \"{epInfo["link"]}\"}}");
                            string fileInfoResponse = nodeService.Read();
                            if (fileInfoResponse != null) {
                                return ProcessResponse(epInfo, nodeService, fileInfoResponse);
                            }
                        }
                    }
                }
            } catch (Exception e) {
                Logs.CreateReport(e);
            }
            return false;
        }

        private List<JToken> GetQueuedEpisodesForDownload() {
            string text = iOController.ReadAllText(QUEUE_ANIMES_FILE);
            if (text == null) {
                Logging.Log("Waiting 5 minutes for new episodes");
                Thread.Sleep(/* 5 min */ 300000);
            } else {
                try {
                    List<JToken> animes = JObject.Parse(text)["array"].ToObject<JArray>().ToList();
                    List<JToken> episodes = new List<JToken>();
                    foreach (var anime in animes) {
                        string filepath = $"./animesinfo/{anime}.json";
                        if (File.Exists(filepath)) {
                            JArray jArray = iOController.ReadJArray(filepath);
                            foreach (var token in jArray) {
                                if (!((bool)token["downloaded"])) {
                                    token["nameUrl"] = anime;
                                    ((JObject)token).Remove("downloaded");
                                    episodes.Add(token);
                                }
                            }
                        }
                    }
                    return episodes;
                } catch (Exception e) {
                    Logs.CreateReport(e);
                }
            }
            return null;
        }
    }
}

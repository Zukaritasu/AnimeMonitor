using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AnimeMonitor.Services {
    internal class NodeService : IDisposable {

        private static readonly string NODE_EXECUTABLE_SERVICE = GetNodeExecutableService();
        private readonly NodeServiceConnection _serviceConn = null;
        private Process _process = null;

        private static string GetNodeExecutableService() {
            string executableFile = "ndservc.exe";
            if (File.Exists(Path.Combine(Application.StartupPath, executableFile)))
                return Path.Combine(Application.StartupPath, executableFile);
            return Path.GetFullPath($"../../DownloadManager/{executableFile}");
        }

        public NodeService(NodeServiceConnection serviceConn) {
            _serviceConn = serviceConn;
        }

        public NodeService Run() {
            if (_process == null)
                _process = Process.Start(GetProcessStartInfo());
            return this;
        }

        public void Dispose() {
            _process?.Dispose();
        }

        private ProcessStartInfo GetProcessStartInfo() {
            return new ProcessStartInfo() {
                FileName = NODE_EXECUTABLE_SERVICE,
                Arguments = $"{_serviceConn.Port}",
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                CreateNoWindow = true
            };
        }
    }
}

using AnimeMonitor.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AnimeMonitor.Services {
    internal class NodeServiceConnection : IDisposable {

        public int Port { get; private set; }

        private Socket _socket = null;
        private readonly byte[] _defaultBuffer = new byte[4098];
        private static readonly IPAddress DEFAULT_IPADDRESS = IPAddress.Parse("127.0.0.1");

        public NodeServiceConnection() {
            Port = GenerateRandomNumber();
        }

        private static int GenerateRandomNumber() {
            Random rnd = new Random();
            return rnd.Next(2911, 3110 + 1);
        }

        public string Read() {
            try {
                if (_socket != null && _socket.Connected) {
                    int length = _socket.Receive(_defaultBuffer);
                    if (length > 0) {
                        return Encoding.Default.GetString(_defaultBuffer, 0, length);
                    }
                }
            } catch (Exception e) {
                Logs.CreateReport(e);
            }
            return null;
        }

        public int Write(string text) {
            try {
                if (_socket != null && _socket.Connected)
                    return _socket.Send(Encoding.Default.GetBytes(text));
            } catch (Exception e) {
                Logs.CreateReport(e);
            }
            return -1;
        }

        public bool Connect() {
            try {
                TcpListener listener = new TcpListener(DEFAULT_IPADDRESS, Port);
                listener.Start();
                TimeSpan timeout = TimeSpan.FromSeconds(5);
                DateTime startTime = DateTime.Now;
                while (DateTime.Now - startTime < timeout) {
                    if (listener.Pending()) {
                        _socket = listener.AcceptSocket();
                        return true;
                    }
                }

                throw new Exception("Error: Connection timeout. No client connected.");

            } catch (Exception e) {
                Logs.CreateReport(e);
            }

            return false;
        }

        public void Dispose() {
            if (_socket != null) {
                _socket.Close();
            }
        }
    }
}

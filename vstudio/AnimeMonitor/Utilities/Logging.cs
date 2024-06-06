using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeMonitor.Utilities {
    public class Logging {

        private Logging() { }

        public static void Log(string message) {
            PrintMesaage("LOG", message);
        }

        public static void Error(string message) {
            PrintMesaage("ERR", message);
        }

        public static void Warning(string message) {
            PrintMesaage("WAR", message);
        }

        private static void PrintMesaage(string type, string message) {
            string currentTime = DateTime.Now.ToString("HH.mm.ss");
            Console.WriteLine($"[{currentTime} {type}] {message}");
        }
    }
}

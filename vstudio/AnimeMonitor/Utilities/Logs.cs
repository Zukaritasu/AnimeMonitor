using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeMonitor.Utilities {
    public sealed class Logs {

        public static readonly string DIRECTORY_LOGS = "./logs";

        private Logs() { }

        public static void CreateReport(Exception exception) {
#if DEBUG
            Console.WriteLine(exception.ToString());
#endif
            if (!Directory.Exists(DIRECTORY_LOGS))
                Directory.CreateDirectory(DIRECTORY_LOGS);
            string dateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

            using (StreamWriter writer = new StreamWriter($"{DIRECTORY_LOGS}/{dateTime}.log")) {
                writer.WriteLine($"Type exception: {exception.GetType().FullName}");
                writer.WriteLine($"Message: {exception.Message}");
                writer.WriteLine($"Stack Trace:\n{exception.StackTrace}");
            }
        }
    }
}

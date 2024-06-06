using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeMonitor.Services {
    public enum ResponseStatus {
        SUCCEEDED,
        FAILED,
        UNAUTHORIZED,
        /*  */
        DOWNLOAD_PROGRESS
    }
}

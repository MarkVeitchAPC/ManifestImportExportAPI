using Microsoft.Owin.Hosting;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManifestImportExportAPI.Service
{
    class ApiService
    {
        private IDisposable _webServer;
        private string _hostname;
        private const string EventSource = "ManifestImportExportAPIService";
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public ApiService(string hostname)
        {
            _hostname = hostname;
        }
        public void Start()
        {
            logger.Log(LogLevel.Info, "Starting ManifestImportExportAPIi server.");
            EventLog.WriteEntry(EventSource, "Starting ManifestImportExportAPI server.");
            _webServer = WebApp.Start<Startup>(_hostname);
        }

        public void Stop()
        {
            logger.Log(LogLevel.Info, "Stopping ManifestImportExportAPI server.");
            EventLog.WriteEntry(EventSource, "Stopping ManifestImportExportAPI server.");
            _webServer.Dispose();
        }
    }
}

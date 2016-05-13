using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace ManifestImportExportAPI.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            //Try it again
            string host = ConfigurationManager.AppSettings["host"];
            HostFactory.Run(configuration =>
            {
                configuration.Service<ApiService>(service =>
                {
                    service.ConstructUsing(name => new ApiService(host));
                    service.WhenStarted(tc => tc.Start());
                    service.WhenStopped(tc => tc.Stop());
                });
                configuration.RunAsLocalSystem();
                configuration.StartAutomatically();
                configuration.SetDescription("Manifest Import Export API");
                configuration.SetDisplayName("ManifestImportExportAPI.Service");
                configuration.SetServiceName("ManifestImportExportAPI.Service");
            });
        }
    }
}

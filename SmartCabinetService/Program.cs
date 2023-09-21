using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace SmartCabinetService
{
    public class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(serviceConfig =>

            {
                serviceConfig.UseNLog();
                serviceConfig.Service<SmartCabinetService>(serviceInstance =>
                {
                    serviceInstance.ConstructUsing(() => new SmartCabinetService());
                    serviceInstance.WhenStarted(execute => execute.StartAsync());
                    serviceInstance.WhenStopped(execute => execute.Stop());
                });

                serviceConfig.RunAsLocalSystem();
                serviceConfig.SetServiceName("RFiDDiscoverySmartCabinetService");
                serviceConfig.SetDisplayName("RFiD Discovery Smart Cabinet Service");

                serviceConfig.StartAutomatically();
            });
        }
    }
}

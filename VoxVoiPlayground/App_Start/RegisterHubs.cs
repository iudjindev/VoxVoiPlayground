using System.Web;
using System.Web.Routing;
using SignalR;

[assembly: PreApplicationStartMethod(typeof(VoxVoiPlayground.RegisterHubs), "Start")]

namespace VoxVoiPlayground
{
    public static class RegisterHubs
    {
        public static void Start()
        {
            // Register the default hubs route: ~/signalr/hubs
            RouteTable.Routes.MapHubs();            
        }
    }
}

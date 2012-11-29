using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SignalR.Hubs;
using Twilio;
using System.Configuration;

namespace VoxVoiPlayground.Hubs
{
    [HubName("queue")]
    public class QueueHub : Hub
    {
        readonly TwilioRestClient _client = new TwilioRestClient(ConfigurationManager.AppSettings["account"], ConfigurationManager.AppSettings["auth"]);

        public void GetAll()
        {
            var queues = _client.ListQueues().Queues.ToArray();
            Clients.getAllQueues(queues);
        }
    }
}
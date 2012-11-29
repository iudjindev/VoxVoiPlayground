using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwilioExtenders.Extenders
{
    public class TwilioQueueRequest: TwilioVoiceRequest
    {
        /* Queue extra params */
        public string QueueSid { get; set; }
        public string QueueTime { get; set; }
        public string AvgQueueTime { get; set; }
        public string DequeingCallSid { get; set; }
        public string QueueResult { get; set; }
        public string CurrentQueueSize { get; set; }
        public string QueuePosition { get; set; }
    }
}

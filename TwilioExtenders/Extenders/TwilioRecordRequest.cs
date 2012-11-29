using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwilioExtenders.Extenders
{
    public class TwilioRecordRequest: TwilioVoiceRequest
    {
        /* Record extra params */
        public string RecordingUrl { get; set; }
        public string RecordingDuration { get; set; }
        public string Digits { get; set; }
    }
}

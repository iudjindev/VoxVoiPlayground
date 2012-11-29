using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwilioExtenders.Extenders
{
    public class TwilioTranscribeRequest: TwilioVoiceRequest
    {
        public string RecordingSid { get; set; }
        public string TranscriptionSid { get; set; }
        public string TranscriptionText { get; set; }
        public string TranscriptionStatus { get; set; }
        public string TranscriptionUrl { get; set; }
    }
}

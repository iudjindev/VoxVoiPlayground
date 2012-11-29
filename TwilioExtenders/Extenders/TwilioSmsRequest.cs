using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwilioExtenders.Extenders
{
    public class TwilioSmsRequest: TwilioVoiceRequest
    {
        public string SmsSid { get; set;}
        public string SmsStatus { get; set; }
    }
}

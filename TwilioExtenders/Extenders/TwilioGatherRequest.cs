using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwilioExtenders.Extenders
{
    public class TwilioGatherRequest: TwilioVoiceRequest
    {
        /* Gather extra params */
        public string Digits { get; set; }

    }
}

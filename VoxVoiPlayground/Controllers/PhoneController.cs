using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SignalR;
using Twilio;
using Twilio.TwiML;
using TwilioExtenders.Extenders;

namespace VoxVoiPlayground.Controllers
{
    public class PhoneController : ApiController
    {
        readonly TwilioRestClient _client = new TwilioRestClient(ConfigurationManager.AppSettings["account"], ConfigurationManager.AppSettings["auth"]);
        readonly object _voicesettings = new {voice = "woman"};

        [HttpGet]
        public HttpResponseMessage SayHi()
        {
            var twilioResponse = new TwilioResponse();
            twilioResponse.Say("hi");

            return Request.CreateResponse(HttpStatusCode.OK, twilioResponse.Element);
        }

        [HttpGet]
        public HttpResponseMessage RemoveFromConference(string id)
        {
            var twilioResponse = new TwilioResponse();
            var conference = _client.ListConferences().Conferences.FirstOrDefault(name => name.FriendlyName.Equals(id));

            var participants = _client.ListConferenceParticipants(conference.Sid, false);

            foreach (var participant in participants.Participants)
                _client.KickConferenceParticipant(participant.ConferenceSid, participant.CallSid);

            return Request.CreateResponse(HttpStatusCode.OK, twilioResponse.Element);
        }

        [HttpPost]
        public HttpResponseMessage AnswerIncomingCall()
        {
            var twilioResponse = new TwilioResponse();
            twilioResponse.BeginGather(
                new
                    {
                        action = Url.Link("ExtendedApi", new {controller = "Phone", action = "HandleConferenceCall"}),
                        numDigits = 1
                    });
            twilioResponse.Say(
                "Welcome to VoxVoi. If you have a conference number, please press 2. Otherwise, please stay on your line.",
                _voicesettings);

            twilioResponse.EndGather();

            twilioResponse.Redirect(
                Url.Link("ExtendedApi", new {controller = "Phone", action = "HandleMenuCall"}),
                "POST");

            return Request.CreateResponse(HttpStatusCode.OK, twilioResponse.Element);
        }

        [HttpPost]
        public HttpResponseMessage HandleConferenceCall(TwilioGatherRequest twilioRequest)
        {
            var digits = twilioRequest.Digits;
            var twilioResponse = new TwilioResponse();

            var conference =
                _client.ListConferences().Conferences.FirstOrDefault(name => name.FriendlyName.Equals(digits));

            if (digits == "2")
            {
                twilioResponse.BeginGather(
                    new
                    {
                        action = Url.Link("ExtendedApi", new { controller = "Phone", action = "CreateOrJoinConferenceCall" }),
                        finishOnKey = "#",
                        timeout = 8
                    });
                twilioResponse.Say("Please enter the number of conference call you want to join, followed by the pound key.", _voicesettings);
                twilioResponse.EndGather();
            }
            else if (conference == null)
            {
                twilioResponse.BeginGather(
                    new
                        {
                            action =
                        Url.Link("ExtendedApi", new {controller = "Phone", action = "HandleConferenceCall"}),
                            numDigits = 1
                        });
                twilioResponse.Say(
                    string.Format(
                        "A conference with number {0} doesn't exist. If you wish to create a conference Press 1, otherwise wait 5 seconds to return to the main menu",
                        digits), _voicesettings);

                twilioResponse.EndGather();

                /* Stop gather and inform falling back to the main menu */
                twilioResponse.Say("No conference number was provided. You're returning to the main menu. Thank you.",
                                   _voicesettings);

                /* Fall back to main menu */
                twilioResponse.Redirect(Url.Link("ExtendedApi", new {controller = "Phone", action = "AnswerIncomingCall"}));
            }
            else if(digits == "1")
            {
                twilioResponse.BeginGather(
                    new
                        {
                            action = Url.Link("ExtendedApi", new { controller = "Phone", action = "CreateOrJoinConferenceCall"}),
                            finishOnKey = "#",
                            timeout = 8
                        });
                twilioResponse.Say("Please enter the number of conference call you want to create, followed by the pound key.", _voicesettings);
                twilioResponse.EndGather();
            }
           
            return Request.CreateResponse(HttpStatusCode.OK, twilioResponse.Element);
        }

        public HttpResponseMessage GoodByeMessage(TwilioVoiceRequest twilioVoiceRequest)
        {
            var twilioResponse = new TwilioResponse();

            twilioResponse.Say("Thank you for using VoxVoi. Goodbye!", _voicesettings);
            twilioResponse.Hangup();

            return Request.CreateResponse(HttpStatusCode.OK, twilioResponse.Element);
        }

        [HttpPost]
        public HttpResponseMessage CreateOrJoinConferenceCall(TwilioGatherRequest twilioRequest)
        {
            var conference = _client.ListConferences().Conferences.FirstOrDefault(name => name.FriendlyName.Equals(twilioRequest.Digits));

            var twilioResponse = new TwilioResponse();
            var digits = twilioRequest.Digits;

            twilioResponse.Say(string.Format("You're now joining conference {0}. Thank you.", digits), _voicesettings);

            if (conference != null)
            {
                var participant = _client.GetConferenceParticipant(conference.Sid, twilioRequest.CallSid);
                var context = GlobalHost.ConnectionManager.GetHubContext<Hubs.ConferenceHub>();
                context.Clients.addParticipant(participant.ConferenceSid, participant.CallSid, participant.Muted);
            }
            
            twilioResponse.DialConference(twilioRequest.Digits, new { waitUrl = "http://twimlets.com/holdmusic?Bucket=com.twilio.music.rock", endConferenceOnExit = true });
            twilioResponse.Redirect(Url.Link("ExtendedApi", new { controller = "Phone", action = "GoodByeMessage" }));

           
            return Request.CreateResponse(HttpStatusCode.OK, twilioResponse.Element);
        }

        [HttpPost]
        public HttpResponseMessage HandleMenuCall(TwilioGatherRequest twilioRequest)
        {
            var twilioResponse = new TwilioResponse();

            twilioResponse.Say("Welcome to VoxVoi", _voicesettings);
            twilioResponse.BeginGather(
                   new
                   {
                       action =
                   Url.Link("ExtendedApi", new { controller = "Phone", action = "ProcessMenuCall" }),
                       numDigits = 1
                   });

            twilioResponse.Say("Press 1 to connect with an agent", _voicesettings);
            twilioResponse.Say("Press 2 to be awesome", _voicesettings);
            twilioResponse.Say("Press 3 to leave a message", _voicesettings);
            twilioResponse.Say("Press 0 to return to the main menu", _voicesettings);

            twilioResponse.EndGather();
            twilioResponse.Redirect(Url.Link("ExtendedApi", new {controller = "Phone", action = "HandleMenuCall"}));

            return Request.CreateResponse(HttpStatusCode.OK, twilioResponse.Element);
        }

        [HttpPost]
        public HttpResponseMessage ProcessMenuCall(TwilioGatherRequest twilioRequest)
        {
            var twilioResponse = new TwilioResponse();
            var digits = twilioRequest.Digits;

            switch (digits)
            {
                case "1":
                    twilioResponse.Redirect(Url.Link("ExtendedApi", new { controller = "Phone", action = "JoinQueue" }));
                    break;
                case "2":
                    twilioResponse.Say("You're awesome!", _voicesettings);
                    break;
                case "3":
                    twilioResponse.Redirect(Url.Link("ExtendedApi", new { controller = "Phone", action = "RecordMessage" }));
                    break;
                case "0":
                    twilioResponse.Redirect(Url.Link("ExtendedApi", new { controller = "Phone", action = "AnswerIncomingCall" }));
                    break;
            }
            
            twilioResponse.Redirect(Url.Link("ExtendedApi", new { controller = "Phone", action = "HandleMenuCall" }));

            return Request.CreateResponse(HttpStatusCode.OK, twilioResponse.Element);
        }

        [HttpPost]
        public HttpResponseMessage RecordMessage(TwilioRecordRequest twilioRequest)
        {
            var twilioResponse = new TwilioResponse();

            twilioResponse.Say("Please leave your message after the beep and finish with the pound key.", _voicesettings);
            twilioResponse.Say("You have 6 seconds.", _voicesettings);
            twilioResponse.Record(
                new
                    {
                        action = Url.Link("ExtendedApi", new {controller = "Phone", action = "HandleRecordedMessage"}),
                        finishOnkey = "#",
                        maxLength = "6",
                        method = "POST",
                        transcribe = "true"
                    });
            twilioResponse.Say("We did not receive a recording. You're returning to the previous menu.", _voicesettings);
            twilioResponse.Redirect(Url.Link("ExtendedApi", new { controller = "Phone", action = "HandleMenuCall" }));

            return Request.CreateResponse(HttpStatusCode.OK, twilioResponse.Element);
        }

        
        [HttpPost]
        public HttpResponseMessage HandleRecordedMessage(TwilioRecordRequest twilioRequest)
        {
            var twilioResponse = new TwilioResponse();

            twilioResponse.Say("Your message has been recorded.", _voicesettings);
            twilioResponse.Redirect(Url.Link("ExtendedApi", new { controller = "Phone", action = "GoodByeMessage" }));

            return Request.CreateResponse(HttpStatusCode.OK, twilioResponse.Element);
        }


        [HttpPost]
        public HttpResponseMessage JoinQueue(TwilioQueueRequest twilioRequest)
        {
            var twilioResponse = new TwilioResponse();
            twilioResponse.Say("You're now joining the Priority Queue", _voicesettings);
            twilioResponse.Enqueue("PriorityQueue",
                                   new
                                       {
                                           action = Url.Link("ExtendedApi", new {controller = "Phone", action = "LeaveQueue"}),
                                           waitUrl = Url.Link("ExtendedApi", new {controller = "Phone", action = "WaitInQueue"})
                                       });
            return Request.CreateResponse(HttpStatusCode.OK, twilioResponse.Element);
        }

        [HttpPost]
        public HttpResponseMessage LeaveQueue(TwilioQueueRequest twilioRequest)
        {
            var twilioResponse = new TwilioResponse();

            twilioResponse.Say("We're sorry but there is no agent available at the moment.", _voicesettings);
            twilioResponse.Redirect(Url.Link("ExtendedApi", new { controller = "Phone", action = "GoodByeMessage" }));

            return Request.CreateResponse(HttpStatusCode.OK, twilioResponse.Element);
        }

        [HttpPost]
        public HttpResponseMessage WaitInQueue(TwilioQueueRequest twilioRequest)
        {
            var twilioResponse = new TwilioResponse();

            twilioResponse.Say(string.Format("You are number {0} out of {1} in the queue, please stay on your line.",
                                             twilioRequest.CurrentQueueSize, twilioRequest.QueuePosition), _voicesettings);
            twilioResponse.Redirect("http://twimlets.com/holdmusic?Bucket=com.twilio.music.rock");

            return Request.CreateResponse(HttpStatusCode.OK, twilioResponse.Element);
        }
    }
}

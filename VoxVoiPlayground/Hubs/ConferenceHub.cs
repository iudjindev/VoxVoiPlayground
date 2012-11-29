using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using SignalR.Hubs;
using Twilio;

namespace VoxVoiPlayground.Hubs
{
    [HubName("conference")]
    public class ConferenceHub: Hub
    {
        readonly TwilioRestClient _client = new TwilioRestClient(ConfigurationManager.AppSettings["account"], ConfigurationManager.AppSettings["auth"]);

        /// <summary>
        /// Add a participant to the list of participants
        /// </summary>
        /// <param name="confId"></param>
        /// <param name="callId"></param>
        /// <param name="muted"></param>
        public void AddParticipant(string confId, string callId, string muted)
        {
            Clients.addParticipant(confId, callId, muted);
        }

        /// <summary>
        /// Get all conferences available and provide them back to the caller
        /// </summary>
        public void GetAll()
        {
            var conferences = _client.ListConferences().Conferences.ToArray();
            // Call function on client (javascript)
            Caller.getAllConferenceCalls(conferences);
        }
        /// <summary>
        /// Kick a participant from a conference
        /// </summary>
        /// <param name="confId"></param>
        /// <param name="callId"></param>
        public void KickParticipant(string confId, string callId)
        {
            _client.KickConferenceParticipant(confId, callId);
            Clients.kickParticipant(confId, callId);
        }

        public void MuteParticipant(string confId, string callId)
        {
            _client.MuteConferenceParticipant(confId, callId);

            /* Refresh participants */
            var participants = _client.ListConferenceParticipants(confId, null).Participants.ToArray();
            Clients.getAllParticipants(participants);
        }

        public void UnMuteParticipant(string confId, string callId)
        {
            _client.UnmuteConferenceParticipant(confId, callId);

            /* Refresh participants */
            var participants = _client.ListConferenceParticipants(confId, null).Participants.ToArray();
            Clients.getAllParticipants(participants);
        }
        /// <summary>
        /// List all participants in a conference
        /// </summary>
        /// <param name="confId"></param>
        public void ListParticipants(string confId)
        {
            var participants = _client.ListConferenceParticipants(confId, null).Participants.ToArray();
            Caller.getAllParticipants(participants);
        }
    }
}
#if !DISABLESTEAMWORKS && HE_STEAMCOMPLETE
using Steamworks;
using System;

namespace HeathenEngineering.SteamworksIntegration
{
    /// <summary>
    /// Represents an authentication session and carries unique information about the session request such as the user the session is inrealation to and the ticket data of the session.
    /// </summary>
    [Serializable]
    public class AuthenticationSession
    {
        /// <summary>
        /// Indicates that this session is being managed by a client or server
        /// </summary>
        public bool isClientSession = true;
        /// <summary>
        /// The user this session is in relation to
        /// </summary>
        public CSteamID user;
        /// <summary>
        /// The ID of the user that owns the game the user of this session is playing ... e.g. if this differes from the user then this is a barrowed game.
        /// </summary>
        public CSteamID gameOwner;
        /// <summary>
        /// The session data aka the 'ticket' data.
        /// </summary>
        public byte[] data;
        /// <summary>
        /// The responce recieved when validating a provided ticket.
        /// </summary>
        public EAuthSessionResponse responce;
        /// <summary>
        /// If true then the game this user is playing is barrowed from another user e.g. this user does not have a license for this game but is barrowing it from another user.
        /// </summary>
        public bool IsBarrowed { get { return user != gameOwner; } }
        /// <summary>
        /// The callback deligate to be called when the authenticate session responce returns from the steam backend.
        /// </summary>
        public Action<AuthenticationSession> onStartCallback;

        /// <summary>
        /// Ends the authentication session.
        /// </summary>
        public void End()
        {
            if (isClientSession)
                SteamUser.EndAuthSession(user);
            else
                SteamGameServer.EndAuthSession(user);
        }
    }

}
#endif
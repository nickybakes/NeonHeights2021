#if !DISABLESTEAMWORKS
using HeathenEngineering.Events;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration
{
    public class SteamSystemEvents : MonoBehaviour
    {
        public SteamSettings settings;
        public UnityEvent evtSteamInitalized = new UnityEvent();
        /// <summary>
        /// An event raised when an error has occred while intializing the Steamworks API
        /// </summary>
        public UnityStringEvent evtSteamInitalizationError = new UnityStringEvent();
        private void Awake()
        {
            settings.evtSteamInitalized.AddListener(evtSteamInitalized.Invoke);
            settings.evtSteamInitalizationError.AddListener(evtSteamInitalizationError.Invoke);
        }

        private void OnDestroy()
        {
            settings.evtSteamInitalized.RemoveListener(evtSteamInitalized.Invoke);
            settings.evtSteamInitalizationError.RemoveListener(evtSteamInitalizationError.Invoke);
        }
    }
}
#endif
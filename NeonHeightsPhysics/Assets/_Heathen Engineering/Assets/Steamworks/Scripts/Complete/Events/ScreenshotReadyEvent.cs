﻿#if !DISABLESTEAMWORKS && HE_STEAMCOMPLETE
using Steamworks;
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration
{
    [System.Serializable]
    public class ScreenshotReadyEvent : UnityEvent<ScreenshotReady_t> { }
}
#endif
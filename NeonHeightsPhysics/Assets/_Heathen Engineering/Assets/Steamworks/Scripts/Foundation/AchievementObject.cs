#if !DISABLESTEAMWORKS
using Steamworks;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration
{
    /// <summary>
    /// A <see cref="ScriptableObject"/> containing the definition of a Steamworks Achievement.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note that this object simply contains the definition of an achievement that has been created in the Steamworks API.
    /// for more information please see <a href="https://partner.steamgames.com/doc/features/achievements">https://partner.steamgames.com/doc/features/achievements</a>
    /// </para>
    /// </remarks>
    [HelpURL("https://kb.heathenengineering.com/assets/steamworks/achievement-object")]
    [CreateAssetMenu(menuName = "Steamworks/Achievement Object")]
    public class AchievementObject : ScriptableObject
    {
        /// <summary>
        /// The API Name as it appears in the Steamworks portal.
        /// </summary>
        [HideInInspector]
        public string achievementId;

        /// <summary>
        /// Indicates that this achievment has been unlocked by this user.
        /// </summary>
        /// <remarks>
        /// Only available on client builds
        /// </remarks>
        public bool IsAchieved
        {
            get
            {
                if (API.StatsAndAchievements.Client.GetAchievement(achievementId, out bool status))
                    return status;
                else
                    return false;
            }
            set
            {
                if (value)
                    API.StatsAndAchievements.Client.SetAchievement(achievementId);
                else
                    API.StatsAndAchievements.Client.ClearAchievement(achievementId);
            }
        }
        /// <summary>
        /// The display name for this achievement.
        /// </summary>
        /// <remarks>
        /// Only available on client builds
        /// </remarks>
        [HideInInspector]
        public string displayName;
        /// <summary>
        /// The display description for this achievement.
        /// </summary>
        /// <remarks>
        /// Only available on client builds
        /// </remarks>
        [HideInInspector]
        public string displayDescription;
        /// <summary>
        /// Is this achievement a hidden achievement.
        /// </summary>
        /// <remarks>
        /// Only available on client builds
        /// </remarks>
        [HideInInspector]
        public bool hidden;

        /// <summary>
        /// <para>Unlocks the achievement.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#SetAchievement">https://partner.steamgames.com/doc/api/ISteamUserStats#SetAchievement</a>
        /// </summary>
        public void Unlock() => IsAchieved = true;

        /// <summary>
        /// <para>Resets the unlock status of an achievmeent.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#ClearAchievement">https://partner.steamgames.com/doc/api/ISteamUserStats#ClearAchievement</a>
        /// </summary>
        public void ClearAchievement() => IsAchieved = false;

        /// <summary>
        /// Unlock the achievement for the <paramref name="user"/>
        /// </summary>
        /// <remarks>
        /// Only available on server builds
        /// </remarks>
        /// <param name="user"></param>
        public void Unlock(CSteamID user)
        {
            API.StatsAndAchievements.Server.SetUserAchievement(user, achievementId);
        }

        /// <summary>
        /// Clears the achievement for the <paramref name="user"/>
        /// </summary>
        /// <remarks>
        /// Only available on server builds
        /// </remarks>
        /// <param name="user"></param>
        public void ClearAchievement(CSteamID user)
        {
            API.StatsAndAchievements.Server.ClearUserAchievement(user, achievementId);
        }

        /// <summary>
        /// Gets the achievement status for the <paramref name="user"/>
        /// </summary>
        /// <remarks>
        /// Only available on server builds
        /// </remarks>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool GetAchievementStatus(CSteamID user)
        {
            bool achieved;
            API.StatsAndAchievements.Server.GetUserAchievement(user, achievementId, out achieved);
            return achieved;
        }

        public void Store() => API.StatsAndAchievements.Client.StoreStats();

    }
}
#endif
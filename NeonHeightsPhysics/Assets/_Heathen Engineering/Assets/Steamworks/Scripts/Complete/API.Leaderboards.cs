#if !DISABLESTEAMWORKS && HE_STEAMCOMPLETE
using Steamworks;
using System;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration.API
{
    public static class Leaderboards
    {
        public static class Client
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void Init()
            {
                m_LeaderboardUGCSet_t = null;
                m_LeaderboardScoresDownloaded_t = null;
                m_LeaderboardFindResult_t = null;
                m_LeaderboardScoreUploaded_t = null;
            }

            private static CallResult<LeaderboardUGCSet_t> m_LeaderboardUGCSet_t;
            private static CallResult<LeaderboardScoresDownloaded_t> m_LeaderboardScoresDownloaded_t;
            private static CallResult<LeaderboardFindResult_t> m_LeaderboardFindResult_t;
            private static CallResult<LeaderboardScoreUploaded_t> m_LeaderboardScoreUploaded_t;

            /// <summary>
            /// Attaches a piece of user generated content the current user's entry on a leaderboard.
            /// </summary>
            /// <remarks>
            /// This content could be a replay of the user achieving the score or a ghost to race against. The attached handle will be available when the entry is retrieved and can be accessed by other users using GetDownloadedLeaderboardEntry which contains LeaderboardEntry_t.m_hUGC. To create and download user generated content see the documentation for the Steam Workshop.
            /// </remarks>
            /// <param name="leaderboard">A leaderboard handle obtained from FindLeaderboard or FindOrCreateLeaderboard.</param>
            /// <param name="ugc">Handle to a piece of user generated content that was shared using RemoteStorage.FileShare.</param>
            /// <param name="callback"></param>
            public static void AttachUGC(SteamLeaderboard_t leaderboard, UGCHandle_t ugc, Action<LeaderboardUGCSet_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_LeaderboardUGCSet_t == null)
                    m_LeaderboardUGCSet_t = CallResult<LeaderboardUGCSet_t>.Create();

                var handle = SteamUserStats.AttachLeaderboardUGC(leaderboard, ugc);
                m_LeaderboardUGCSet_t.Set(handle, callback.Invoke);
            }
            /// <summary>
            /// Attaches a piece of user generated content the current user's entry on a leaderboard.
            /// </summary>
            /// <remarks>
            /// This content could be a replay of the user achieving the score or a ghost to race against. The attached handle will be available when the entry is retrieved and can be accessed by other users using GetDownloadedLeaderboardEntry which contains LeaderboardEntry_t.m_hUGC. To create and download user generated content see the documentation for the Steam Workshop.
            /// </remarks>
            public static void AttachUGC(SteamLeaderboard_t leaderboard, string fileName, byte[] data, Action<LeaderboardUGCSet_t, bool> callback)
            {
                if (callback == null)
                    return;

                API.RemoteStorage.Client.FileWriteAsync(fileName, data, (writeResult, writeError) =>
                {
                    if(!writeError)
                    {
                        API.RemoteStorage.Client.FileShare(fileName, (shareResult, shareError) =>
                        {
                            if (!shareError)
                            {
                                AttachUGC(leaderboard, shareResult.m_hFile, callback);
                            }
                            else
                            {
                                callback.Invoke(new LeaderboardUGCSet_t
                                {
                                    m_eResult = shareResult.m_eResult,
                                    m_hSteamLeaderboard = leaderboard
                                },
                                true);
                            }
                        });
                    }
                    else
                    {
                        callback.Invoke(new LeaderboardUGCSet_t
                        {
                            m_eResult = writeResult.m_eResult,
                            m_hSteamLeaderboard = leaderboard
                        },
                        true);
                    }
                });
            }
            public static void AttachUGC(SteamLeaderboard_t leaderboard, string fileName, object jsonObject, System.Text.Encoding encoding, Action<LeaderboardUGCSet_t, bool> callback)
            {
                if (callback == null)
                    return;

                API.RemoteStorage.Client.FileWriteAsync(fileName, jsonObject, encoding, (writeResult, writeError) =>
                {
                    if (!writeError)
                    {
                        API.RemoteStorage.Client.FileShare(fileName, (shareResult, shareError) =>
                        {
                            if (!shareError)
                            {
                                AttachUGC(leaderboard, shareResult.m_hFile, callback);
                            }
                            else
                            {
                                callback.Invoke(new LeaderboardUGCSet_t
                                {
                                    m_eResult = shareResult.m_eResult,
                                    m_hSteamLeaderboard = leaderboard
                                },
                                true);
                            }
                        });
                    }
                    else
                    {
                        callback.Invoke(new LeaderboardUGCSet_t
                        {
                            m_eResult = writeResult.m_eResult,
                            m_hSteamLeaderboard = leaderboard
                        },
                        true);
                    }
                });
            }
            public static void AttachUGC(SteamLeaderboard_t leaderboard, string fileName, string content, System.Text.Encoding encoding, Action<LeaderboardUGCSet_t, bool> callback)
            {
                if (callback == null)
                    return;

                API.RemoteStorage.Client.FileWriteAsync(fileName, content, encoding, (writeResult, writeError) =>
                {
                    if (!writeError)
                    {
                        API.RemoteStorage.Client.FileShare(fileName, (shareResult, shareError) =>
                        {
                            if (!shareError)
                            {
                                AttachUGC(leaderboard, shareResult.m_hFile, callback);
                            }
                            else
                            {
                                callback.Invoke(new LeaderboardUGCSet_t
                                {
                                    m_eResult = shareResult.m_eResult,
                                    m_hSteamLeaderboard = leaderboard
                                },
                                true);
                            }
                        });
                    }
                    else
                    {
                        callback.Invoke(new LeaderboardUGCSet_t
                        {
                            m_eResult = writeResult.m_eResult,
                            m_hSteamLeaderboard = leaderboard
                        },
                        true);
                    }
                });
            }
            /// <summary>
            /// Fetches a series of leaderboard entries for a specified leaderboard.
            /// </summary>
            /// <param name="leaderboard">A leaderboard handle obtained from FindLeaderboard or FindOrCreateLeaderboard.</param>
            /// <param name="request">The type of data request to make.</param>
            /// <param name="start">The index to start downloading entries relative to eLeaderboardDataRequest.</param>
            /// <param name="end">The last index to retrieve entries for relative to eLeaderboardDataRequest.</param>
            public static void DownloadEntries(SteamLeaderboard_t leaderboard, ELeaderboardDataRequest request, int start, int end, int maxDetailsPerEntry, Action<LeaderboardEntry[], bool> callback)
            {
                if (callback == null)
                    return;

                if (m_LeaderboardScoresDownloaded_t == null)
                    m_LeaderboardScoresDownloaded_t = CallResult<LeaderboardScoresDownloaded_t>.Create();

                var handle = SteamUserStats.DownloadLeaderboardEntries(leaderboard, request, start, end);
                m_LeaderboardScoresDownloaded_t.Set(handle, (results, error) =>
                {
                    callback.Invoke(ProcessScoresDownloaded(results, error, maxDetailsPerEntry), error);
                });
            }
            /// <summary>
            /// Fetches leaderboard entries for an arbitrary set of users on a specified leaderboard.
            /// </summary>
            /// <remarks>
            /// A maximum of 100 users can be downloaded at a time, with only one outstanding call at a time. If a user doesn't have an entry on the specified leaderboard, they won't be included in the result.
            /// </remarks>
            /// <param name="leaderboard">A leaderboard handle obtained from FindLeaderboard or FindOrCreateLeaderboard.</param>
            /// <param name="users">An array of Steam IDs to get the leaderboard entries for.</param>
            /// <param name="callback"></param>
            public static void DownloadEntries(SteamLeaderboard_t leaderboard, CSteamID[] users, int maxDetailsPerEntry, Action<LeaderboardEntry[], bool> callback)
            {
                if (callback == null)
                    return;

                if (m_LeaderboardScoresDownloaded_t == null)
                    m_LeaderboardScoresDownloaded_t = CallResult<LeaderboardScoresDownloaded_t>.Create();

                var handle = SteamUserStats.DownloadLeaderboardEntriesForUsers(leaderboard, users, users.Length);
                m_LeaderboardScoresDownloaded_t.Set(handle, (results, error) =>
                {
                    callback.Invoke(ProcessScoresDownloaded(results, error, maxDetailsPerEntry), error);
                });
            }
            public static void DownloadEntries(SteamLeaderboard_t leaderboard, UserData[] users, int maxDetailsPerEntry, Action<LeaderboardEntry[], bool> callback) => DownloadEntries(leaderboard, Array.ConvertAll(users, (i) => i.cSteamId), maxDetailsPerEntry, callback);
            /// <summary>
            /// Gets a leaderboard by name.
            /// </summary>
            /// <param name="leaderboardName">The name of the leaderboard to find. Must not be longer than <see cref="Constants.k_cchLeaderboardNameMax"/>.</param>
            /// <param name="callback"></param>
            public static void Find(string leaderboardName, Action<LeaderboardFindResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_LeaderboardFindResult_t == null)
                    m_LeaderboardFindResult_t = CallResult<LeaderboardFindResult_t>.Create();

                var handle = SteamUserStats.FindLeaderboard(leaderboardName);
                m_LeaderboardFindResult_t.Set(handle, callback.Invoke);
            }
            /// <summary>
            /// Gets a leaderboard by name, it will create it if it's not yet created.
            /// </summary>
            /// <remarks>
            /// Leaderboards created with this function will not automatically show up in the Steam Community. You must manually set the Community Name field in the App Admin panel of the Steamworks website. As such it's generally recommended to prefer creating the leaderboards in the App Admin panel on the Steamworks website and using FindLeaderboard unless you're expected to have a large amount of dynamically created leaderboards.
            /// </remarks>
            /// <param name="leaderboardName">The name of the leaderboard to find. Must not be longer than <see cref="Constants.k_cchLeaderboardNameMax"/>.</param>
            /// <param name="sortingMethod">The sort order of the new leaderboard if it's created.</param>
            /// <param name="displayType">The display type (used by the Steam Community web site) of the new leaderboard if it's created.</param>
            /// <param name="callback"></param>
            public static void FindOrCreate(string leaderboardName, ELeaderboardSortMethod sortingMethod, ELeaderboardDisplayType displayType, Action<LeaderboardFindResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (sortingMethod == ELeaderboardSortMethod.k_ELeaderboardSortMethodNone)
                {
                    Debug.LogError("You should never pass ELeaderboardSortMethod.k_ELeaderboardSortMethodNone for the sorting method as this is undefined behaviour.");
                    return;
                }

                if (displayType == ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNone)
                {
                    Debug.LogError("You should never pass ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNone for the display type as this is undefined behaviour.");
                    return;
                }

                if (m_LeaderboardFindResult_t == null)
                    m_LeaderboardFindResult_t = CallResult<LeaderboardFindResult_t>.Create();

                var handle = SteamUserStats.FindOrCreateLeaderboard(leaderboardName, sortingMethod, displayType);
                m_LeaderboardFindResult_t.Set(handle, callback.Invoke);
            }
            /// <summary>
            /// Returns the display type of a leaderboard handle.
            /// </summary>
            /// <param name="leaderboard"></param>
            /// <returns></returns>
            public static ELeaderboardDisplayType GetDisplayType(SteamLeaderboard_t leaderboard) => SteamUserStats.GetLeaderboardDisplayType(leaderboard);
            /// <summary>
            /// Returns the total number of entries in a leaderboard.
            /// </summary>
            /// <param name="leaderboard"></param>
            /// <returns></returns>
            public static int GetEntryCount(SteamLeaderboard_t leaderboard) => SteamUserStats.GetLeaderboardEntryCount(leaderboard);
            /// <summary>
            /// Returns the name of a leaderboard handle.
            /// </summary>
            /// <param name="leaderboard"></param>
            /// <returns></returns>
            public static string GetName(SteamLeaderboard_t leaderboard) => SteamUserStats.GetLeaderboardName(leaderboard);
            /// <summary>
            /// Returns the sort order of a leaderboard handle.
            /// </summary>
            /// <param name="leaderboard"></param>
            /// <returns></returns>
            public static ELeaderboardSortMethod GetSortMethod(SteamLeaderboard_t leaderboard) => SteamUserStats.GetLeaderboardSortMethod(leaderboard);
            /// <summary>
            /// Uploads a user score to a specified leaderboard.
            /// </summary>
            /// <remarks>
            /// Details are optional game-defined information which outlines how the user got that score. For example if it's a racing style time based leaderboard you could store the timestamps when the player hits each checkpoint. If you have collectibles along the way you could use bit fields as booleans to store the items the player picked up in the playthrough.
            /// <para>
            /// Uploading scores to Steam is rate limited to 10 uploads per 10 minutes and you may only have one outstanding call to this function at a time.
            /// </para>
            /// </remarks>
            /// <param name="leaderboard">A leaderboard handle obtained from FindLeaderboard or FindOrCreateLeaderboard.</param>
            /// <param name="method">Do you want to force the score to change, or keep the previous score if it was better?</param>
            /// <param name="score">The score to upload.</param>
            /// <param name="details"></param>
            public static void UploadScore(SteamLeaderboard_t leaderboard, ELeaderboardUploadScoreMethod method, int score, int[] details, Action<LeaderboardScoreUploaded_t, bool> callback = null)
            {
                if (m_LeaderboardScoreUploaded_t == null)
                    m_LeaderboardScoreUploaded_t = CallResult<LeaderboardScoreUploaded_t>.Create();

                var handle = SteamUserStats.UploadLeaderboardScore(leaderboard, method, score, details, details == null ? 0 : details.Length);
                if (callback != null)
                    m_LeaderboardScoreUploaded_t.Set(handle, callback.Invoke);
            }

            private static LeaderboardEntry[] ProcessScoresDownloaded(LeaderboardScoresDownloaded_t param, bool bIOFailure, int maxDetailEntries)
            {
                ///Check for the current users data in the record set and update accordingly
                if (!bIOFailure)
                {
                    var userId = SteamUser.GetSteamID();
                    var entries = new LeaderboardEntry[param.m_cEntryCount];

                    for (int i = 0; i < param.m_cEntryCount; i++)
                    {
                        LeaderboardEntry_t buffer;
                        int[] details = null;

                        if (maxDetailEntries < 1)
                            SteamUserStats.GetDownloadedLeaderboardEntry(param.m_hSteamLeaderboardEntries, i, out buffer, details, maxDetailEntries);
                        else
                        {
                            details = new int[maxDetailEntries];
                            SteamUserStats.GetDownloadedLeaderboardEntry(param.m_hSteamLeaderboardEntries, i, out buffer, details, maxDetailEntries);
                        }

                        LeaderboardEntry record = new LeaderboardEntry();
                        record.entry = buffer;
                        record.details = details;

                        entries[i] = record;
                    }

                    return entries;
                }
                else
                    return new LeaderboardEntry[0];
            }

        }
    }

}
#endif
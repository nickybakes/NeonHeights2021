#if HE_STEAMCOMPLETE
using UnityEngine;
using UGC = HeathenEngineering.SteamworksIntegration.API.UserGeneratedContent.Client;
using HeathenEngineering.SteamworksIntegration;
using System.Collections.Generic;
using System;

namespace HeathenEngineering.DEMO
{
    /// <summary>
    /// This is for demonstration purposes only
    /// </summary>
    [System.Obsolete("This script is for demonstration purposes ONLY")]
    public class Scene7Behaviour : MonoBehaviour
    {
        public UserGeneratedContentQueryManager queryManager;
        public UnityEngine.UI.Text pageCount;
        public UnityEngine.UI.InputField searchInput;
        public GameObject recordTemplate;
        public Transform contentRoot;

        private List<GameObject> currentRecords = new List<GameObject>();

        public void QueryItems()
        {
            queryManager.SearchAll(searchInput.text);
        }

        public void UpdateResults(List<UGCCommunityItem> results)
        {
            pageCount.text = queryManager.activeQuery.Page.ToString() + " of " + queryManager.activeQuery.pageCount.ToString();

            while(currentRecords.Count > 0)
            {
                var target = currentRecords[0];
                currentRecords.Remove(target);
                Destroy(target);
            }

            foreach(var result in results)
            {
                var go = Instantiate(recordTemplate, contentRoot);
                currentRecords.Add(go);
                var comp = go.GetComponent<Scene7DisplayItem>();
                comp.AssignResult(result);
            }
        }
    }
}
#endif
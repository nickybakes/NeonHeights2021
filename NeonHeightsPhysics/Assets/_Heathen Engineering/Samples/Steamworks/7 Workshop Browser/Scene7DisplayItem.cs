#if HE_STEAMCOMPLETE
using UnityEngine;
using HeathenEngineering.SteamworksIntegration;

namespace HeathenEngineering.DEMO
{
    /// <summary>
    /// This is for demonstration purposes only
    /// </summary>
    [System.Obsolete("This script is for demonstration purposes ONLY")]
    public class Scene7DisplayItem : MonoBehaviour
    {
        public UnityEngine.UI.RawImage previewImage;
        public UnityEngine.UI.Text title;

        private UGCCommunityItem record;

        private void OnDestroy()
        {
            if (record != null)
                record.previewImageUpdated.AddListener(HandleImageUpdate);
        }

        public void AssignResult(UGCCommunityItem item)
        {
            record = item;

            record.previewImageUpdated.AddListener(HandleImageUpdate);

            title.text = record.Title;

            if (record.previewImage != null)
                previewImage.texture = record.previewImage;
        }

        private void HandleImageUpdate()
        {
            if (record.previewImage != null)
                previewImage.texture = record.previewImage;
        }
    }
}
#endif
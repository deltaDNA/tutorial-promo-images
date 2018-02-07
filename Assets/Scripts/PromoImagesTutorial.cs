using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using DeltaDNA;

namespace TutoralPromoImages
{
    // Promo image tutorial using Engage In-Game campaigns containing image messages
    // to populate Promo Image banners in various fixed slots in UI. 
    // Promo Images are Prefabs that can be positioned and sized by game designer
    // But their content and targeting can be controlled by marketer from deltDNA Platform.

    // Notes :: MVP , still to apply final Promo Image content (images and deeplinks, rewards, challenges etc..)
    // All Campaigns and actions used by this tutorial are on the LIVE environment of the deltaDNA Demo game
    // They all reside on the "promoCheck" decision point.

    public class PromoImagesTutorial : MonoBehaviour
    {
        public const string CLIENT_VERSION = "0.0.01";

        public Text lblUserID;  // Displays UserID

        private void Awake()
        {
            // Enter additional configuration here
            DDNA.Instance.ClientVersion = CLIENT_VERSION;
            DDNA.Instance.SetLoggingLevel(DeltaDNA.Logger.Level.DEBUG);
            DDNA.Instance.Settings.DebugMode = true;

            // Launch the SDK
            DDNA.Instance.StartSDK(
                "56922021622026022056772309414071",
                "https://collect2674dltcr.deltadna.net/collect/api",
                "https://engage2674dltcr.deltadna.net"
            );
        }

        // Use this for initialization
        void Start()
        {
            this.lblUserID.text = "UserID : " + DDNA.Instance.UserID; 
        }

        // Player clicks the Reset button, to wipe userID and restart as another player.
        // Helpful for checking AB Tests
        public void bttnReset_Click()
        {
            DDNA.Instance.StopSDK();
            DDNA.Instance.ClearPersistentData();
            this.Awake();
            this.Start();

        }

        // Player clicks Refresh button which forces all Promo Image children to check for new images 
        public void bttnRefresh_Click()
        {
            BroadcastMessage("PromoRefresh"); 
        }

    }
}
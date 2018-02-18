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
        public int gold; 
        public Text lblUserID;  // Displays UserID
        public Text lblGold;    // Displays Gold Amount
        private int promoImageCounter = 0; 

        private void Awake()
        {
            gold = 0; 

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
            this.lblGold.text = "Gold : " + gold;


            // Find all promo images in the scene 
            PromoImage[] promoImages = FindObjectsOfType(typeof(PromoImage)) as PromoImage[];

            foreach (PromoImage promoImage in promoImages)
            {
                // Make sure we only add the callback method to each promo image once
                if (promoImageCounter < promoImages.Length)
                {
                    // Add Promo Image Click Callback Handler
                    promoImage.PromoImageClicked += (PromoImage.PromoClickArgs obj) =>
                    {
                        PromoImageClicked(obj);
                    };

                    promoImageCounter++; 
                }
            }
        }


        // Callback handler for promo images 
        private void PromoImageClicked(PromoImage.PromoClickArgs obj)
        {
            // Use the contents of the Image Message Action type and value
            // and any Game Parameters from the parameters object to drive 
            // specific behaviours in your game.
            if (obj.ActionType == "action")
            {
                switch (obj.ActionValue)
                {
                    case "deeplink":
                        break;
                    case "reward":
                        // Check for game parameters containing reward, if present we can reward the player.
                        if (obj.parameters.ContainsKey("rewardName") && obj.parameters.ContainsKey("rewardAmount"))
                        {
                            RewardPlayer(obj.parameters["rewardName"] as string, System.Convert.ToInt32(obj.parameters["rewardAmount"]));
                        }
                        break;
                    default:
                        break;
                }
            }
            // If the Action is a URL Link, navigate to it.
            else if (obj.ActionType == "link")
            {
                // Navigage to browser location if this was URL navigation link
                if (!string.IsNullOrEmpty(obj.ActionValue))
                {
                    Debug.Log("Navigating player to : " + obj.ActionValue);
                    Application.OpenURL(obj.ActionValue);
                }
            }

            Debug.Log("Promo Image Click Handled in Callback for " + obj.ActionType + " :: " + obj.ActionValue);
            foreach(var element in obj.parameters)
            {
                Debug.Log(string.Format("Game Parameter - {0} :: {1} ", element.Key, element.Value));
            }
        }


        // Reward The player with items or currencies
        private void RewardPlayer(string rewardName , int rewardAmount)
        {
            if (rewardName == "Gold")
            {
                gold += rewardAmount;
                lblGold.text = string.Format("Gold : {0}", gold);
            }
        }


        // Player clicks the Reset button, to wipe userID and restart as another player.
        // Helpful for checking AB Tests
        public void bttnReset_Click()
        {
            DDNA.Instance.StopSDK();
            DDNA.Instance.ClearPersistentData();

            gold = 0; 
            this.Awake();          
            this.Start();
            bttnRefresh_Click();
            
        }

        // Player clicks Refresh button which forces all Promo Image children to check for new images 
        public void bttnRefresh_Click()
        {
            BroadcastMessage("PromoRefresh"); 
        }

    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using DeltaDNA;

namespace TutoralPromoImages
{
    public class PromoImagesTutorial : MonoBehaviour
    {
        public const string CLIENT_VERSION = "0.0.01";

        public Text lblUserID; 

        // Use this for initialization
        void Start()
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

            this.lblUserID.text = "UserID : " + DDNA.Instance.UserID; 
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
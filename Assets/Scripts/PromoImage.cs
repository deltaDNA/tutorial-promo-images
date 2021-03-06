﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using DeltaDNA;
namespace DeltaDNA
{
    using JSONObject = System.Collections.Generic.Dictionary<string, object>;

    public class PromoImage : MonoBehaviour
    {
        public string promoLocation;                // The decision point request parameter that will be used to target each promo image slot indepently              
        public Text offerTitle;
        public Text offerText;
        
        private DateTime offerExpiry = DateTime.MinValue;
        private int offerDurationSeconds; 

        string clickType = null;                    // The type of action that should be performed if the player clicks on the button
        string clickValue = null;                   // The value of the action that should be performed if the player clicks on the button
        JSONObject parameters = null;               // Any Game Parameters that are attached to the promo Image
        JSONObject eventParams = null;              // Meta data about the Engage in game campaign that the promo image originated from

        // Texture objects to hold the default and downloaded image 
        public  Texture2D defaultImage;
        private Texture2D downloadedImage;
         

        // The image that will display our promo
        private Image promoImage;

        // The callback method that will fire when the promo image is clicked by the player
        // Should be implemented in game code to react to Game Parameters sent with the promo image
        public event Action<PromoClickArgs> PromoImageClicked;

        
        // Object that will be returned in callback so you can see what promo action type, value and parameters the game needs to react to.
        public class PromoClickArgs : System.EventArgs
        {
            public PromoClickArgs(JSONObject parameters, string type, string value)
            {
                this.parameters = parameters;
                this.ActionType = type;
                this.ActionValue = value;
            }

            public JSONObject parameters { get; set; }
            public string ActionType { get; set; }
            public string ActionValue { get; set; }
        }


        
        // Awae method called when the Promo Image is created
        private void Awake()
        {
            clickType = null; 
            clickValue = null;
            parameters = null;
            eventParams = null;
            offerText.text = "";
            offerTitle.text = "";
            offerText.enabled = false;
            
            offerDurationSeconds = -1;
            
            // Get Promo Image Panel
            promoImage = GetComponentInChildren<Image>();
            
            // Set Default Promo Image
            if (defaultImage != null && promoImage != null)
            {
                promoImage.sprite = Sprite.Create(defaultImage, new Rect(0.0f, 0.0f, defaultImage.width, defaultImage.height), new Vector2(0.0f, 0.0f), 1.0f);
            }
        }

        
        // promoImage Start(), called when promo image is instantiated or when the PromoRefresh() method is called externally
        // Results in a call to Engage to retrieve a new promo image for the player.
        void Start()
        {
            // Check for a Promo Image from deltaDNA Engage
            // if the promo location request parameter is set                               
            if(string.IsNullOrEmpty(promoLocation))
            {
                Debug.Log("Promo Slot Location Name not Set");
                return;
            }

            // Make sure deltaDNA SDK is running before making Engage requests
            if (DDNA.Instance.isActiveAndEnabled)
            {
                Debug.Log("Check Engage for Promo Image for " + promoLocation);
                promoCheck();
            }
            else
            {
                Debug.Log("Check Engage for Promo Image for " + promoLocation + " Failed, deltaDNA SDK not running, try again later (Hit Refresh)");
            }
        }

        // Method used when the PromoImage is told to refresh itself
        // This public method can be triggered from the Promo Image's parent
        // with a BroadcastMessage("PromoRefresh") call
        public void PromoRefresh()
        {
            Debug.Log("Refreshing Promo Image : " + promoLocation);
            Start();
        }    


        // Method used to update any dynamic text displays on promo images
        // In this case it is displaying a countdown timer on one of them
        void Update()
        {
            if (offerExpiry > DateTime.Now)
            {
                UpdateExpiry();
            }
            else if(offerDurationSeconds > 0 ) 
            {
                ExpireOffer();
            }

        }


        
        private void UpdateExpiry()
        {
            offerText.text = string.Format("Expires in {0:0} seconds", offerExpiry.Subtract(DateTime.Now).TotalSeconds);
        }

        // Change the offer text and set timer paramters to ensure offer is epired.
        private void ExpireOffer()
        {
            offerText.text = "Offer Expired!";
            offerDurationSeconds = -1;
            offerExpiry = DateTime.MinValue;
        }

        // Make an Engage In-Game campaign request
        // To check for a promo image for this promo location
        void promoCheck()
        {
            var engagement = new Engagement("promoCheck")
                .AddParam("promoLocation", promoLocation);

            // Make request
            DDNA.Instance.RequestEngagement(engagement, (response) => {

                // Check Response
                if (response != null && response.StatusCode == 200)
                {
                    // Get Image info from Engage Response
                    FetchPromoImage(response);
                    parameters = FetchParams(response);
                    eventParams = FetchEventParams(response);
                }

            }, (exception) => {
                Debug.Log("Engage reported an error: " + exception.Message);
            });
        }


        // Get image info from Engage Response, download the image
        // and retrieve values for the button click action type and value
        void FetchPromoImage(Engagement response)
        {
            // Check for Image Object
            if(response.JSON.ContainsKey("image"))
            {
                JSONObject image = response.JSON["image"] as JSONObject;

                // Check for Image URL
                if (image.ContainsKey("url"))
                {
                    string imageUrl = image["url"] as string;
                    // Download Image from URL
                    StartCoroutine(LoadResourceCoroutine(imageUrl));

                    // Fetch Button Action Type and Value
                    FetchButtonValues(image);
                }                
            }
        }

        
        // Fetch GameParamters sent with the image action
        // These should be used by the game to react to Game Parameters 
        // Placed in the promo image action by the marketer
        private JSONObject FetchParams(Engagement response)
        {

            offerDurationSeconds = -1;
            offerExpiry = DateTime.MinValue;

            if (response.JSON.ContainsKey("parameters"))
            {
                JSONObject parameters = response.JSON["parameters"] as JSONObject; 
                if (parameters.ContainsKey("offerTitle"))
                {
                    offerTitle.text = parameters["offerTitle"] as string;
                }

                if (parameters.ContainsKey("offerText"))
                {
                    offerText.text = parameters["offerText"] as string;
                }

                if (parameters.ContainsKey("offerDurationSeconds"))
                {
                    offerDurationSeconds = Convert.ToInt32(parameters["offerDurationSeconds"]);
                    offerExpiry = DateTime.Now.AddSeconds(offerDurationSeconds);
                    UpdateExpiry();
                }

                return parameters;
            }
            return null; 
        }


        // Fetch Event Paramters from the Engage request
        // These will be automatically added back in to the "promoImageClicked" event
        // recorded if the player clicks on one of the promo images for reporting and analysis        
        private JSONObject FetchEventParams(Engagement response)
        {
            if (response.JSON.ContainsKey("eventParams"))
            {
                return response.JSON["eventParams"] as JSONObject;
            }
            return null;
        }


        // Fetch the Image Action button click type and value         
        private void FetchButtonValues(JSONObject image)
        {
            // Get the Action Values for the Image Message Background
            JSONObject layout = image["layout"] as JSONObject;
            JSONObject landscape = layout["landscape"] as JSONObject;
            JSONObject background = landscape["background"] as JSONObject;
            JSONObject action = background["action"] as JSONObject;

            if (action.ContainsKey("type") && action.ContainsKey("value"))
            {
                clickType = action["type"] as string;
                clickValue = action["value"] as string;
            }
            
        }


        // Promo Image Button Clicked by player
        public void OnClick()
        {
            Debug.Log("PromoImage Button Clicked : " + promoLocation);

            offerText.text = "";
            offerTitle.text = "";

            if (offerDurationSeconds > 0)
            {
                ExpireOffer();
            }

            if (!string.IsNullOrEmpty(clickType))
            {
                // Record an event in deltaDNA using eventParams Enagage meta data to indicate Campaign Action 
                GameEvent promoImageClicked = new GameEvent("promoImageClicked")
                    .AddParam("promoLocation", promoLocation)
                    .AddParam("promoImageClickType", clickType);

                if (!string.IsNullOrEmpty(clickValue))
                {
                    promoImageClicked.AddParam("promoImageClickValue", clickValue);
                }

                if (eventParams != null)
                {
                    promoImageClicked.AddParam("responseDecisionpointName", eventParams["responseDecisionpointName"] as string)
                        .AddParam("responseEngagementID", eventParams["responseEngagementID"])
                        .AddParam("responseEngagementName", eventParams["responseEngagementName"])
                        .AddParam("responseEngagementType", eventParams["responseEngagementType"])
                        .AddParam("responseTransactionID", eventParams["responseTransactionID"])
                        .AddParam("responseVariantName", eventParams["responseVariantName"])
                        .AddParam("responseMessageSequence", eventParams["responseMessageSequence"]);
                }

                DDNA.Instance.RecordEvent(promoImageClicked);

                if (parameters != null)
                {
                    // Callback method to be populated in your game code in order to react to player clicking on promo images
                    // will contain an object indicating the name and value of the promo image action along with any game parameters attched to it. 
                    PromoImage.PromoClickArgs promoClickArgs = new PromoImage.PromoClickArgs(parameters, clickType, clickValue);
                    if (this.PromoImageClicked != null)
                    {
                        this.PromoImageClicked(promoClickArgs);
                    }
                }

                // Refresh this Promo Image now that it has been clicked.
                this.PromoRefresh(); 
            }
        }


        // Download Image from URL to a Texture
        private IEnumerator LoadResourceCoroutine(string url)
        {
            // Download Image from URL in to a Texture
            #if UNITY_2017_1_OR_NEWER
            using (var www = UnityWebRequestTexture.GetTexture(url))
            {
                #if UNITY_2017_2_OR_NEWER
                    yield return www.SendWebRequest();
                #else
                yield return www.Send();
                #endif

                if (www.isNetworkError || www.isHttpError)
                {
                    Logger.LogWarning("Failed to load resource " + url + " " + www.error);
                }
                else
                {
                    downloadedImage = DownloadHandlerTexture.GetContent(www);
                }                
            }
            #else
                WWW www = new WWW(url);

                yield return www;

                if (www.error == null) {
                    www.LoadImageIntoTexture(downloadedImage);
                } else {
                    Logger.LogWarning("Failed to load resource "+url+" "+www.error);
                }                
            #endif

            // Create Sprite from Texture and apply to Promo Image
            if (downloadedImage != null && promoImage != null)
            {
                Debug.Log("Updating " + promoLocation); 
                promoImage.sprite = Sprite.Create(downloadedImage, new Rect(0.0f, 0.0f, downloadedImage.width, downloadedImage.height), new Vector2(0.0f, 0.0f), 1.0f);
                offerTitle.enabled = !string.IsNullOrEmpty(offerTitle.text);
                offerText.enabled = !string.IsNullOrEmpty(offerText.text); 
            }
        }
    }
}
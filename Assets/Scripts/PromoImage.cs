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

        // This is the decision point request parameter
        // that will be used to target each promo image slot indepently       
        public string promoLocation;

        // Texture objects to hold the default and downloaded image 
        public Texture2D defaultImage;
        private Texture2D downloadedImage;

        // The image that will display our promo
        private Image promoImage;


        // When the Promo Image is created
        private void Awake()
        {
            // Get Promo Image Panel
            promoImage = GetComponentInChildren<Image>();
            
            // Set Default Promo Image
            if (defaultImage != null && promoImage != null)
            {
                promoImage.sprite = Sprite.Create(defaultImage, new Rect(0.0f, 0.0f, defaultImage.width, defaultImage.height), new Vector2(0.0f, 0.0f), 1.0f);
            }
        }

        
        // Use this for initialization
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
                }

            }, (exception) => {
                Debug.Log("Engage reported an error: " + exception.Message);
            });

        }


        // Get image info from Engage Response
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
                }                
            }

            // TODO - Add logic to download additional game parameters from Engage
            // To handle deeplinks, rewards, promo durations, expiry ...
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
            }
        }






        // Utility method to force dealy
        IEnumerator Wait(float duration)
        {
            yield return new WaitForSeconds(duration);   //Wait
        }
    }
}
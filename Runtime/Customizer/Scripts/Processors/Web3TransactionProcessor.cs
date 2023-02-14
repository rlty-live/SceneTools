using UnityEngine;
using Judiva.Metaverse.Interactions;
using RLTY.SessionInfo;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace RLTY.Customisation
{
    [RequireComponent(typeof(TriggerZone))]
    public class Web3TransactionProcessor : Processor
    {
        [SerializeField]
        public NFTData data;

        public static Action<string, Action<Texture>> downloadImageAction;
        public Renderer image;
        public Renderer backFaceImage;

        public static NFTData DeserializeJson(string json)
        {
            return JsonConvert.DeserializeObject<NFTData>(json);
        }

        public override void Customize(KeyValueBase keyValue)
        {
            if (string.IsNullOrEmpty(keyValue.value))
            {
                gameObject.SetActive(false);
                return;
            }
            try
            {
                RLTYLog("key value = " + keyValue.value, this, LogType.Log);

                string formatedKeyValue = keyValue.value.Replace(@"\", "");

                RLTYLog("formatted key value = " + formatedKeyValue, this, LogType.Log);

                data = new NFTData();
                data = DeserializeJson(formatedKeyValue);

                RLTYLog("Deserialized =" + data.Web3Transaction + data.address + data.tokenid + data.image, this, LogType.Log);

                if (string.IsNullOrEmpty(data.image))
                {
                    if (image.material)
                        downloadImageAction?.Invoke(data.image, (x) => image.material.mainTexture = x);

                    if (backFaceImage.material)
                        downloadImageAction?.Invoke(data.image, (x) => backFaceImage.material.mainTexture = x);
                }
            }

            catch (System.Exception e)
            {
                JLogError("Invalid Web3 data on key=" + keyValue.key + " value=" + keyValue.value);
                gameObject.SetActive(false);
            }
        }

        public void Web3Transaction()
        {
            SessionInfoManagerHandlerData.Web3Transaction(JsonConvert.SerializeObject(data));
            RLTYLog("Web3: " + JsonConvert.SerializeObject(data), this, LogType.Log);
        }

        //USE THIS TO ACTIVATE TRANSACTION ON COLLISION
        //private TriggerZone _zone;
        //private bool _actionProcessed = false;
        //[SerializeField] private bool _checkUserOrientationAlignedWithForward = false;

        //protected override void Awake()
        //{
        //    base.Awake();
        //    _zone = GetComponent<TriggerZone>();
        //    _zone.onPlayerEnter += (x) => enabled = true;
        //    _zone.onPlayerExit += (x) => enabled = false;
        //    enabled = false;
        //}

        //private void OnEnable()
        //{
        //    _actionProcessed = false;
        //}

        //private void Update()
        //{
        //    //check if we trigger a donation
        //    if (!_actionProcessed && (!_checkUserOrientationAlignedWithForward || Vector3.Dot(AllPlayers.Me.Transform.forward, transform.forward) > 0.3f))
        //    {
        //        _actionProcessed = true;
        //        SessionInfoManagerHandlerData.Web3Transaction(JsonConvert.SerializeObject(_data));
        //    }
        //}
    }
}

[System.Serializable]
public class NFTData
{
    public string Web3Transaction { get; set; }
    public string address { get; set; }
    public string tokenid { get; set; }
    public string image { get; set; }
    public string chain { get; set; }
}

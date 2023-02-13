using UnityEngine;
using Judiva.Metaverse.Interactions;
using RLTY.SessionInfo;
using Newtonsoft.Json;
using System;

namespace RLTY.Customisation
{
    [RequireComponent(typeof(TriggerZone))]
    public class Web3TransactionProcessor : Processor
    {
        public static Action<string, Action<Texture>> downloadImageAction;
        public Renderer image;
        public Renderer backFaceImage;

        [System.Serializable]
        public class Data
        {
            public string Web3Transaction;
            public string address;
            public string tokenid;
            public string image;
            public string chain;
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
                _data = JsonConvert.DeserializeObject<Data>(keyValue.value);

                if (image.material)
                    downloadImageAction?.Invoke(_data.image, (x) => image.material.mainTexture = x);

                if (image.material.mainTexture && backFaceImage.material)
                    backFaceImage.material.mainTexture = image.material.mainTexture;
            }
            catch (System.Exception e)
            {
                JLogError("Invalid Web3 data on key=" + keyValue.key + " value=" + keyValue.value);
                gameObject.SetActive(false);
            }
        }

        [SerializeField] private bool _checkUserOrientationAlignedWithForward = false;
        [SerializeField] private Data _data;

        public void Web3Transaction()
        {
            SessionInfoManagerHandlerData.Web3Transaction(JsonConvert.SerializeObject(_data));
            RLTYLog(_data.ToString(), this, LogType.Log);
        }

        //USE THIS TO ACTIVATE TRANSACTION ON COLLISION
        //private TriggerZone _zone;
        //private bool _actionProcessed = false;

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

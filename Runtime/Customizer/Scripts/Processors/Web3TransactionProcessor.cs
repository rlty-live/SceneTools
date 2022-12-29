using UnityEngine;
using Judiva.Metaverse.Interactions;
using RLTY.SessionInfo;
using Newtonsoft.Json;

namespace RLTY.Customisation
{
    [RequireComponent(typeof(TriggerZone))]
    public class Web3TransactionProcessor : Processor
    {
        public class Data
        {
            public string smartContractAddress;
            public string activeChainId;
        }

        public override void Customize(Component target, KeyValueBase keyValue)
        {
            if (string.IsNullOrEmpty(keyValue.value))
            {
                gameObject.SetActive(false);
                return;
            }
                
            string[] tmp = keyValue.value.Split(",");
            if (tmp.Length != 2)
                tmp = new string[2] { "nodata", "nodata" };
            _data = JsonConvert.SerializeObject(new Data() { smartContractAddress = tmp[0].Trim(), activeChainId = tmp[1].Trim() });
        }

        [SerializeField] private string _data = "";
        private TriggerZone _zone;

        private bool _actionProcessed = false;

        protected override void Awake()
        {
            base.Awake();
            _zone = GetComponent<TriggerZone>();
            _zone.onPlayerEnter += (x) => enabled = true;
            _zone.onPlayerExit += (x) => enabled = false;
            enabled = false;
        }

        private void OnEnable()
        {
            _actionProcessed = false;
        }

        private void Update()
        {
            //check if we trigger a donation
            if (!_actionProcessed && Vector3.Dot(AllPlayers.Me.Transform.forward, transform.forward) > 0.5f)
            {
                _actionProcessed = true;
                SessionInfoManagerHandlerData.Web3Transaction(_data);
            }
        }
    }
}

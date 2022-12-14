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

        public override void Customize(Component target, RLTY.SessionInfo.KeyValueBase keyValue)
        {
            string[] tmp = keyValue.value.Split(",");
            _data = JsonConvert.SerializeObject(new Data() { smartContractAddress = tmp[0], activeChainId = tmp[1] });
        }

        [SerializeField] private string _data = "";
        private TriggerZone _zone;

        private bool _donationStarted = false;

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
            _donationStarted = false;
        }

        private void Update()
        {
            //check if we trigger a donation
            if (!_donationStarted && Vector3.Dot(AllPlayers.Me.Transform.forward, transform.forward) > 0.7f)
            {
                _donationStarted = true;
                SessionInfoManagerHandlerData.UserDonation(_data);
            }
        }
    }
}

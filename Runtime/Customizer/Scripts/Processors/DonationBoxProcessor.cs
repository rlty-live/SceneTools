using UnityEngine;
using Judiva.Metaverse.Interactions;
using RLTY.SessionInfo;

namespace RLTY.Customisation
{
    [RequireComponent(typeof(TriggerZone))]
    public class DonationBoxProcessor : Processor
    {
        public override void Customize(Component target, KeyValueBase keyValue)
        {
            if (string.IsNullOrEmpty(keyValue.value))
            {
                gameObject.SetActive(false);
                return;
            }
            _walletId = keyValue.value;
        }

        [SerializeField] private string _walletId = "1";
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
            if (!_donationStarted && Vector3.Dot(AllPlayers.Me.Transform.forward, transform.forward) > 0.5f)
            {
                _donationStarted = true;
                SessionInfoManagerHandlerData.UserDonation(_walletId);
            }
        }
    }
}

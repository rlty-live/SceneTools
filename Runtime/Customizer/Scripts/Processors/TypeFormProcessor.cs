using UnityEngine;
using Judiva.Metaverse.Interactions;
using RLTY.SessionInfo;

namespace RLTY.Customisation
{
    [RequireComponent(typeof(TriggerZone))]
    public class TypeFormProcessor : Processor
    {
        public override void Customize(Component target, KeyValueBase keyValue)
        {
            if (string.IsNullOrEmpty(keyValue.value))
            {
                gameObject.SetActive(false);
                return;
            }
            _typeformId = keyValue.value;
        }

        [SerializeField] private string _typeformId = "1";
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
                SessionInfoManagerHandlerData.OpenTypeForm(_typeformId);
            }
        }
    }
}

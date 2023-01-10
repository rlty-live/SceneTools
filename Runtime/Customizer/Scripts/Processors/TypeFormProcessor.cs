using UnityEngine;
using Judiva.Metaverse.Interactions;
using RLTY.SessionInfo;

namespace RLTY.Customisation
{
    [RequireComponent(typeof(TriggerZone))]
    public class TypeFormProcessor : Processor
    {
        public override Component FindComponent(Component target)
        {
            //we don't need a component
            return this;
        }

        public override void Customize(KeyValueBase keyValue)
        {
            if (string.IsNullOrEmpty(keyValue.value))
            {
                gameObject.SetActive(false);
                return;
            }
            _typeformId = keyValue.value;
        }

        [SerializeField] private bool _checkUserOrientationAlignedWithForward = false;
        [SerializeField] private string _typeformId = "1";

        public void OpenTypeForm()
        {
            SessionInfoManagerHandlerData.OpenTypeForm(_typeformId);
        }
    }
}

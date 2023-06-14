using UnityEngine;
using RLTY.SessionInfo;
using UnityEditor;
using Sirenix.OdinInspector;

namespace RLTY.Customisation
{
    [AddComponentMenu("RLTY/Iframe opener")]
    public class TypeFormProcessor : Processor
    {
        public override void Customize(KeyValueBase keyValue)
        {
            if (string.IsNullOrEmpty(keyValue.value))
            {
                gameObject.SetActive(false);
                return;
            }
            _typeformId = keyValue.value;
        }

        private bool _checkUserOrientationAlignedWithForward = false;
        [SerializeField] [LabelText("Url or ID")]
        private string _typeformId = "1";

        public void OpenTypeForm()
        {
            SceneInteractionHandlerData.OpenTypeForm(_typeformId);
        }
    }
}

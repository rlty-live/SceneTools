using System.Collections.Generic;
using UnityEngine;

namespace RLTY.Customisation
{
    [AddComponentMenu("RLTY/Customisable/Deactivator")]
    public class CustomisableDeactivator : MonoBehaviour
    {
        #region F/P

        #region Private

        private List<Customisable> customisables = new List<Customisable>();
        public List<string> deactivateKeys = new List<string>();

        #endregion

        #endregion

        #region Custom Methods

        #region Public

        public void SetCustomisables(List<Customisable> _customisables) => customisables = _customisables;
        public void SetDeactivateKeys(List<string> _deactivateKeys) => deactivateKeys = _deactivateKeys;
        
        public void DeactivateCustomisable()
        {
            foreach (Customisable customisable in customisables)
            {
                if (!customisable.IsDeactivable) continue;
                
                string hashCode = customisable.gameObject.GetHashCode().ToString();
                int lenghtHashCode = hashCode.Length;
                string compareKey = customisable.key.Remove(customisable.key.Length - lenghtHashCode, lenghtHashCode);
                
                if (!deactivateKeys.Contains(compareKey)) continue;

                customisable.gameObject.SetActive(false);
            }
        }

        #endregion

        #endregion
    }
}
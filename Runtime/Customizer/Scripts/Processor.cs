using UnityEngine;


namespace RLTY.Customisation
{
    [RequireComponent(typeof(Customisable)), DisallowMultipleComponent]
    public class Processor : SceneTool
    {
        protected static string commonWarning = "Please add one or remove this Customisable";
        public virtual Component FindComponent()
        {
            return this;
        }

        public virtual void Customize(KeyValueBase keyValue)
        {
            JLogBase.Log("Got " + keyValue.value, this);
        }

        protected virtual void Awake()
        {
#if UNITY_EDITOR
            if (!TryGetComponent(out Customisable custo))
                if (debug) Debug.LogWarning("You're trying to add a Processor to a Gameobject devoid of a Customisable component, it will do nothing", this);
#endif
        }

        #region Observer pattern

        #endregion
    }

}
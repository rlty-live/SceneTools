using UnityEngine;


namespace RLTY.Customisation
{
    [RequireComponent(typeof(Customisable)), DisallowMultipleComponent]
    public class Processor : RLTYMonoBehaviour
    {
        protected static string commonWarning = "Please add one or remove this Customisable";
        public virtual Component FindComponent(Component target)
        {
            return null;
        }

        public virtual void Customize(KeyValueBase keyValue)
        {

        }

        protected virtual void Awake()
        {
            if (!TryGetComponent(out Customisable custo))
                if (debug) Debug.LogWarning("You're trying to add a Processor to a Gameobject devoid of a Customisable component, it will do nothing", this);
        }

        #region Observer pattern
        public override void EventHandlerRegister()
        {
        }

        public override void EventHandlerUnRegister()
        {
        }
        #endregion
    }

}
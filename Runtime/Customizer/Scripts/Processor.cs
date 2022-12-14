using System.Collections;
using System.Collections.Generic;
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

        public virtual void Customize(Component target, RLTY.SessionInfo.KeyValueBase keyValue)
        {

        }

        #region Processor declaration

        public static Dictionary<CustomisableType, System.Type> AllTypes
        {
            get
            {
                if (_allProcessors == null)
                {
                    _allProcessors = new Dictionary<CustomisableType, System.Type>();
                    _allProcessors[CustomisableType.Texture] = typeof(MaterialProcessor);
                    _allProcessors[CustomisableType.Color] = typeof(MaterialProcessor);
                    _allProcessors[CustomisableType.Sprite] = typeof(SpriteProcessor);
                    _allProcessors[CustomisableType.Video] = typeof(VideoStreamProcessor);
                    _allProcessors[CustomisableType.Text] = typeof(TextProcessor);
                    _allProcessors[CustomisableType.ExternalPage] = typeof(ExternalPageProcessor);
                    _allProcessors[CustomisableType.Audio] = typeof(AudioProcessor);
                    _allProcessors[CustomisableType.DonationBox] = typeof(DonationBoxProcessor);
                }
                return _allProcessors;
            }
        }
        private static Dictionary<CustomisableType, System.Type> _allProcessors;

        #endregion

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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RLTY.Testing
{
    [RequireComponent(typeof(Camera)), AddComponentMenu("RLTY/Testing/EditorOnlyMainCamera")]
    public class MainCameraOnlyEditor : RLTYMonoBehaviour
    {
        [InfoBox("Use this script to makes tests on a MainCamera, it will be destroyed at runtime")]
        public override void Start()
        {
            base.Start();

#if !UNITY_EDITOR
                Destroy(this.gameObject);
                if (debug) Debug.Log("Destroying this camera", this.gameObject);
#else
            DestroyImmediate(this.gameObject);
            if (debug)
                Debug.Log("Destroying this camera", this.gameObject);
#endif
        }

        #region Observer Pattern
        public override void EventHandlerRegister()
        { }
        public override void EventHandlerUnRegister()
        { }
        #endregion
    }
}

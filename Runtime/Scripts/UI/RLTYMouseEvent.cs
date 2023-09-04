using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

namespace RLTY.UI
{
    [RequireComponent(typeof(Collider))]
    public class RLTYMouseEvent : JMonoBehaviour
    {
        /// <summary>
        /// Maximum distance from the viewpoint at which events are triggered
        /// </summary>
        public float mouseDetectDistance = 1000;
        /// <summary>
        /// UnityEvents for Inspector
        /// </summary>
        public UnityEvent OnPointerEnter, OnPointerExit, OnPointerDown, OnPointerUp, OnClick;

        public void NotifyOnPointerEnter()
        {
            //Debug.Log($"OnPointerEnter {name}");
            OnPointerEnter?.Invoke();
        }

        public void NotifyOnPointerExit()
        {
            //Debug.Log($"OnPointerExit {name}");
            OnPointerExit?.Invoke();
        }

        public void NotifyOnPointerDown()
        {
            //Debug.Log($"OnPointerDown {name}");
            OnPointerDown?.Invoke();
        }

        public void NotifyOnPointerUp()
        {
            //Debug.Log($"OnPointerUp {name}");
            OnPointerUp?.Invoke();
        }

        public void NotifyOnClick()
        {
            //Debug.Log($"OnClick {name}");
            OnClick?.Invoke();
        }
    }
}

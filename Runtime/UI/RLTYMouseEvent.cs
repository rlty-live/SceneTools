using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

namespace RLTY.UI
{
    public class RLTYMouseEvent : JMonoBehaviour
    {
        /// <summary>
        /// events to bind from code
        /// </summary>
        public event Action onPointerDown, onPointerUp, onClick;

        /// <summary>
        /// UnityEvents for Inspector
        /// </summary>
        public UnityEvent OnPointerDown, OnPointerUp, OnClick;

        public void NotifyOnPointerDown() { OnPointerDown?.Invoke(); onPointerDown?.Invoke(); }
        public void NotifyOnPointerUp() { OnPointerUp?.Invoke(); onPointerUp?.Invoke(); }
        public void NotifyOnClick() { OnClick?.Invoke(); onClick?.Invoke(); }
    }
}

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using Sirenix.OdinInspector;

namespace RLTY.UI
{
    [RequireComponent(typeof(Collider))]
    public class RLTYMouseEvent : JMonoBehaviour
    {
        [Title("RLTYMouseEvent")]
        /// <summary>
        /// Maximum distance from the viewpoint at which events are triggered
        /// </summary>
        public float mouseDetectDistance = 1000;
        [ShowIf("@transform.GetComponent<RectTransform>() != null")]
        public bool AutoResizeColliderOnStart;

        /// <summary>
        /// UnityEvents for Inspector
        /// </summary>
        public UnityEvent OnPointerEnter, OnPointerExit, OnPointerDown, OnPointerUp, OnClick = new UnityEvent();

        private Collider _collider;
        public Collider Collider => _collider != null ? _collider : _collider = GetComponent<Collider>();

        public void Start()
        {
            if (AutoResizeColliderOnStart)
                ResizeCollider();
        }

        [Button]
        public void ResizeCollider()
        {
            if (transform.TryGetComponent(out RectTransform rectTransform))
            {
                if (Collider is BoxCollider boxCollider)
                {
                    boxCollider.size = new Vector3(rectTransform.rect.width, rectTransform.rect.height, 0.01f);
                    boxCollider.center = rectTransform.rect.center;
                }
            }
        }

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
            //JLog($"OnPointerDown {name}");
            OnPointerDown?.Invoke();
        }

        public void NotifyOnPointerUp()
        {
            //JLog($"OnPointerUp {name}");
            OnPointerUp?.Invoke();
        }

        public void NotifyOnClick()
        {
            JLog($"OnClick {name}");
            OnClick?.Invoke();
        }
    }
}

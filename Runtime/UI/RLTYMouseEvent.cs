using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace RLTY.UI
{
    public class RLTYMouseEvent : JMonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public UnityEvent OnPointerEnter, OnPointerExit, OnPointerDown, OnPointerUp, OnClick;

        /*
        public void NotifyOnPointerEnter() => OnPointerEnter?.Invoke();
        public void NotifyOnPointerExit() => OnPointerExit?.Invoke();*/
        public void NotifyOnPointerDown() => OnPointerDown?.Invoke();
        public void NotifyOnPointerUp() => OnPointerUp?.Invoke();
        public void NotifyOnClick() => OnClick?.Invoke();

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            OnPointerEnter?.Invoke();
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            OnPointerExit?.Invoke();
        }
    }
}

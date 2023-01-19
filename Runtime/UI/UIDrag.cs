using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RLTY.UI
{
    /// <summary>
    /// Receive mouse drag events on the screen, and forward them to anyone listening (useful for camera mouse control)
    /// </summary>
    public class UIDrag : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        public event Action<Vector2> OnUserDrag;
        public event Action<Ray> OnUserClick;
        public LayerMask raycastMask;
        private bool _drag = false;
        private Camera _view;
        private float _farClip;
        private float _fieldOfView;
        private int _width, _height;
        private Transform _canvas;

        private RLTYMouseEvent _pointedObject;

        public void OnDrag(PointerEventData eventData)
        {
            _drag = true;
            OnUserDrag?.Invoke(new Vector2(eventData.delta.x / Screen.width, eventData.delta.y / Screen.height));
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _drag = false;
            //raycast
            if (Physics.Raycast(Camera.main.ScreenPointToRay(eventData.pressPosition), out RaycastHit hit, 10000f, raycastMask) && hit.collider.TryGetComponent<RLTYMouseEvent>(out _pointedObject))
            {
                _pointedObject.NotifyOnPointerDown();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_pointedObject) 
                _pointedObject.NotifyOnPointerUp();
            if (!_drag)
            {
                if (_pointedObject)
                    _pointedObject.NotifyOnClick();
                OnUserClick?.Invoke(Camera.main.ScreenPointToRay(eventData.pressPosition));
            }
            _pointedObject = null;
        }

        void Awake()
        {
            _canvas = GetComponentInParent<Canvas>().transform;
        }

        void LateUpdate()
        {
            if (_view == null)
                _view = GetComponentInParent<Camera>();
            if (_farClip!=_view.farClipPlane || _fieldOfView!=_view.fieldOfView || _width!=Screen.width || _height!=Screen.height)
            {
                _width = Screen.width;
                _height = Screen.height;
                _farClip = _view.farClipPlane;
                _fieldOfView = _view.fieldOfView;
                _canvas.transform.localPosition = new Vector3(0, 0, _farClip * 0.99f);
                Vector3 pos = _view.transform.InverseTransformPoint(_view.ViewportToWorldPoint(new Vector3(1,1, _farClip * 0.99f)));
                _canvas.transform.localScale = new Vector3(2*pos.x, 2*pos.y, 1);
            }
        }
    }
}

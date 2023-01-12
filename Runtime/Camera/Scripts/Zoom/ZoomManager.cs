using System.Collections.Generic;
using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Animations;

namespace _0_MetaEvent._0_Features.Camera.Scripts.Zoom
{
    public class ZoomManager : MonoBehaviour
    {
        private static ZoomManager instance = default;
        public static ZoomManager Instance => instance;

        [SerializeField] private List<ZoomController> _controllers = new List<ZoomController>();

        [SerializeField, ReadOnly] private UnityEngine.Camera _mainCamera = null;
        [SerializeField, ReadOnly] private CinemachineBrain _mainCinemachineBrain = null;
        [SerializeField, ReadOnly] private CinemachineVirtualCamera _mainVirtualCamera = null;

        private CinemachineVirtualCamera _current = null;
        private ParentConstraint _parentConstraint = null;

        private float _time = 0;
        private float _duration = 1.5f;

        private bool transitionStart = false;

        public bool IsZoomed = false;

        #region Unity Methods

        private void Awake()
        {
            if (instance && this != instance)
            {
                Debug.Log("Destroy");
                Destroy(this);
                return;
            }

            instance = this;
        }

        private void Start()
        {
            foreach (ZoomController controller in FindObjectsOfType<ZoomController>())
            {
                if (!_controllers.Contains(controller))
                    _controllers.Add(controller);
            }

            if (GetMainCamera())
            {
                TryGetOrCreateMainBrain(_mainCamera);
                CreateMainVirtualCamera();
            }

            ZoomHandlerData.OnZoomStart += () =>
            {
                UIManagerHandlerData.EnablePlayerInput(false);
            };
            ZoomHandlerData.OnZoomEnd += () => { IsZoomed = true; };

            ZoomHandlerData.OnUnzoomEnd += () =>
            {
                IsZoomed = false;
                UIManagerHandlerData.EnablePlayerInput(true);
            };
        }

        private void Update()
        {
            if (transitionStart)
            {
                _time += Time.deltaTime;
                if (_time > _duration)
                {
                    transitionStart = false;
                    _time = 0;
                    if (!IsZoomed) ZoomHandlerData.ZoomEnd();
                    else ZoomHandlerData.UnzoomEnd();
                }
            }
        }

        #endregion

        #region Initialize Methods

        /// <summary>
        /// Get the main camera of the Game and stock in _mainCamera
        /// </summary>
        /// <returns>Return true if successful</returns>
        bool GetMainCamera()
        {
            UnityEngine.Camera main = UnityEngine.Camera.main;
            if (!main) return false;
            _mainCamera = main;
            return true;
        }

        /// <summary>
        /// Get Brain of the Cinemachine main camera or Create if no Brain found
        /// </summary>
        /// <param name="mainCamera"></param>
        void TryGetOrCreateMainBrain(UnityEngine.Camera mainCamera)
        {
            if (!mainCamera.TryGetComponent(out CinemachineBrain brain))
            {
                brain = mainCamera.gameObject.AddComponent<CinemachineBrain>();
            }

            _mainCinemachineBrain = brain;
            // _mainCinemachineBrain.enabled = false;
        }

        void CreateMainVirtualCamera()
        {
            GameObject virtualCamera =
                Instantiate(new GameObject());
            virtualCamera.name = "ZoomVirtualCamera";
            _mainVirtualCamera = virtualCamera.AddComponent<CinemachineVirtualCamera>();
            _mainVirtualCamera.Priority = 11;
            _mainVirtualCamera.gameObject.SetActive(false);
        }
        
        #endregion


        public void ZoomOnThis(CinemachineVirtualCamera virtualCamera, float duration)
        {
            if (_current) return;
            _current = virtualCamera;
            ZoomHandlerData.ZoomStart();

            transitionStart = true;
            _duration = duration;
            _mainCinemachineBrain.m_DefaultBlend.m_Time = duration;

            virtualCamera.enabled = true;
            virtualCamera.Priority = 20;
            virtualCamera.m_Lens.FieldOfView = _mainCamera.fieldOfView;
        }

        public void UnZoom(CinemachineVirtualCamera virtualCamera, float duration)
        {
            if (_current != virtualCamera) return;


            ZoomHandlerData.UnzoomStart();

            transitionStart = true;
            _duration = duration;
            _mainCinemachineBrain.m_DefaultBlend.m_Time = duration;

            virtualCamera.gameObject.SetActive(false);
            virtualCamera.enabled = false;

            _current = null;
        }

        public void ZoomOnThis2(Transform target, CinemachineVirtualCamera vcam)
        {
            Debug.LogWarning($"VCam pos {vcam.transform.position}");
            
            Setup(target, 60, vcam);

            Debug.LogWarning($"VCam position after Setup {vcam.transform.position}");
            
            vcam.gameObject.SetActive(true);
        }

        public void ZoomOnThis3(Transform target)
        {
            Setup(target, 60, _mainVirtualCamera);

            _mainVirtualCamera.gameObject.SetActive(true);
        }

        public void UnZoom2()
        {
            _mainVirtualCamera.gameObject.SetActive(false);
        }

        // Adjust the camera's distance to the object so that the object occupies at most a specified percentage of the screen
        public static void Setup(Transform target, float fov, CinemachineVirtualCamera vcam, float screenHeightPercentage = 1)
        {
            // Check if the target, virtual camera, and screen height percentage are defined
            if (target != null && vcam != null && screenHeightPercentage > 0 && screenHeightPercentage <= 1)
            {
                // Get the height of the object
                float objectHeight = 0;
                Renderer renderer = target.GetComponent<Renderer>();
                if (renderer != null)
                {
                    objectHeight = renderer.bounds.size.y;
                    objectHeight *= target.lossyScale.y;
                }
                else
                {
                    // Get the height of the object from its RectTransform
                    RectTransform rectTransform = target.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        objectHeight = rectTransform.rect.height;
                        objectHeight *= rectTransform.lossyScale.y;
                    }
                }

                // Check if the object's height is valid
                if (objectHeight > 0)
                {
                    // Calculate the height of the camera frustum at the distance where the object occupies the specified percentage of the screen
                    float cameraHeight = objectHeight / screenHeightPercentage;
                    float distance = cameraHeight / (2 * Mathf.Tan(fov * Mathf.Deg2Rad / 2));

                    // Set the camera's distance to the object to the calculated distance
                    vcam.LookAt = target;
                    vcam.transform.position = target.position - vcam.transform.forward * distance;
                }
                
            }
        }
    }
}
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Judiva.Metaverse.Interactions;
using RLTY.SessionInfo;

namespace RLTY.Customisation
{
    [AddComponentMenu("RLTY/Customisable/Processors/VideoStream"), HideMonoScript]
    public class VideoStreamProcessor : Processor
    {
        #region Global Variables
        [Title("Playback conditions")]
        [SerializeField, Tooltip("The WatchArea collider can be of any type, but has to be on this Gameobject")]
        private bool useWatchArea = false;
        [SerializeField]
        [ShowIf("useWatchArea")]
        private TriggerZone triggerZone;

        //[SerializeField, ReadOnly, Space(10), Tooltip("True if all corners of Screen are visible")]
        //private bool visibleByPlayer;
        //[SerializeField, ShowIf("showUtilities", true)]
        //[Tooltip("Frequency in seconds in which we will check for screen visibility")]
        //private float visibilityCheckRate = 0.3f;

        [Title("NexPlayer"), Space(10)]
        [InfoBox("For WebGL Autoplay to work volume must be set to 0, see NexPlayer documentation", "wrongNexPlayerSetup")]
        public bool useTestUrl;
        [SerializeField, ReadOnly, ShowIf("showUtilities", true)]
        private IVideoPlayer _iVideoPlayer;
        private MonoBehaviour _iVideoPlayerMonoBehaviour;

        [ShowIf("useTestUrl", true)]
        public string testStreamUrl;
        [ShowIf("useTestUrl", true)]
        public bool testLiveStream;
        [ReadOnly, ShowIf("showUtilities", true)]
        public string customizedUrl;
        [ReadOnly, ShowIf("showUtilities", true)]
        public bool customizedLiveStream;

        private bool wrongNexPlayerSetup;
        private string nexPlayerSetupWarningMessage = "For WebGL Autoplay to work volume must be set to 0, see NexPlayer documentation";
        [SerializeField, ReadOnly, ShowIf("showUtilities", true)]
        private int streamWidth = 1;
        [SerializeField, ReadOnly, ShowIf("showUtilities", true)]
        private int streamHeight = 1;



        [Title("Rendering")]
        [SerializeField]
        [Tooltip("If false a new renderTexture will be created at runtime")]
        private bool chooseRenderTexture;
        [ShowIf("chooseRenderTexture", true)]
        public RenderTexture renderTexture;
        private int bitDepth = 16;
        private int[] acceptedBitDepth = { 0, 16, 24, 32 };

        private static Shader staticDefaultShader;
        [SerializeField, ShowIf("showUtilities", true)]
        private Shader defaultShader;
        [SerializeField, HideIf("screenMaterial", null)]
        [Tooltip("If empty URP/Unlit will be selected")]
        private Shader shader;
        [SerializeField]
        [Tooltip("If empty a new Material using the will be created with the shader selected below at runtime")]
        private Material screenMaterial;

        [SerializeField, ShowIf("showUtilities", true)]
        private Image screenRenderer = null;
        [SerializeField]
        [Tooltip("Use to test for screen visibility, every corners of this RectTransform has to be visible for VisibleByPlayer to be true")]
        private RectTransform screen;

        [Title("Animation")]
        [SerializeField]
        private bool animateScreen = false;
        [SerializeField, Tooltip("Time the screen will take to open and close, In seconds")]
        private float transitionTime = 10f;
        private Coroutine lerpScreenCoroutine;
        private Vector3 openScale;


        [Title("Audio")]
        [SerializeField]
        private float soundFadeDuration = 2;
        [SerializeField]
        private float volume = 0.75f;
        [SerializeField]
        private float volumeInVisio = 0.1f;

#if UNITY_EDITOR
        private Texture previousTexture = null;
#endif

        [ShowIf("showUtilities", true), ReadOnly]
        private string setupWarningMessage = "";

        private bool firstUserActivation = true;
        #endregion

        #region Properties
        public bool isValidNewDimensions()
        {
            if (_iVideoPlayer.GetVideoWidth() > 0 && _iVideoPlayer.GetVideoWidth() > 0)
            {
                if (streamWidth != _iVideoPlayer.GetVideoWidth() || streamHeight != _iVideoPlayer.GetVideoHeight())
                {
                    streamWidth = _iVideoPlayer.GetVideoWidth();
                    streamHeight = _iVideoPlayer.GetVideoHeight();

                    return true;
                }

                else
                    return false;
            }

            else
                return false;
        }

        // WIP visibleCorners stay equal to 4 when screen is in the back of the camera
        //private bool isScreenFullyVisible()
        //{
        //    Vector3[] objectCorners = new Vector3[4];
        //    int visibleCorners = 0;
        //    Rect screenBounds = new Rect(0f, 0f, Screen.width, Screen.height);

        //    if (screen)
        //    {
        //        screen.GetWorldCorners(objectCorners);

        //        foreach (Vector3 corner in objectCorners)
        //        {
        //            if (screenBounds.Contains(Camera.main.WorldToScreenPoint(corner)))
        //                visibleCorners++;
        //        }

        //        if (visibleCorners == 4)
        //        {
        //            visibleByPlayer = true;
        //            return true;
        //        }


        //        else
        //        {
        //            visibleByPlayer = false;
        //            return false;
        //        }
        //    }


        //    return false;
        //}

        [ShowInInspector, ShowIf("showUtilities"), Tooltip("Bit depth must be either 0, 16, 24 or 32")]
        public int BitDepth
        {
            get
            {
                return bitDepth;
            }

            set
            {
                for (int i = 0; i < acceptedBitDepth.Length; i++)
                {
                    if (acceptedBitDepth[i] == value)
                    {
                        bitDepth = value;
                        break;
                    }
                }
            }
        }
        #endregion

        public override void Customize(Component target, KeyValueBase keyValue)
        {
            customizedUrl = keyValue.value;

            if (keyValue.key.Contains("LiveStream"))
            {
                customizedLiveStream = true;
            }
            else customizedLiveStream = false;
        }

        #region EditorOnly Logic
#if UNITY_EDITOR
        private void OnValidate() => CheckSetup();

        public void OnDrawGizmos()
        {
            //if (visibleByPlayer && screenRenderer)
            //    Gizmos.DrawIcon(screenRenderer.transform.position, "Eye");
        }

        public void OnApplicationQuit() => screenMaterial.mainTexture = previousTexture;
#endif
        #endregion

        #region Common Logic
        public override void CheckSetup()
        {
            bool test = false;

            //Try to find a watch area in children or desactivate triggering
            if (useWatchArea)
                if (!triggerZone)
                {
                    if (!GetComponentInChildren<TriggerZone>())
                    {
                        if (debug)
                            Debug.LogWarning("No triggerZone Assigned, useWatchArea will be set to false at runtime", this);
                        if (Application.IsPlaying(this))
                            useWatchArea = false;

                        test = false;
                    }

                    else
                    {
                        triggerZone = GetComponentInChildren<TriggerZone>();
                        test = true;
                    }
                }

            //Rendering Setup, check for at least a renderer
            if (!screenRenderer)
            {
                setupWarningMessage = "No screenRenderer assigned, video will only be visible if a " + screenMaterial + " has been set and is used in the scene";
                if (debug) Debug.Log(setupWarningMessage, this);
            }

            //Search for a declared default shader (or use URP Unlit) and propagate this one to all videoStream present
            if (defaultShader)
                staticDefaultShader = defaultShader;
            else
            {
                staticDefaultShader = Shader.Find("Universal Render Pipeline/Lit");
            }

            //Search for an assigned shader or use the default one
            if (!shader)
            {
                shader = staticDefaultShader;

                setupWarningMessage = "No shader selected, Universal Render Pipeline / Unlit will be used";
                if (debug) Debug.LogWarning(setupWarningMessage, this);
            }

            correctSetup = test;
        }

        /// <summary>
        /// Will instantiate NexPlayer and start playback automatically, if trigger zone aren't in use for this VideoStream
        /// </summary>
        public void StartPlayback()
        {
                InitializeRenderTexture();
                InstantiateNexPlayer();
                FadeSound(volume);

                if (animateScreen)
                    lerpScreenCoroutine = StartCoroutine(ToggleOpenScreen(true));
            }

        private void InitializeRenderTexture()
        {
            if (!acceptedBitDepth.Contains(bitDepth))
            {
                bitDepth = 16;
                if (debug)
                    Debug.Log("Bit depth for " + renderTexture + " is not properly setup, switching to 16 bits");
            }

            if (debug)
                Debug.Log("Intialized render texture for " + _iVideoPlayer, this);

            if (!renderTexture)
            {
                if (chooseRenderTexture)
                {
                    chooseRenderTexture = false;

                    if (debug)
                        Debug.Log(this + " is set to renderTexture and custom Texture, but Texture has not been assigned, will fallaback to creating a new one", this);
                }


                renderTexture = new RenderTexture(1920, 1080, BitDepth);
                renderTexture.Create();

                //To refine or not use entirely
                //renderTexture.useMipMap = true;
                //renderTexture.autoGenerateMips = true;

                renderTexture.wrapMode = TextureWrapMode.Repeat;
            }

            if (!screenMaterial)
            {
                screenMaterial = new Material(shader);
                screenRenderer.material = screenMaterial;
            }

            screenMaterial.mainTexture = renderTexture;
            //Flip the texture on Y axis
            screenMaterial.mainTextureScale = new Vector2(1, -1);
        }

        // Scaling of the Texture is not working for now, it should be compensated by the UV mapping
        // For now it will take the actual UV mapping
        //private void RenewRenderTexture()
        //{
        //    if (debug) Debug.Log("Started creation of a renderTexture matching " + nexPlayer.URL + ", dimensions: " + streamWidth + "," + streamHeight);

        //    //Clear previous renderTexture from memory 
        //    //(is not cleaned up by the Garbage collector the same way as the rest, see documentation)
        //    if (renderTexture)
        //    {
        //        renderTexture.Release();
        //        Destroy(renderTexture);
        //    }

        //    renderTexture = new RenderTexture(streamWidth, streamHeight, bitDepth);

        //    string[] strings = nexPlayer.URL.Split("/");
        //    string name = strings[strings.Length - 1];
        //    renderTexture.name = name;
        //    screenMaterial.name = name;

        //    screenMaterial.SetTexture("_MainTex", renderTexture);
        //    nexPlayer.SetTargetTexture(renderTexture);
        //    if (nexPlayerRenderController)
        //        nexPlayerRenderController.renderTexture = renderTexture;

        //    if (debug)
        //        Debug.Log(nexPlayer + "texture is set to " + nexPlayer.GetTargetRenderTexture());

        //    screenRenderer.enabled = false;
        //    screenRenderer.enabled = true;
        //}

        private void InstantiateNexPlayer()
        {
            if (debug)
            {
                if (useTestUrl)
                    Debug.Log("Instantiating NexPlayerSimpleRLTY for stream: " + testStreamUrl);
                else
                    Debug.Log("Instantiating NexPlayerSimpleRLTY for stream: " + customizedUrl);
            }

            //REFACTOR FRANCOIS
            if (_iVideoPlayerMonoBehaviour)
                Destroy(_iVideoPlayerMonoBehaviour);


            //Never Worked, just flip the material tiling instead
            //nexPlayerRenderController.enableHorizontalFlip = true;
            //nexPlayerRenderController.enableVerticalFlip = true;

            if (IVideoPlayer.Implementation != null && IVideoPlayer.Implementation is IVideoPlayer)
            {
                _iVideoPlayerMonoBehaviour = gameObject.AddComponent(IVideoPlayer.Implementation.GetType()) as MonoBehaviour;
                _iVideoPlayerMonoBehaviour.enabled = true;
                _iVideoPlayer = _iVideoPlayerMonoBehaviour as IVideoPlayer;
            }
            else
            {
                Debug.LogError("No implementation defined for IVideoPlayer");
            }
        }

        private void DestroyNexPlayer()
        {
            //StartCoroutine(FadeSound(false));
            Destroy(_iVideoPlayerMonoBehaviour);
        }
        #endregion

        #region Unity Callbacks
        public override void Start()
        {
            base.Start();
            CheckSetup();
            InitializeRenderTexture();

            if (animateScreen && screen)
            {
                openScale = screen.localScale;
                screen.localScale = new Vector3(screen.localScale.x, 0, screen.localScale.z);
            }

            //InvokeRepeating("isScreenFullyVisible", 0, 0.3f);

            VoiceManagerHandlerData.OnJoinVisio += FadeSoundOnEnterVisio;
            VoiceManagerHandlerData.OnLeaveVisio += FadeSoundOnExitVisio;
        }

        private void Update()
        {
            if (!useWatchArea & firstUserActivation)
            {
                if (UnityEngine.Input.anyKeyDown |
                    UnityEngine.Input.GetMouseButtonDown(0) |
                    UnityEngine.Input.GetMouseButtonDown(1) |
                    UnityEngine.Input.GetMouseButtonDown(2))
                {
                    StartPlayback();
                    firstUserActivation = false;
                }
            }
        }


        private void OnDestroy()
        {
            VoiceManagerHandlerData.OnJoinVisio -= FadeSoundOnEnterVisio;
            VoiceManagerHandlerData.OnLeaveVisio -= FadeSoundOnExitVisio;
        }

        IEnumerator ToggleOpenScreen(bool open)
        {
            if (debug)
            {
                if (open)
                    Debug.Log("Opening screen " + screen, this);
                else
                    Debug.Log("Closing screen " + screen, this);
            }

            float elapsedTime = 0;

            Vector3 initialScale;
            Vector3 targetScale;

            if (screen)
            {
                initialScale = screen.transform.localScale;

                if (open) targetScale = openScale;
                else targetScale = new Vector3(openScale.x, 0, openScale.z);

                while (elapsedTime < transitionTime)
                {
                    screen.localScale = Vector3.Lerp(initialScale, targetScale, (elapsedTime / transitionTime));
                    elapsedTime += Time.deltaTime;

                    yield return null;
                }

                screen.localScale = targetScale;
                yield return null;
            }
        }

        Coroutine _soundFadeCoroutine;

        void FadeSound(float targetVolume)
        {
            if (_soundFadeCoroutine != null)
                StopCoroutine(_soundFadeCoroutine);
            _soundFadeCoroutine = StartCoroutine(FadeSoundCoroutine(targetVolume));
        }
        IEnumerator FadeSoundCoroutine(float targetVolume)
        {
            float startVolume;
            float elapsedTime = 0;

            if (_iVideoPlayerMonoBehaviour != null)
            {
                startVolume = _iVideoPlayer.Volume;

                while (elapsedTime < soundFadeDuration)
                {
                    _iVideoPlayer.Volume = Mathf.Lerp(startVolume, targetVolume, soundFadeDuration / elapsedTime);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }

            else
            {
                if (debug)
                    Debug.Log("No NexPLayer found", this);
            }

            yield return null;
        }

        void FadeSoundOnEnterVisio(string visioId)
        {
            FadeSound(volumeInVisio);
        }

        void FadeSoundOnExitVisio(string visioId)
        {
            FadeSound(volume);
        }

        #region Observer Pattern
        public override void EventHandlerRegister()
        {          
        }

        public override void EventHandlerUnRegister()
        {       
        }
        #endregion
        #endregion
    }
}


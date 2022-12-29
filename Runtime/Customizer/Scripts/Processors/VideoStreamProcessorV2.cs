using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Judiva.Metaverse.Interactions;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RLTY.Customisation
{
#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [HideMonoScript]
#endif
    public class VideoStreamProcessorV2 : Processor
    {
        #region Global Variables
        [Title("Global Parameters")]
        public string videoURL;
        [SerializeField]
        public bool isLiveStream;
        [Tooltip("Texture to apply when nothing is playing, can be left null ?")]
        [SerializeField]
        public Texture2D defaultTexture;
        [HideInInspector]
        [Tooltip("If true player won't be destroyed when leaving viewing area, but deactivated instead.")]
        public bool keepLoaded;

        [Title("External Player")]
        private IVideoPlayer _iVideoPlayer;
        [SerializeField, ReadOnly]
        private MonoBehaviour _iVideoPlayerMonoBehaviour;

        [Title("Rendering")]
        [Tooltip("Apply either to shared material or instanced material")]
        public bool applyToMaterial;
        [ShowIf("applyToMaterial")]
        public Material mat;
        [HideIf("applyToMaterial", true)]
        public Renderer[] targetRenderers;
        [Tooltip("If several material on target renderer, provide index, if not first will be used")]
        [HideIf("applyToMaterial", true)]
        public int materialIndex;
        public bool applyToUI;
        [ShowIf("applyToUI")]
        public MaskableGraphic[] targets;

        [Title("Audio")]
        //public bool spatializedAudio;
        ////public AudioOutput audioOutput;
        //public Transform[] speakers;
        [SerializeField]
        private float soundFadeDuration = 2;
        [SerializeField]
        private float volume = 0.75f;
        [SerializeField]
        private float volumeInVisio = 0.1f;
        Coroutine _soundFadeCoroutine;

        [Title("Playback")]
        [SerializeField]
        private bool useTriggerZone;

        #endregion

        public void Customize(string url) => videoURL = url;

        #region Triggering
        public void StartPlayback()
        {
            //REFACTOR FRANCOIS
            if (_iVideoPlayerMonoBehaviour)
                Destroy(_iVideoPlayerMonoBehaviour);

            if (IVideoPlayer.Implementation != null && IVideoPlayer.Implementation is IVideoPlayer)
            {
                _iVideoPlayerMonoBehaviour = gameObject.AddComponent(IVideoPlayer.Implementation.GetType()) as MonoBehaviour;
                _iVideoPlayerMonoBehaviour.enabled = true;
                _iVideoPlayer = _iVideoPlayerMonoBehaviour as IVideoPlayer;
            }
            else
                Debug.LogError("No implementation defined for IVideoPlayer");
        }

        public void DestroyPlayer()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                Destroy(_iVideoPlayerMonoBehaviour);
            else
                DestroyImmediate(_iVideoPlayerMonoBehaviour);
#else
                Destroy(_iVideoPlayerMonoBehaviour);
#endif
        }
        #endregion

        #region Audio
        void FadeSoundOnEnterVisio(string visioId) => FadeSound(volumeInVisio);
        void FadeSoundOnExitVisio(string visioId) => FadeSound(volume);
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
                if (debug)
                Debug.Log("No NexPLayer found", this);

            yield return null;
        }
        #endregion

        #region Callbacks
        public override void Start()
        {
            base.Start();
            CheckSetup();

            VoiceManagerHandlerData.OnJoinVisio += FadeSoundOnEnterVisio;
            VoiceManagerHandlerData.OnLeaveVisio += FadeSoundOnExitVisio;
        }

        public void Update()
        {
            if (_iVideoPlayerMonoBehaviour)
                if (Input.anyKeyDown
                    | Input.GetMouseButtonDown(0)
                    | Input.GetMouseButtonDown(1)
                    | Input.GetMouseButtonDown(2))
                    _iVideoPlayer.Volume = volume;
        }

        public override void OnDestroy()
        {
            VoiceManagerHandlerData.OnJoinVisio -= FadeSoundOnEnterVisio;
            VoiceManagerHandlerData.OnLeaveVisio -= FadeSoundOnExitVisio;
        }
        #endregion
    }

    //#if UNITY_EDITOR
    //[CustomEditor(typeof(VideoStreamProcessorV2))]
    //public class VideoStreamProcessorEditorTest : Editor
    //{
    //    public override void OnInspectorGUI()
    //    {
    //        VideoStreamProcessorV2 videoStreamProcessorV2 = (VideoStreamProcessorV2)target;

    //        // will enable the default inpector UI 
    //        base.OnInspectorGUI();

    //        // implement your UI code here
    //        if (GUILayout.Button("Instantiate"))
    //        {
    //            //videoStreamProcessorV2.InstantiatePlayer();
    //            videoStreamProcessorV2.StartPlayback();
    //        }

    //        //if (GUILayout.Button("Disable")) videoStreamProcessorV2.mediaPlayer.enabled = false;

    //        if (GUILayout.Button("Destroy")) videoStreamProcessorV2.DestroyPlayer();
    //    }
    //}
    //#endif
}

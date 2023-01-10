using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Collections;
using Judiva.Metaverse.Interactions;

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

        [Title("Rendering")]
        [Tooltip("Apply either to shared material or instanced material")]
        public bool applyToMeshes;
        [ShowIf("applyToMeshes")]
        public Renderer[] targetRenderers;

        public bool applyToUI;
        [ShowIf("applyToUI")]
        public MaskableGraphic[] targetsUIs;

        //[Title("Audio")]
        //public bool spatializedAudio;
        ////public AudioOutput audioOutput;
        //public Transform[] speakers;

        [Title("Playback")]
        public bool useTriggerZone;
        [ShowIf("useTriggerZone")]
        public TriggerZone triggerZone;
        #endregion

        public override void Customize(KeyValueBase kvo)
        {
            videoURL = kvo.value;
            Debug.Log("Got " + kvo.value + " from sceneDescription", this);
        }
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

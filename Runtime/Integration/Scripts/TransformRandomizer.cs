using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RLTY.Tools
{
#if UNITY_EDITOR
    [CanEditMultipleObjects,AddComponentMenu("RLTY/Integration/Transform randomizer")]
#endif
    public class TransformRandomizer : RLTYMonoBehaviour
    {
#if UNITY_EDITOR
        #region Global Variables       
        [Title("Amplitudes")]
        [SerializeField, ShowIf("showUtilities", true)]
        bool separateAxis = false;
        [SerializeField, HorizontalGroup("Scale"), Range(0.1f, 1)]
        float scaleMin = 0.1f;
        [SerializeField, HorizontalGroup("Scale"), Range(1, 10)]
        float scaleMax = 10;
        [SerializeField, ShowIf("separateAxis", true)]
        float yScaleMin, yScaleMax, zScaleMin, ZScaleMax;

        [SerializeField, HorizontalGroup("Rotation"), Range(-180, 180), Space(10)]
        float rotationMin = 0.1f;
        [SerializeField, Range(180, 360)]
        float rotationMax = 360;
        [SerializeField]
        bool freezeAxis = true;
        [SerializeField, ShowIf("freezeAxis", true) /*, HorizontalGroup("Axis")*/]
        bool x = true;
        [SerializeField, ShowIf("freezeAxis", true) /*, HorizontalGroup("Axis")*/]
        bool y = false;
        [SerializeField, ShowIf("freezeAxis", true) /*, HorizontalGroup("Axis")*/]
        bool z = false;

        Vector3 initialScale = Vector3.zero;
        Quaternion initialRotation = Quaternion.identity;
        #endregion

        #region EditorOnly Logic
#if UNITY_EDITOR
        private void RandomizePosition()
        {
            if(initialScale == Vector3.zero)
                initialScale = transform.localScale;

            float commonScaleFactor = Random.Range(scaleMin, scaleMax);

            if (separateAxis)
                transform.localScale = new Vector3(
                    commonScaleFactor * transform.localScale.x,
                    Random.Range(scaleMin, scaleMax) * transform.localScale.y,
                    Random.Range(scaleMin, scaleMax) * transform.localScale.z);
            else
                transform.localScale = new Vector3(
                    commonScaleFactor * transform.localScale.x,
                    commonScaleFactor * transform.localScale.y,
                    commonScaleFactor * transform.localScale.z);
        }

        private void RandomizeRotation()
        {
            float newRotationX, newRotationY, newRotationZ;

            if (initialRotation == Quaternion.identity)
                initialRotation = transform.rotation;

            if (x) newRotationX = 0;
            else newRotationX = Random.Range(rotationMin, rotationMax) * transform.rotation.x;

            if (y) newRotationY = 0;
            else newRotationY = Random.Range(rotationMin, rotationMax) * transform.rotation.y;

            if (z) newRotationZ = 1;
            else newRotationZ = Random.Range(rotationMin, rotationMax) * transform.rotation.z;

            transform.Rotate(new Vector3(newRotationX, newRotationY, newRotationZ), Space.Self);
        }

        [Button("Randomize")]
        private void RandomizeBoth()
        {
            Undo.RegisterCompleteObjectUndo(this.gameObject, "Restore transform");

            RandomizePosition();
            RandomizeRotation();
        }

        [Button("Restore")]
        private void Restore()
        {
            transform.localScale = initialScale;
            transform.rotation = initialRotation;
        }
#endif
        #endregion
#endif

        public override void EventHandlerRegister()
        { }
        public override void EventHandlerUnRegister()
        { }

    }

}

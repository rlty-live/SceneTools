﻿
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using System.Linq;

namespace Judiva.Metaverse.Interactions
{
#if UNITY_EDITOR
    [AddComponentMenu("RLTY/Interaction/Trigger Zone"), HideMonoScript, RequireComponent(typeof(Collider))]
#endif
    public class TriggerZone : RLTYMonoBehaviourBase
    {

        /*
        #region Global Variables
        //[SerializeField]
        //private List<IPlayer> _inside = new List<IPlayer>();
*/
        #region EditorDisplay
        [PropertyOrder(40), BoxGroup("Display", VisibleIf ="showUtilities")]
        public bool alwaysDisplay = false;
        [PropertyOrder(41), ShowIf("showUtilities", true), ShowIf("alwaysDisplay", true), BoxGroup("Display")]
        public bool solidColor = false;
        [SerializeField, Range(0, 1)]
        [PropertyOrder(42), ShowIf("showUtilities", true), ShowIf("alwaysDisplay", true), BoxGroup("Display")]
        private float opacity = 0.5f;

        private static UnityEngine.Color
            idle = UnityEngine.Color.yellow,
            activatedByOther = UnityEngine.Color.blue,
            activatedByPlayer = UnityEngine.Color.red,
            activatedByBoth = UnityEngine.Color.magenta;
        #endregion
/*
        #endregion

        #region Events

        public event Action<TriggerZone> onEmpty, onNotEmpty, onPlayerEnter, onPlayerExit;
        public event Action<TriggerZone, IPlayer> onEnter, onExit, onOtherEnter, onOtherExit;

        [BoxGroup("Events"), Space(5)]
        public UnityEvent OnPlayerEnter, OnPlayerExit, OnOtherEnter, OnOtherExit, OnEmpty, OnNotEmpty;

        #endregion

        #region Properties

        public int InsideCount { get { return _inside.Count; } }

        public List<IPlayer> Inside {  get { return _inside; } }

        public static List<TriggerZone> All = new List<TriggerZone>();

        #endregion

        #region Public Methods

        /// <summary>
        /// This method is useful when teleporting player, as OnTriggerExit may not be called
        /// </summary>
        public void ForcePlayerExit(Vector3 newPosition)
        {
            IPlayer avatar = AllPlayers.Me;
            Vector3 localPosition = transform.InverseTransformPoint(newPosition);
            if (_inside.Contains(avatar))
            {
                //let's see if newPosition is inside collider
                Collider[] colliders = transform.GetComponents<Collider>();
                foreach (Collider c in colliders)
                {
                    if (c is SphereCollider)
                    {
                        SphereCollider s = c as SphereCollider;
                        if ((localPosition - s.center).magnitude < s.radius)
                            return;
                    }
                    if (c is BoxCollider)
                    {
                        BoxCollider b = c as BoxCollider;
                        Vector3 v=localPosition - b.center;
                        if (Mathf.Abs(v.x) <= b.size.x / 2 && Mathf.Abs(v.y) <= b.size.y / 2 && Mathf.Abs(v.z) <= b.size.z / 2)
                            return;
                    }
                }
                _inside.Remove(avatar);
                if (_inside.Count == 0)
                    onEmpty?.Invoke(this);
                onPlayerExit?.Invoke(this);
            }
        }

        public bool IsInside(IPlayer avatar)
        {
            return _inside.Contains(avatar);
        }

        #endregion

        #region UnityLoop

        private void Start()
        {
            AllPlayers.OnPlayerLeft += (avatar) =>
            {
                if (_inside.Contains(avatar))
                {
                    _inside.Remove(avatar);
                    if (avatar == AllPlayers.Me)
                        onPlayerExit?.Invoke(this);
                    else
                        onOtherExit?.Invoke(this, avatar);
                    if (_inside.Count == 0)
                        onEmpty?.Invoke(this);
                }
            };
            onPlayerEnter += (x) => OnPlayerEnter?.Invoke();
            onPlayerExit += (x) => OnPlayerExit?.Invoke();
            onOtherEnter += (t, p) => OnOtherEnter?.Invoke();
            onOtherExit += (t, p) => OnOtherExit?.Invoke();
            onEmpty += (x) => OnEmpty?.Invoke();
            onNotEmpty += (x) => OnNotEmpty?.Invoke();
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<CharacterController>() == null)
                return;
            IPlayer avatar = other.GetComponentInParent<IPlayer>();
            if (avatar == null) return;
            if (!_inside.Contains(avatar))
            {
                bool wasEmpty = (_inside.Count == 0);
                _inside.Add(avatar);
                if (wasEmpty)
                    onNotEmpty?.Invoke(this);
                onEnter?.Invoke(this, avatar);
                if (IsPlayer(other))
                    onPlayerEnter?.Invoke(this);
                else
                    onOtherEnter?.Invoke(this, avatar);
            }
        }

        void OnTriggerExit(Collider other)
        {
            IPlayer avatar = other.GetComponentInParent<IPlayer>();
            if (avatar == null) return;
            if (_inside.Contains(avatar))
            {
                _inside.Remove(avatar);
                if (_inside.Count == 0)
                    onEmpty?.Invoke(this);
                onExit?.Invoke(this, avatar);
                if (IsPlayer(other))
                    onPlayerExit?.Invoke(this);
                else
                    onOtherExit?.Invoke(this, avatar);
            }
        }

        private void OnEnable()
        {
            All.Add(this);
        }

        private void OnDisable()
        {
            All.Remove(this);
        }

        #endregion

        #region Private
        private bool IsPlayer(Collider c)
        {
            IPlayer avatar = c.GetComponentInParent<IPlayer>();
            return avatar != null ? avatar == AllPlayers.Me : false;
        }

        #endregion
*/
        #region Editor
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
                return;
            if (gameObject.layer!=2) gameObject.layer = 2;

            Collider[] c = GetComponents<Collider>();
            MeshCollider newCO;

            foreach (Collider co in c)
            {
                if (co.GetType() == typeof(MeshCollider))
                {
                    newCO = (MeshCollider)co;
                    newCO.convex = true;
                }

                co.isTrigger = true;
            }
        }
        
        private void OnDrawGizmos()
        {
            if (alwaysDisplay)
            {
                if (TryGetComponent(out MeshCollider _meshC))
                    foreach (MeshCollider meshC in GetComponents<MeshCollider>())
                        if (solidColor)
                            Gizmos.DrawMesh(meshC.sharedMesh, 0, meshC.transform.position, meshC.transform.rotation, meshC.transform.localScale);
                        else
                            Gizmos.DrawWireMesh(meshC.sharedMesh, 0, meshC.bounds.center, meshC.transform.rotation, meshC.transform.localScale);

                Gizmos.matrix = transform.localToWorldMatrix;

                if (TryGetComponent(out BoxCollider _boxC))
                    foreach (BoxCollider boxC in GetComponents<BoxCollider>())
                        if (solidColor)
                            Gizmos.DrawCube(boxC.center, boxC.size);
                        else
                            Gizmos.DrawWireCube(boxC.center, boxC.size);

                if (TryGetComponent(out SphereCollider _sphereC))
                    foreach (SphereCollider sphereC in GetComponents<SphereCollider>())
                    {
                        if (solidColor)
                            Gizmos.DrawSphere(sphereC.center, sphereC.radius);
                        else
                            Gizmos.DrawWireSphere(sphereC.center, sphereC.radius);
                    }
            }
        }
#endif
        #endregion
    }
}

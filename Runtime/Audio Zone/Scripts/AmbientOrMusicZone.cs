using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class AmbientOrMusicZone : SceneTool
{
    public AudioClip AudioClip;
    [PropertyRange(0,1)]
    public float Volume = 1;
    [PropertyRange(0,50)]
    public int Priority = 1;

    public bool IsGlobal;

    //[HideInInspector]
    public Collider Collider;

    [Button]
    private void Reset()
    {
        Collider = GetComponent<Collider>();
    }
}

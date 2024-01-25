using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

//[RequireComponent(BoxCollider)]
public class ClimbableWall : SceneTool
{
    [ReadOnly]
    public uint WallID;

    private void OnValidate()
    {
        uint id = 1;
        foreach (ClimbableWall wall in FindObjectsByType<ClimbableWall>(FindObjectsSortMode.InstanceID))
        {
            wall.WallID = id;
            id++;
        }
    }
}

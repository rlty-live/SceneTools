using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class BlockedArea : NetworkSceneTool
{
    public string name;

    public Sprite buttonOpenImage;
    public Sprite buttonCloseImage;

    public Transform buttonPosition;
    public bool IsAdminOnly = false;

    [ListDrawerSettings(CustomAddFunction = nameof(Testo), CustomRemoveElementFunction = nameof(Testa))]
    public List<GameObject> RoomWalls;

    public GameObject Testo()
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.transform.SetParent(this.transform);
        obj.transform.position = this.transform.position;
        obj.name = "Wall";
        DestroyImmediate(obj.GetComponent<BoxCollider>());
        return obj;
    }
    
    public void Testa(GameObject obj)
    {
        RoomWalls.Remove(obj);
        DestroyImmediate(obj);
    }
}

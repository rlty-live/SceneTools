using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AddCanvasManager : MonoBehaviour
{
    public AddCanvas[] addCanvases;

    public void AddAddCanvases()
    {
        List<AddCanvas> tempAddCanvasList = new List<AddCanvas>();

        foreach(Transform tr in GetComponentsInChildren<Transform>())
        {
            if (tr != transform)
                tempAddCanvasList.Add(tr.gameObject.AddComponent<AddCanvas>());
        }

        addCanvases = tempAddCanvasList.ToArray();
    }

    public void AddCenteredChildren()
    {
        foreach (AddCanvas addCanvas in addCanvases)
            addCanvas.AddCenteredChild();
    }

    public void SetupSprites()
    {
        foreach (AddCanvas addCanvas in addCanvases)
            addCanvas.SetUpSpriteRenderer();
    }

    public void AddBorders()
    {
        foreach (AddCanvas addCanvas in addCanvases)
            addCanvas.AddBorders();
    }

    public void RemoveChildren()
    {
        foreach (AddCanvas addCanvas in addCanvases)
            DestroyImmediate(addCanvas.centeredChild);
    }

    public void RemoveAddCanvases()
    {
        foreach (AddCanvas addCanvas in addCanvases)
        {
            DestroyImmediate(addCanvas);
            addCanvases = null;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(AddCanvasManager))]
public class AddCanvasManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        AddCanvasManager addCanvasManager = (AddCanvasManager)target;

        if (GUILayout.Button("Add AddCanvases on children"))
            addCanvasManager.AddAddCanvases();

        if (GUILayout.Button("Add centered children"))
            addCanvasManager.AddCenteredChildren();

        if (GUILayout.Button("Setup sprites"))
            addCanvasManager.SetupSprites();

        if (GUILayout.Button("Add borders"))
            addCanvasManager.AddBorders();

        if (GUILayout.Button("Remove centered Child"))
            addCanvasManager.RemoveChildren();

        if (GUILayout.Button("Remove AddCanvas"))
            addCanvasManager.RemoveAddCanvases();
    }
}
#endif

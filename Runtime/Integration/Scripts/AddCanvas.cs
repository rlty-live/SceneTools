using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum ForwardDirection
{
    positiveX,
    negativeX,
    positiveZ,
    negativeZ
}

public class AddCanvas : MonoBehaviour
{
    public string demoSpritePath = "Assets/Customisation Samples/Textures/Yourlogo.png";

    Renderer thisRenderer;

    public Vector3 center;
    public Vector3 boundSize;
    public GameObject centeredChild;
    public SpriteRenderer centeredChildRenderer;

    public Sprite demoSprite;

    public ForwardDirection forwardDirection = ForwardDirection.positiveX;

    [Range(0.01f, 0.1f)]
    public float bordersRatio = 0.01f;

    bool debug;

    //Create centered child
    public void AddCenteredChild()
    {
        if (TryGetComponent<Renderer>(out Renderer rdr))
        {
            thisRenderer = GetComponent<Renderer>();
            center = thisRenderer.bounds.center;
            boundSize = thisRenderer.bounds.size;

            centeredChild = new GameObject(transform.name + "_center", typeof(SpriteRenderer));
            centeredChildRenderer = centeredChild.GetComponent<SpriteRenderer>();

            centeredChild.transform.position = center;
            centeredChild.transform.SetParent(transform);
            centeredChild.isStatic = true;
        }

        else
            if (debug) Debug.Log("Gameobject does not contain Renderer", this);
    }

    public void SetUpSpriteRenderer()
    {
#if UNITY_EDITOR
        demoSprite = (Sprite)AssetDatabase.LoadAssetAtPath(demoSpritePath, typeof(Sprite));
#endif

        centeredChildRenderer.sprite = demoSprite;
        centeredChildRenderer.drawMode = SpriteDrawMode.Sliced;
        centeredChildRenderer.sortingOrder = 0;

        FindOrientation();

        if(forwardDirection == ForwardDirection.positiveX || forwardDirection == ForwardDirection.negativeX)
        {
            centeredChildRenderer.bounds.size.Set(thisRenderer.bounds.size.x, thisRenderer.bounds.size.y, thisRenderer.bounds.size.z);
            centeredChildRenderer.size = new Vector2(boundSize.z, boundSize.y);
            centeredChild.transform.Rotate(new Vector3(0, 90, 0));
        }

        if(forwardDirection == ForwardDirection.positiveZ || forwardDirection == ForwardDirection.negativeZ)
        {
            centeredChildRenderer.bounds.size.Set(thisRenderer.bounds.size.x, thisRenderer.bounds.size.y, thisRenderer.bounds.size.z);
            centeredChildRenderer.size = new Vector2(boundSize.x, boundSize.y);
        }
    }

    public void FindOrientation()
    {
        float smallestDimension = 0;

        if (boundSize.x > smallestDimension)
            smallestDimension = boundSize.x;

        if (boundSize.y < smallestDimension)
            smallestDimension = boundSize.y;

        if (boundSize.z < smallestDimension)
            smallestDimension = boundSize.z;

        if (smallestDimension == boundSize.x)
            forwardDirection = ForwardDirection.positiveX;

        if (smallestDimension == boundSize.z)
            forwardDirection = ForwardDirection.positiveZ;
    }

    public void AddBorders()
    {
        centeredChildRenderer.size = new Vector2(
            centeredChildRenderer.size.x - (centeredChildRenderer.size.x * bordersRatio),
            centeredChildRenderer.size.y - (centeredChildRenderer.size.x * bordersRatio));
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(AddCanvas)), CanEditMultipleObjects]
public class AddCanvasEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        AddCanvas addCanvas = (AddCanvas)target;

        if (GUILayout.Button("Add centered child"))
            addCanvas.AddCenteredChild();

        if (GUILayout.Button("Setup Renderer"))
            addCanvas.SetUpSpriteRenderer();

        if (GUILayout.Button("Add borders"))
            addCanvas.AddBorders();

        if (GUILayout.Button("Remove centered child"))
            DestroyImmediate(addCanvas.centeredChild);
    }
}
#endif

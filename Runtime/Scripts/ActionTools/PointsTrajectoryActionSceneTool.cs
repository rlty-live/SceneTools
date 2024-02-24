using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[ExecuteInEditMode]
[AddComponentMenu("RLTY/SceneTools/Points Trajectory")]
public class PointsTrajectoryActionSceneTool : TransformActionSceneTool
{
    [TitleGroup("Points Data")]
    public List<Transform> Points = new List<Transform>();

    protected override bool IsDataValid()
    {
        return base.IsDataValid() && Points.Count > 0 && !Points.Contains(null);
    }

    protected override void DrawGizmos()
    {
        Dictionary<MeshFilter, Transform> meshes = new Dictionary<MeshFilter, Transform>();
        foreach (Transform childTr in Target.GetComponentsInChildren<Transform>())
        {
            if (childTr.TryGetComponent(out MeshFilter meshFilter))
            {
                meshes.Add(meshFilter, childTr);
            }
        }

        for (int i = 0; i < Points.Count; i++)
        {
            if (i == Points.Count-1) Gizmos.color = Color.red;
            else Gizmos.color = Color.yellow;
            foreach (KeyValuePair<MeshFilter, Transform> kvp in meshes)
                DrawMesh(kvp.Key.sharedMesh, Points[i].position, kvp.Value.rotation, kvp.Value.lossyScale);

            Gizmos.color = Color.blue;
            if (i < Points.Count - 1)
                Gizmos.DrawLine(Points[i].position, Points[i+1].position);
        }
        Gizmos.DrawLine(Target.position, Points[0].position);
        
        if (ResetMethod == EResetMethod.Fast)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(Target.position, Points[^1].position);
        }
            
    }

#if UNITY_EDITOR
    
    private void Update()
    {
        Points.RemoveAll((tr) => tr == null);
    }

    [HorizontalGroup, Button]
    private void AddTarget()
    {
        Points.Add(Target);
    }
    
    [HorizontalGroup, Button]
    private void CreatePoint()
    {
        GameObject go = new GameObject
        {
            name = $"Point ({Points.Count + 1})",
            transform =
            {
                parent = transform,
                localPosition = Vector3.zero
            }
        };
        Points.Add(go.transform);
    }
    
#endif
}
using Judiva.Metaverse.Interactions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class RLTYGameobjectMenu : Editor
{
    const string toolbarfolderName = "GameObject/RLTY/";
    const string assetFolderName = "Packages/live.rlty.scenetools/Runtime/Prefabs/";
    public enum RLTYPrefabType
    {
        TriggerZone,
        VideoPlayer,
        SpawnPoint,
        Teleport,
        Jump,
        MusicArea,
        SocialWall,
        Frame_Admin,
        Frame_Public,
        Zoomable,
        VisioArea,
        PopUpArea
    }

    public static Object LoadPrefab(RLTYPrefabType type)
    {
        string path = assetFolderName + "Simple/" + type.ToString() + ".prefab";
        Object asset = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(asset ,EditorSceneManager.GetActiveScene());
        GameObjectUtility.SetParentAndAlign(instance, Selection.activeGameObject);
        Undo.RegisterCreatedObjectUndo(instance, "Delete created object");
        Selection.activeObject = instance;
        return instance;
    }

    #region instantiation
    [MenuItem(toolbarfolderName + "TriggerZone")]
    public static void CreateTriggerZoneInstance() => LoadPrefab(RLTYPrefabType.TriggerZone);

    [MenuItem(toolbarfolderName + "VideoPlayer")]
    public static void CreateVideoStreamPrefab() => LoadPrefab(RLTYPrefabType.VideoPlayer);

    [MenuItem(toolbarfolderName + "SpawnPoint")]
    public static void CreateSpawnPoint() => LoadPrefab(RLTYPrefabType.SpawnPoint);

    [MenuItem(toolbarfolderName + "Teleporter")]
    public static void CreateTeleporter() => LoadPrefab(RLTYPrefabType.Teleport);

    [MenuItem(toolbarfolderName + "Jumper")]
    public static void CreateJumper() => LoadPrefab(RLTYPrefabType.Jump);

    [MenuItem(toolbarfolderName + "MusicArea")]
    public static void CreateMusicArea() => LoadPrefab(RLTYPrefabType.MusicArea);

    [MenuItem(toolbarfolderName + "SocialWall")]
    public static void CreateSocialWall() => LoadPrefab(RLTYPrefabType.SocialWall);

    [MenuItem(toolbarfolderName + "Admin Frame")]
    public static void CreateAdminFrame() => LoadPrefab(RLTYPrefabType.Frame_Admin);

    [MenuItem(toolbarfolderName + "Admin Frame")]
    public static void CreateUserFrame() => LoadPrefab(RLTYPrefabType.Frame_Public);

    [MenuItem(toolbarfolderName + "Zoomable object")]
    public static void CreateZoomableFrame() => LoadPrefab(RLTYPrefabType.Zoomable);

    #endregion

    #region CreateFromScratch
    //public void CreateTriggerZoneGameObject()
    //{
    //    GameObject go = new GameObject("TriggerZone");

    //    TriggerZone tZ = go.AddComponent<TriggerZone>();
    //    SphereCollider sC = go.AddComponent<SphereCollider>();
    //    BoxCollider bC = go.AddComponent<BoxCollider>();
    //    MeshCollider mC = go.AddComponent<MeshCollider>();

    //    tZ.alwaysDisplay = true;
    //    tZ.solidColor = true;

    //    sC.isTrigger = true;
    //    bC.isTrigger = true;
    //    sC.center = new Vector3(1, 0, 0);
    //    bC.center += new Vector3(2, 0, 0);

    //    GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
    //    mC.sharedMesh = cylinder.GetComponent<MeshFilter>().sharedMesh;
    //    DestroyImmediate(cylinder);
    //    mC.convex = true;
    //    mC.isTrigger = true;

    //    go.transform.SetParent(Selection.activeGameObject.transform);
    //    go.transform.localPosition = Vector3.zero;
    //}
    #endregion

}

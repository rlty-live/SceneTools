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
        TestAvatar,
        TriggerZone,
        SpawnPoint,
        Teleport,
        Jump,
        MusicArea,
        VideoPlayer,
        SocialWall,
        Frame_Admin,
        Frame_Public,
        Zoomable,
        VisioArea,
        PopUp_Area,
        PopUp_Clickbox,
    }

    public static Object LoadPrefab(RLTYPrefabType type, bool simple)
    {
        string category = simple ? "Simple" : "Advanced";
        string path = assetFolderName + category + "/" + type.ToString() + ".prefab";

        Transform sceneViewCameraTransform = SceneView.lastActiveSceneView.camera.transform;

        Object asset = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, EditorSceneManager.GetActiveScene());
        GameObjectUtility.SetParentAndAlign(instance, Selection.activeGameObject);
        instance.transform.position = sceneViewCameraTransform.position + sceneViewCameraTransform.forward*5;

        Undo.RegisterCreatedObjectUndo(instance, "Delete created object");
        Selection.activeObject = instance;

        return instance;
    }

    #region instantiation
    [MenuItem(toolbarfolderName + "Advanced/" + "Test Avatar")]
    public static void CreateTestAvatar() => LoadPrefab(RLTYPrefabType.TestAvatar, false);

    [MenuItem(toolbarfolderName + "Advanced/" + "TriggerZone")]
    public static void CreateTriggerZoneInstance() => LoadPrefab(RLTYPrefabType.TriggerZone, false);

    [MenuItem(toolbarfolderName + "SpawnPoint")]
    public static void CreateSpawnPoint() => LoadPrefab(RLTYPrefabType.SpawnPoint, true);

    [MenuItem(toolbarfolderName + "Teleporter")]
    public static void CreateTeleporter() => LoadPrefab(RLTYPrefabType.Teleport, true);

    [MenuItem(toolbarfolderName + "Jumper")]
    public static void CreateJumper() => LoadPrefab(RLTYPrefabType.Jump, true);

    [MenuItem(toolbarfolderName + "MusicArea")]
    public static void CreateMusicArea() => LoadPrefab(RLTYPrefabType.MusicArea, true);

    [MenuItem(toolbarfolderName + "VisioArea")]
    public static void CreateVisioArea() => LoadPrefab(RLTYPrefabType.VisioArea, true);

    [MenuItem(toolbarfolderName + "VideoPlayer")]
    public static void CreateVideoStreamPrefab() => LoadPrefab(RLTYPrefabType.VideoPlayer, true);

    [MenuItem(toolbarfolderName + "SocialWall")]
    public static void CreateSocialWall() => LoadPrefab(RLTYPrefabType.SocialWall, true);

    [MenuItem(toolbarfolderName + "Admin Frame")]
    public static void CreateAdminFrame() => LoadPrefab(RLTYPrefabType.Frame_Admin, true);

    [MenuItem(toolbarfolderName + "User Frame")]
    public static void CreateUserFrame() => LoadPrefab(RLTYPrefabType.Frame_Public, true);

    [MenuItem(toolbarfolderName + "Zoomable object")]
    public static void CreateZoomableFrame() => LoadPrefab(RLTYPrefabType.Zoomable, true);

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

using System.Collections.Generic;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEditor;
using RLTY.Customisation;
using Newtonsoft.Json;
using Sirenix.Utilities;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

[CreateAssetMenu(fileName = "AssetBundleSceneSetup", menuName = "RLTY/BuildSetup/Assetbundles", order = 1)]
public class AssetbundleBuildSetup : ScriptableObject
{
    public bool useCustomFolder = false;
    public string customFolderPath = "../../AssetBundles";

    public string StreamingAssetsLocalPath
    {
        get
        {
            string tmp = useCustomFolder ? customFolderPath : "../../AssetBundles";
            if (!tmp.Contains(":"))
                tmp = Application.dataPath + "/" + tmp;
            return tmp;
        }
    }

    public string PublishS3Path
    {
        get
        {
            return "../Publish";
        }
    }

    [System.Serializable]
    public class Environment
    {
        public string id;

        //To use when loading assetbundles
        [HideInInspector]
        public string variant;
        public List<SceneAsset> scenes = new List<SceneAsset>();
        public bool rebuild = true;
    }

    public List<Environment> environmentList;

    private void OnValidate()
    {
        foreach (Environment e in environmentList)
        {
            e.scenes.Sort((x, y) => AssetDatabase.GetAssetPath(x).CompareTo(AssetDatabase.GetAssetPath(y)));

            //Make ID compatible with assetbundle allowed characters
            Regex rgx = new Regex("[^a-z-]");
            e.id = rgx.Replace(e.id, "");
            e.variant = rgx.Replace(e.variant, "");

            //Apply AssetBundleTag
            //foreach (SceneAsset sceneAsset in e.scenes)
            //    if (sceneAsset)
            //        AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sceneAsset)).SetAssetBundleNameAndVariant(e.id, e.variant);
        }

        //Check for duplicate and add numbering if found
        foreach (Environment e in environmentList)
        {
            int duplicateIndex = 1;

            foreach (Environment e1 in environmentList)
            {

                if (e != e1 && e.id == e1.id)
                {
                    e1.id = e.id + duplicateIndex;
                    duplicateIndex++;
                }
            }
        }

        foreach (Environment e in environmentList)
            Debug.Log(e);
    }



    public void PrepareForBuild()
    {
        //build a list of all scenes in project
        string sceneType = typeof(SceneAsset).ToString();
        int k = sceneType.LastIndexOf(".");
        if (k > 0)
            sceneType = sceneType.Substring(k + 1);
        string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", sceneType));

        //clear their assetbundle assignment
        foreach (string g in guids)
            AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(g)).SetAssetBundleNameAndVariant("", "");

        //prepare assignment for scenes that we actually want to build
        foreach (Environment e in environmentList)
        {
            if (e.rebuild)
            {
                foreach (SceneAsset s in e.scenes)
                {
                    string assetPath = AssetDatabase.GetAssetPath(s);
                    AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant(e.id, "");
                }
            }
        }
    }
}


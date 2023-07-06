using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using Sirenix.Utilities;
using Sirenix.OdinInspector;
using UnityEditor.SceneManagement;

[HideMonoScript]
[CreateAssetMenu(fileName = "AssetBundle scene setup", menuName = "RLTY/BuildSetup/Assetbundles", order = 1)]
public class AssetbundleBuildSetup : ScriptableObject
{
    public bool useCustomFolder = false;
    [ShowIf("useCustomFolder")]
    public string customFolderPath = "../AssetBundles";
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

    public List<Environment> environmentList;
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

    private bool environmentExists = false;

    private void OnValidate()
    {
        CheckSetup();
        //foreach (Environment e in environmentList)
        //    Debug.Log(e);
    }

    //[Button, ShowIf("useCustomFolder")]
    //public void OpenDestinationFolder()
    //{
    //    EditorUtility.RevealInFinder(System.IO.Directory.GetParent(System.IO.Directory.GetParent(Application.dataPath).ToString()).ToString());
    //}

    [Button, HideIf("environmentExists")]
    public void AddCurrentSceneSetup()
    {
        if(!environmentExists)
        {
            Environment newEnvironment = new AssetbundleBuildSetup.Environment();
            newEnvironment.id = AlphaNumeric(EditorSceneManager.GetActiveScene().name, true);

            //Populate with loaded scenes
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                newEnvironment.scenes.Add((SceneAsset)AssetDatabase.LoadAssetAtPath(EditorSceneManager.GetSceneAt(i).path, typeof(SceneAsset)));
                Debug.Log(EditorSceneManager.GetActiveScene().name);
            }

            newEnvironment.rebuild = true;
            environmentList.Add(newEnvironment);

            EditorUtility.SetDirty(this);
        }
        else
        {
            JLogBase.Log("Can't create new environement as it already exists", typeof(AssetbundleBuildSetup));
        }

    }

    public void CheckSetup()
    {
        //Format environments IDs
        foreach (Environment e in environmentList)
        {
            e.scenes.Sort((x, y) => AssetDatabase.GetAssetPath(x).CompareTo(AssetDatabase.GetAssetPath(y)));

            //Make ID compatible with assetbundle allowed characters
            e.id = AlphaNumeric(e.id, true);
            e.variant = AlphaNumeric(e.variant, true);
        }

        //Check for duplicate and add copy index if found
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

            //Deactivate building of empty Environments
            bool containsValidScenes = false;

            if (!e.scenes.IsNullOrEmpty())
            {
                foreach (SceneAsset scene in e.scenes)
                {
                    if (scene != null)
                        containsValidScenes = true;
                }
            }
            if (!containsValidScenes)
                e.rebuild = false;
        }

        //Check if current scene already exists in the environement list
        string environmentID = AlphaNumeric(EditorSceneManager.GetActiveScene().name, true);
        foreach (Environment env in environmentList)
        {
            if (env.id == environmentID)
            {
                environmentExists = true;
                return;
            }
        }

        environmentExists = false;
    }

    /// <summary>
    /// Will return a string containing only letters, numbers and "-"
    /// </summary>
    /// <param name="_string">The string to format</param>
    /// <param name="lowerCase">Should highercase characters be authorized ?</param>
    /// <returns>Formated string</returns>
    public static string AlphaNumeric(string _string, bool lowerCase)
    {
        Regex rgx;

        if (lowerCase)
        {
            rgx = new Regex("[^a-z0-9-]");
            _string = _string.ToLower();
        }

        else
            rgx = new Regex("[^a-zA-Z0-9-]");

        _string = rgx.Replace(_string, "");
        return _string;
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
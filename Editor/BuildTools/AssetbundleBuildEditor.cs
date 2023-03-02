using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;
using System;
using System.Collections;
using ICSharpCode.SharpZipLib.Zip;
using Unity.EditorCoroutines.Editor;
using System.Collections.Generic;
using System.Diagnostics;
using RLTY.Customisation;
using UnityEditor.SceneManagement;
using Newtonsoft.Json;

public class AssetbundleBuildEditor : EditorWindow
{
    private static AssetbundleBuildSetup _setup;
    private static string tmpDirectoryName = "RLTYTmp2";

    private static List<PlayerTarget> _assetbundleTargets = new List<PlayerTarget>();

    #region Helpers

    class PlayerTarget
    {
        public BuildTarget target;
        public bool server = false;
        public bool build = false;
        public bool headless = false;

        public string Name
        {
            get
            {
                switch (target)
                {
                    case BuildTarget.WebGL:
                        return "WebGL";
                    case BuildTarget.StandaloneWindows64:
                        return "Win " + (headless ? "Headless " : "") + Label;
                    case BuildTarget.StandaloneLinux64:
                        return "Linux " + (headless ? "Headless " : "") + Label;
                }
                return "unnamed";
            }
        }

        public string Label
        {
            get
            {
                return server ? "Server" : "Client";
            }
        }
    }

    private static string GetAssetBundlePath(BuildTarget target, StandaloneBuildSubtarget subTarget)
    {
        if (target == BuildTarget.WebGL)
            return GetWebGLAssetBundlePath();
        return _setup.StreamingAssetsLocalPath + "/" + target + "/" + (subTarget == StandaloneBuildSubtarget.Server ? "Server" : "Client");
    }

    private static string GetManifestPath()
    {
        return _setup.StreamingAssetsLocalPath + "/Manifest";
    }

    private static string GetWebGLAssetBundlePath()
    {
        return _setup.StreamingAssetsLocalPath + "/" + BuildTarget.WebGL;
    }

    private static string GetLinuxAssetBundlePath()
    {
        return _setup.StreamingAssetsLocalPath + "/" + BuildTarget.StandaloneLinux64 + "/" + "Server";
    }

    private static void FindSetup()
    {
        if (_setup == null)
        {
            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(AssetbundleBuildSetup)));
            if (guids.Length > 0)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                _setup = AssetDatabase.LoadAssetAtPath<AssetbundleBuildSetup>(assetPath);
            }
        }
    }

    private static BuildTargetGroup GetTargetGroupForTarget(BuildTarget target) => target switch
    {
        BuildTarget.StandaloneOSX => BuildTargetGroup.Standalone,
        BuildTarget.StandaloneWindows => BuildTargetGroup.Standalone,
        BuildTarget.iOS => BuildTargetGroup.iOS,
        BuildTarget.Android => BuildTargetGroup.Android,
        BuildTarget.StandaloneWindows64 => BuildTargetGroup.Standalone,
        BuildTarget.WebGL => BuildTargetGroup.WebGL,
        BuildTarget.StandaloneLinux64 => BuildTargetGroup.Standalone,
        _ => BuildTargetGroup.Unknown
    };

    #endregion

    [MenuItem("RLTY/AssetBundle Build Editor")]
    private static void Init()
    {
        // Get existing open window or if none, make a new one:
        AssetbundleBuildEditor window = (AssetbundleBuildEditor)EditorWindow.GetWindow(typeof(AssetbundleBuildEditor));
        window.Show();
    }


    // Start is called before the first frame update
    void OnGUI()
    {
        if (_assetbundleTargets.Count == 0)
        {
            _assetbundleTargets.Add(new PlayerTarget { target = BuildTarget.StandaloneWindows64, server = true, headless = true });
            _assetbundleTargets.Add(new PlayerTarget { target = BuildTarget.StandaloneLinux64, server = true, headless = true });
            _assetbundleTargets.Add(new PlayerTarget { target = BuildTarget.StandaloneWindows64, server = false, headless = false });
            _assetbundleTargets.Add(new PlayerTarget { target = BuildTarget.WebGL, server = false, headless = false });
        }

        FindSetup();
        _setup = (AssetbundleBuildSetup)EditorGUILayout.ObjectField("Setup", _setup, typeof(AssetbundleBuildSetup));
        for (int i = 0; i < _assetbundleTargets.Count; i++)
            _assetbundleTargets[i].build = GUILayout.Toggle(_assetbundleTargets[i].build, _assetbundleTargets[i].Name);

        if (float.TryParse(Application.version, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float value))
        {
            if (GUILayout.Button("Build AssetBundles"))
            {
                List<PlayerTarget> list = new List<PlayerTarget>();
                for (int i = 0; i < _assetbundleTargets.Count; i++)
                    if (_assetbundleTargets[i].build)
                        list.Add(_assetbundleTargets[i]);
                EditorCoroutineUtility.StartCoroutine(PerformBuildAssetBundles(list), this);
            }


            GUILayout.Space(40);
        }
        else
            GUILayout.Label("Incorrect version syntax, must be 'X.Y'");
        PlayerSettings.bundleVersion = EditorGUILayout.TextField("Version", Application.version);
        if (GUILayout.Button("Prepare Publishing"))
            PreparePublishToS3();
    }

    #region Assetbundles

    IEnumerator PerformBuildAssetBundles(List<PlayerTarget> targetsToBuild)
    {
        _setup.PrepareForBuild();

        string tmp = Application.dataPath;
        string tmpDirectory = tmp.Substring(0, tmp.IndexOf(":") + 2) + tmpDirectoryName;
        DateTime startTime = DateTime.Now;

        // show the progress display
        int buildAllProgressID = Progress.Start("Build All Assetbundles", "Building all selected platforms", Progress.Options.Sticky);
        Progress.ShowDetails();
        yield return new EditorWaitForSeconds(0.5f);

        BuildTarget originalTarget = EditorUserBuildSettings.activeBuildTarget;
        StandaloneBuildSubtarget originalSubTarget = EditorUserBuildSettings.standaloneBuildSubtarget;

        // build each target
        for (int targetIndex = 0; targetIndex < targetsToBuild.Count; ++targetIndex)
        {
            var buildTarget = targetsToBuild[targetIndex];

            Progress.Report(buildAllProgressID, targetIndex + 1, targetsToBuild.Count);
            int buildTaskProgressID = Progress.Start($"Build {buildTarget.ToString()}", null, Progress.Options.Sticky, buildAllProgressID);
            yield return new EditorWaitForSeconds(0.5f);

            // perform the build
            BuildAssetBundlesForTarget(tmpDirectory, buildTarget.target, buildTarget.headless ? StandaloneBuildSubtarget.Server : StandaloneBuildSubtarget.Player);

            Progress.Finish(buildTaskProgressID, Progress.Status.Succeeded);
            yield return new EditorWaitForSeconds(0.5f);
        }

        Progress.Finish(buildAllProgressID, Progress.Status.Succeeded);
        UnityEngine.Debug.Log("Assetbundle build time=" + (DateTime.Now.Subtract(startTime).TotalSeconds));

        if (EditorUserBuildSettings.activeBuildTarget != originalTarget || EditorUserBuildSettings.standaloneBuildSubtarget != originalSubTarget)
        {
            EditorUserBuildSettings.SwitchActiveBuildTargetAsync(GetTargetGroupForTarget(originalTarget), originalTarget);
        }

        //build manifests
        foreach (var environment in _setup.environmentList)
            if (environment.rebuild)
            {
                List<Customisable> list = new List<Customisable>();
                foreach (SceneAsset sceneAsset in environment.scenes)
                {
                    string path = AssetDatabase.GetAssetPath(sceneAsset);
                    EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                    list.AddRange(FindObjectsOfType<Customisable>());
                }
                SceneManifest manifest = new SceneManifest();
                foreach (Customisable customisable in list)
                {
                    if (customisable.gameObject.activeInHierarchy && customisable.enabled)
                        manifest.Populate(customisable.type, customisable.key, customisable.commentary);
                }

                string data = JsonConvert.SerializeObject(manifest, Formatting.Indented);
                if (!Directory.Exists(GetManifestPath()))
                    Directory.CreateDirectory(GetManifestPath());

                //File.WriteAllText(GetManifestPath() + "/" + environment.id + "_manifest.json", data);
                //File.WriteAllText(GetManifestPath() + "/"+ environment.id + "_static_frames.json", StaticFramesManifest.GetStaticFramesManifest());

                data = data.Remove(data.Length - 1, 1);

                File.WriteAllText(
                    "Assets/" + manifest + ".Json",
                    data + ",\"static_frames\":" + StaticFramesManifest.GetStaticFramesManifest() + "}");
            }

        yield return null;
    }

    private static void BuildAssetBundlesForTarget(string tmpDirectory, BuildTarget target, StandaloneBuildSubtarget subTarget)
    {
        if (Directory.Exists(tmpDirectory))
            Directory.Delete(tmpDirectory, true);
        if (!Directory.Exists(tmpDirectory))
            Directory.CreateDirectory(tmpDirectory);
        string path = tmpDirectory + "/" + target;
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        EditorUserBuildSettings.standaloneBuildSubtarget = subTarget;
        BuildPipeline.BuildAssetBundles(path,
                                BuildAssetBundleOptions.None,
                                target);

        string[] filePaths = Directory.GetFiles(path);
        string assetBundleDirectory = GetAssetBundlePath(target, target == BuildTarget.WebGL ? StandaloneBuildSubtarget.Player : subTarget);
        if (!Directory.Exists(assetBundleDirectory))
            Directory.CreateDirectory(assetBundleDirectory);
        foreach (string file in filePaths)
        {
            string str = assetBundleDirectory + "/" + Path.GetFileName(file);
            string tmp = file.Substring(path.Length);
            if (tmp.Contains(tmpDirectoryName))
                File.Delete(file);
            else
            {
                if (File.Exists(str))
                    File.Delete(str);
                File.Move(file, str);
            }
        }
        Directory.Delete(tmpDirectory, true);
    }

    #endregion

    private static void PreparePublishToS3()
    {
        string path = _setup.PublishS3Path;
        if (Directory.Exists(path))
            Directory.Delete(path, true);
        Directory.CreateDirectory(path);

        string clientAssetPath = path + "/rlty-unity-assets/v" + Application.version + "/client";
        string serverAssetPath = path + "/rlty-unity-assets/v" + Application.version + "/server";

        Directory.CreateDirectory(clientAssetPath);
        Directory.CreateDirectory(serverAssetPath);

        //copy client assetbundles
        UnityEngine.Debug.Log("Copying client assetbundles");
        foreach (string file in Directory.GetFiles(GetWebGLAssetBundlePath()))
        {
            if (!file.Contains("manifest"))
                File.Copy(file, clientAssetPath + "/" + Path.GetFileName(file));
        }

        //copy server assetbundles
        UnityEngine.Debug.Log("Copying linux server assetbundles");
        foreach (string file in Directory.GetFiles(GetLinuxAssetBundlePath()))
        {
            if (!file.Contains("manifest"))
                File.Copy(file, serverAssetPath + "/" + Path.GetFileName(file));
        }

        path = Path.GetFullPath(path);
        UnityEngine.Debug.Log("open folder " + path);
        if (Directory.Exists(path))
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                Arguments = path,
                FileName = "explorer.exe"
            };
            Process.Start(startInfo);
        }
    }
}



namespace FolderZipper
{
    public static class ZipUtil
    {
        public static void ZipFiles(string inputFolderPath, string outputPathAndFile, string password)
        {
            inputFolderPath = Path.GetFullPath(inputFolderPath);
            ArrayList fileList = GenerateFileList(inputFolderPath); // generate file list
            int TrimLength = (inputFolderPath).ToString().Length;

            // find number of chars to remove     // from orginal file path
            TrimLength += 1; //remove '\'
            FileStream ostream;
            byte[] obuffer;
            ZipOutputStream oZipStream = new ZipOutputStream(File.Create(outputPathAndFile)); // create zip stream
            if (password != null && password != String.Empty)
                oZipStream.Password = password;
            oZipStream.SetLevel(9); // maximum compression
            ZipEntry oZipEntry;
            foreach (string file in fileList) // for each file, generate a zipentry
            {
                oZipEntry = new ZipEntry(file.Remove(0, TrimLength));
                oZipStream.PutNextEntry(oZipEntry);

                if (!file.EndsWith(@"/")) // if a file ends with '/' its a directory
                {
                    ostream = File.OpenRead(file);
                    obuffer = new byte[ostream.Length];
                    ostream.Read(obuffer, 0, obuffer.Length);
                    oZipStream.Write(obuffer, 0, obuffer.Length);
                }
            }
            oZipStream.Finish();
            oZipStream.Close();
        }


        private static ArrayList GenerateFileList(string Dir)
        {
            ArrayList fils = new ArrayList();
            bool Empty = true;
            foreach (string file in Directory.GetFiles(Dir)) // add each file in directory
            {
                fils.Add(file);
                Empty = false;
            }

            if (Empty)
            {
                if (Directory.GetDirectories(Dir).Length == 0)
                // if directory is completely empty, add it
                {
                    fils.Add(Dir + @"/");
                }
            }

            foreach (string dirs in Directory.GetDirectories(Dir)) // recursive
            {
                foreach (object obj in GenerateFileList(dirs))
                {
                    fils.Add(obj);
                }
            }
            return fils; // return file list
        }


        public static void UnZipFiles(string zipPathAndFile, string outputFolder, string password, bool deleteZipFile)
        {
            ZipInputStream s = new ZipInputStream(File.OpenRead(zipPathAndFile));
            if (password != null && password != String.Empty)
                s.Password = password;
            ZipEntry theEntry;
            string tmpEntry = String.Empty;
            while ((theEntry = s.GetNextEntry()) != null)
            {
                string directoryName = outputFolder;
                string fileName = Path.GetFileName(theEntry.Name);
                // create directory 
                if (directoryName != "")
                {
                    Directory.CreateDirectory(directoryName);
                }
                if (fileName != String.Empty)
                {
                    if (theEntry.Name.IndexOf(".ini") < 0)
                    {
                        string fullPath = directoryName + "\\" + theEntry.Name;
                        fullPath = fullPath.Replace("\\ ", "\\");
                        string fullDirPath = Path.GetDirectoryName(fullPath);
                        if (!Directory.Exists(fullDirPath)) Directory.CreateDirectory(fullDirPath);
                        FileStream streamWriter = File.Create(fullPath);
                        int size = 2048;
                        byte[] data = new byte[2048];
                        while (true)
                        {
                            size = s.Read(data, 0, data.Length);
                            if (size > 0)
                            {
                                streamWriter.Write(data, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }
                        streamWriter.Close();
                    }
                }
            }
            s.Close();
            if (deleteZipFile)
                File.Delete(zipPathAndFile);
        }
    }
}

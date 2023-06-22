//#define SIMULATEASSETBUNDLECREATION
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Collections;
using ICSharpCode.SharpZipLib.Zip;
using Unity.EditorCoroutines.Editor;
using System.Collections.Generic;
using System.Diagnostics;
using RLTY.Customisation;
using UnityEditor.SceneManagement;
using Path = System.IO.Path;
using Debug = UnityEngine.Debug;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using Mono.Cecil;

public class AssetbundleBuildEditor : EditorWindow
{
    private static AssetbundleBuildSetup _setup;
    private static List<AssetbundleBuildSetup.Environment> buildingsList;
    private static string tmpDirectoryName = "RLTYTmp2";

    private static List<PlayerTarget> _assetbundleTargets = new List<PlayerTarget>();
    private static List<string> _pathsToDelete = new List<string>();

    private static string packageVersion;

    //private static string logs;

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
                        return "Windows " + (headless ? "Headless " : "") + Label;
                    case BuildTarget.StandaloneLinux64:
                        return "Linux " + (headless ? "Headless " : "") + Label;
                    case BuildTarget.iOS:
                        return "iOS " + (headless ? "Headless " : "") + Label;
                    case BuildTarget.Android:
                        return "Android " + (headless ? "Headless " : "") + Label;
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

    #region Paths reconstruction
    private static string GetAssetBundlePath(string environmentid, BuildTarget target, StandaloneBuildSubtarget subTarget)
    {
        if (target == BuildTarget.WebGL)
            return GetWebGLAssetBundlePath(environmentid);
        else if (target == BuildTarget.iOS)
            return GetiOSAssetBundlePath(environmentid);
        else if (target == BuildTarget.Android)
            return GetAndroidAssetBundlePath(environmentid);
        else if (target == BuildTarget.StandaloneWindows64)
            return GetWindowsAssetBundlePath(environmentid, subTarget);
        else if (target == BuildTarget.StandaloneLinux64)
            return GetLinuxAssetBundlePath(environmentid, subTarget);

        //Removed sub folder for linux
        return _setup.StreamingAssetsLocalPath + "/" + environmentid + "/" + target /* + "/" + (subTarget == StandaloneBuildSubtarget.Server ? "Server" : "Client")*/;
    }

    private static string FolderNameFromTargetPlatform(BuildTarget target)
    {
        if (target == BuildTarget.StandaloneWindows64)
            return "Windows";
        else if (target == BuildTarget.StandaloneLinux64)
            return "Linux";
        return target.ToString();
    }

    private static string GetManifestFilePath(string environement)
    {
        return _setup.StreamingAssetsLocalPath + "/" + environement + "/" + "manifest.json";
    }

    private static string GetWebGLAssetBundlePath(string environmentid)
    {
        return _setup.StreamingAssetsLocalPath + "/" + environmentid + "/" + BuildTarget.WebGL;
    }

    private static string GetAndroidAssetBundlePath(string environmentid)
    {
        return _setup.StreamingAssetsLocalPath + "/" + environmentid + "/" + BuildTarget.Android;
    }

    private static string GetiOSAssetBundlePath(string environmentid)
    {
        return _setup.StreamingAssetsLocalPath + "/" + environmentid + "/" + BuildTarget.iOS;
    }

    private static string GetLinuxAssetBundlePath(string environmentid, StandaloneBuildSubtarget subTarget)
    {
        //Removed sub folder for linux
        return _setup.StreamingAssetsLocalPath + "/" + environmentid + "/" + BuildTarget.StandaloneLinux64 /*+ "/" + (subTarget == StandaloneBuildSubtarget.Server ? "Server" : "Client")*/;
    }

    private static string GetWindowsAssetBundlePath(string environmentid, StandaloneBuildSubtarget subTarget)
    {
        return _setup.StreamingAssetsLocalPath + "/" + environmentid + "/" + "Windows" + "/" + (subTarget == StandaloneBuildSubtarget.Server ? "Server" : "Client");
    }
    #endregion

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
        AssetbundleBuildEditor window = (AssetbundleBuildEditor)GetWindow(typeof(AssetbundleBuildEditor));
        window.Show();
    }

    void OnGUI()
    {
        EditorGUILayout.SelectableLabel("Scenetools version: " + packageVersion);

        if (_assetbundleTargets.Count == 0 && _setup)
        {
            _assetbundleTargets.Add(new PlayerTarget { target = BuildTarget.StandaloneWindows64, server = true, headless = true });
            _assetbundleTargets.Add(new PlayerTarget { target = BuildTarget.StandaloneWindows64, server = false, headless = false });
            _assetbundleTargets.Add(new PlayerTarget { target = BuildTarget.StandaloneLinux64, server = true, headless = true });
            _assetbundleTargets.Add(new PlayerTarget { target = BuildTarget.StandaloneLinux64, server = false, headless = true });
            _assetbundleTargets.Add(new PlayerTarget { target = BuildTarget.WebGL, server = false, headless = false });
            _assetbundleTargets.Add(new PlayerTarget { target = BuildTarget.iOS, server = false, headless = false });
            _assetbundleTargets.Add(new PlayerTarget { target = BuildTarget.Android, server = false, headless = false });
        }

        _setup = (AssetbundleBuildSetup)EditorGUILayout.ObjectField("Setup", _setup, typeof(AssetbundleBuildSetup), false);

        foreach (PackageInfo packageInfo in PackageInfo.GetAllRegisteredPackages())
        {
            if (packageInfo.name == "live.rlty.scenetools")
                packageVersion = packageInfo.version;
        }

        //Add Warning Message
        GUILayout.Space(10);
        GUIStyle style = new GUIStyle();
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;
        style.richText = true;
        string setupWarning = "<color=red>Please check your setup file before building</color>";
        GUILayout.Label(setupWarning, style);
        GUILayout.Space(10);

        FindSetup();

        //Hide targets and build button if no setup has been selected
        if (_setup)
        {
            //Create toggle for each target
            for (int i = 0; i < _assetbundleTargets.Count; i++)
                _assetbundleTargets[i].build = GUILayout.Toggle(_assetbundleTargets[i].build, _assetbundleTargets[i].Name);

            //Check all targets
            bool allTargets = false;
            GUILayout.Space(10);
            if (GUILayout.Toggle(allTargets, "All targets"))
                foreach (PlayerTarget target in _assetbundleTargets)
                    target.build = true;

            bool anyTargetSelected = false;

            //Allow build only if targets are selected
            foreach (PlayerTarget target in _assetbundleTargets)
            {
                if (target.build)
                    anyTargetSelected = true;
            }

            //Allow build only if environment are checked for build
            bool anyBundleSelectedForBuild = false;
            foreach (AssetbundleBuildSetup.Environment e in _setup.environmentList)
            {
                if (e.rebuild)
                    anyBundleSelectedForBuild = true;
            }

            //Add warning message if not
            if (anyBundleSelectedForBuild == false)
            {
                GUILayout.Space(10);
                string rebuildWarning = "<color=white>No environment marked for build, please add one</color>";
                GUILayout.Label(rebuildWarning, style);
                GUILayout.Space(10);
            }

            if (anyTargetSelected && anyBundleSelectedForBuild)
            {
                //Allow for build only if setup is present and targets are checked
                if (GUILayout.Button("Build AssetBundles"))
                {
                    List<PlayerTarget> list = new List<PlayerTarget>();

                    //_pathsToDelete.Clear();

                    for (int i = 0; i < _assetbundleTargets.Count; i++)
                        if (_assetbundleTargets[i].build)
                            list.Add(_assetbundleTargets[i]);
                    EditorCoroutineUtility.StartCoroutine(PerformBuildAssetBundles(list), this);
                }
            }
        }

        else
        {
            //Create an assetbundlebuild setup from currently loaded scenes
            if (GUILayout.Button("New setup from current scenes"))
            {
                //Create new AssetbundleBuildSetup Asset
                _setup = AssetbundleBuildSetup.CreateInstance<AssetbundleBuildSetup>();
                AssetDatabase.CreateAsset(_setup, "Assets/AssetBundle build setup.asset");

                //Create environment
                _setup.environmentList = new List<AssetbundleBuildSetup.Environment>();
                AssetbundleBuildSetup.Environment newEnvironment = new AssetbundleBuildSetup.Environment();
                newEnvironment.id = EditorSceneManager.GetActiveScene().name;

                Debug.Log(EditorSceneManager.GetActiveScene().name);

                //Populate with loaded scenes
                for (int i = 0; i < EditorSceneManager.sceneCount; i++)
                {
                    newEnvironment.scenes.Add((SceneAsset)AssetDatabase.LoadAssetAtPath(EditorSceneManager.GetSceneAt(i).path, typeof(SceneAsset)));
                    Debug.Log(EditorSceneManager.GetActiveScene().name);
                }

                newEnvironment.rebuild = true;
                _setup.environmentList.Add(newEnvironment);

                Selection.activeObject = _setup;
            }
        }
    }

    #region Assetbundles

    private static void DeleteTempFolders()
    {
        foreach (var pathtodelete in _pathsToDelete)
        {
            try
            {
                Directory.Delete(pathtodelete, true);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.Message);

                //Meant to save logs in a file, as switching plateform clears the console
                //logs += "\n" + "\n" + DateTime.Now + ": " + e.Message;
            }
        }
    }

    IEnumerator PerformBuildAssetBundles(List<PlayerTarget> targetsToBuild)
    {
        PlayerSettings.bundleVersion = Application.version;
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

        foreach (var environment in _setup.environmentList)
        {
            //delete environement folder
            string envfolder = _setup.StreamingAssetsLocalPath + "/" + environment.id;
            if (Directory.Exists(envfolder))
            {
                Directory.Delete(envfolder, true);
                Directory.CreateDirectory(envfolder);
            }
            // build each target
            for (int targetIndex = 0; targetIndex < targetsToBuild.Count; ++targetIndex)
            {
                var buildTarget = targetsToBuild[targetIndex];

                Progress.Report(buildAllProgressID, targetIndex + 1, targetsToBuild.Count);
                int buildTaskProgressID = Progress.Start($"Build {buildTarget.Name}", null, Progress.Options.Sticky, buildAllProgressID);

                // perform the build
                BuildAssetBundlesForTarget(environment.id, tmpDirectory, buildTarget.target, buildTarget.server ? StandaloneBuildSubtarget.Server : StandaloneBuildSubtarget.Player);

#if SIMULATEASSETBUNDLECREATION
                //What is that for ?
                //yield return new EditorWaitForSeconds(4.5f);
                yield return new EditorWaitForSeconds(6f);
#endif
                Progress.Finish(buildTaskProgressID, Progress.Status.Succeeded);
            }
        }

        //build manifests
        foreach (var environment in _setup.environmentList)
        {
            string environementmanifestfile = null;
            if (environment.rebuild)
            {
                SceneManifest manifest = new SceneManifest(environment.scenes);
                environementmanifestfile = GetManifestFilePath(environment.id);
                string environementmanifestfilepath = Path.GetDirectoryName(environementmanifestfile);
                if (!Directory.Exists(environementmanifestfilepath))
                {
                    Directory.CreateDirectory(environementmanifestfilepath);
                }
                File.WriteAllText(environementmanifestfile, manifest.ToJson());
            }
            PreparePublishToS3(environment.id, environment.rebuild, environementmanifestfile);
        }

        UnityEngine.Debug.Log("Assetbundle build time=" + (DateTime.Now.Subtract(startTime).TotalSeconds));

        //Meant to save logs in a file, as switching plateform clears the console
        //logs += "\n" + "\n" + DateTime.Now + ": " + "Assetbundle build time=" + (DateTime.Now.Subtract(startTime).TotalSeconds);
        //File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) +"/bundleBuildLogs.txt", logs);

        if (EditorUserBuildSettings.activeBuildTarget != originalTarget || EditorUserBuildSettings.standaloneBuildSubtarget != originalSubTarget)
        {
            //Made direct to avoid having to add a wait forsecond (is this why it's there ?)
            EditorUserBuildSettings.SwitchActiveBuildTarget/*Async*/(GetTargetGroupForTarget(originalTarget), originalTarget);
        }
        Progress.Finish(buildAllProgressID, Progress.Status.Succeeded);

        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);

        yield return null;
    }

    private static void BuildAssetBundlesForTarget(string environmentid, string tmpDirectory, BuildTarget target, StandaloneBuildSubtarget subTarget)
    {
        if (Directory.Exists(tmpDirectory))
            Directory.Delete(tmpDirectory, true);
        if (!Directory.Exists(tmpDirectory))
            Directory.CreateDirectory(tmpDirectory);
        string path = tmpDirectory + "/" + environmentid + "/" + target;
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        EditorUserBuildSettings.standaloneBuildSubtarget = subTarget;

#if SIMULATEASSETBUNDLECREATION
        File.WriteAllText(path + "/" + target.ToString() + "_" + subTarget + ".txt", target.ToString());

        //Dry Run builds are still to long for frequent testing
        //BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.DryRunBuild, target);
#else
        BuildPipeline.BuildAssetBundles(path,
                                BuildAssetBundleOptions.None,
                                target);
#endif


        string[] filePaths = Directory.GetFiles(path);
        string assetBundleDirectory = GetAssetBundlePath(environmentid, target, subTarget);

        if (!Directory.Exists(assetBundleDirectory))
            Directory.CreateDirectory(assetBundleDirectory);

        foreach (string file in filePaths)
        {
            string str = assetBundleDirectory + "/" + Path.GetFileName(file);

            if (File.Exists(str)) File.Delete(str);
            File.Move(file, str);
        }

        //Removed Directory deletion to avoid build fails due to the OS locking the files
        //Directory.Delete(tmpDirectory, true);
    }

    #endregion

    #region Folders management

    private static void PreparePublishToS3(string environment, bool rebuildenvironment, string environementmanifestfile)
    {
        string path = _setup.PublishS3Path;
        path = Path.GetFullPath(path, Path.Combine(Application.dataPath, "../"));
        path = Path.GetFullPath(path);
        if (Directory.Exists(path))
            Directory.Delete(path, true);
        Directory.CreateDirectory(path);

        string environmentpath = path + "/rlty-unity-assets/" + environment + "/v" + packageVersion;

        Directory.CreateDirectory(environmentpath);

        Debug.Log("Copying client assetbundles");

        Debug.Log("Copying WebGL client assetbundles");
        CopyToFinalDir(environment, GetWebGLAssetBundlePath(environment), environmentpath, BuildTarget.WebGL, false);

        Debug.Log("Copying linux server assetbundles");
        CopyToFinalDir(environment, GetLinuxAssetBundlePath(environment, StandaloneBuildSubtarget.Server), environmentpath, BuildTarget.StandaloneLinux64, false, StandaloneBuildSubtarget.Server);
        //Copy to WebGL folder too
        CopyToFinalDir(environment, GetWebGLAssetBundlePath(environment), environmentpath, BuildTarget.StandaloneLinux64, false, StandaloneBuildSubtarget.Server);

        Debug.Log("Copying linux client assetbundles");
        CopyToFinalDir(environment, GetLinuxAssetBundlePath(environment, StandaloneBuildSubtarget.Player), environmentpath, BuildTarget.StandaloneLinux64, false, StandaloneBuildSubtarget.Player);

        Debug.Log("Copying windows server assetbundles");
        CopyToFinalDir(environment, GetWindowsAssetBundlePath(environment, StandaloneBuildSubtarget.Server), environmentpath, BuildTarget.StandaloneWindows64, false, StandaloneBuildSubtarget.Server);

        Debug.Log("Copying windows client assetbundles");
        CopyToFinalDir(environment, GetWindowsAssetBundlePath(environment, StandaloneBuildSubtarget.Player), environmentpath, BuildTarget.StandaloneWindows64, false, StandaloneBuildSubtarget.Player);

        Debug.Log("Copying iOS client assetbundles");
        CopyToFinalDir(environment, GetiOSAssetBundlePath(environment), environmentpath, BuildTarget.iOS, false);

        Debug.Log("Copying Android client assetbundles");
        CopyToFinalDir(environment, GetAndroidAssetBundlePath(environment), environmentpath, BuildTarget.Android, false);

        //Add duplicate manifest next to the zipfile
        if (rebuildenvironment)
            File.Copy(environementmanifestfile, environmentpath + Path.GetFileName(environementmanifestfile), true);

        //copy manifest
        if (rebuildenvironment)
            File.Copy(environementmanifestfile, environmentpath + "/" + Path.GetFileName(environementmanifestfile), true);

        //Store everything in a Zip
        StoreInZip(environmentpath);

        path = Path.GetFullPath(path + "/rlty-unity-assets");
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

        //DeleteTempFolders();
    }

    private static void CopyToFinalDir(string environemnentID, string srcrdir, string assetPath, BuildTarget target, bool useSubTarget, StandaloneBuildSubtarget subTarget = StandaloneBuildSubtarget.Player)
    {
        // Create directory path and folder
        if (!Directory.Exists(srcrdir))
            return;

        string platformdependentfolder = assetPath + "/" + FolderNameFromTargetPlatform(target);

        if (useSubTarget)
            platformdependentfolder += "/" + (subTarget == StandaloneBuildSubtarget.Server ? "server" : "client");

        if (!Directory.Exists(platformdependentfolder))
            Directory.CreateDirectory(platformdependentfolder);

        // If created copy files
        if (Directory.Exists(platformdependentfolder))
        {
            foreach (string file in Directory.GetFiles(srcrdir))
            {
                //If file is not a manifest and if this file is bigger than 10Kb (default bundle created by unity)
                FileInfo fileInfo = new FileInfo(file);
                Debug.Log(file + " full name, file size " + fileInfo.Length + " bytes");

                if (!file.Contains("manifest") && fileInfo.Length > 10000)
                {
                    if (subTarget == StandaloneBuildSubtarget.Player)
                    {
                        if (File.Exists(platformdependentfolder + "/" + "client"))
                            File.Delete(platformdependentfolder + "/" + "client");

                        File.Copy(file, platformdependentfolder + "/" + "client");
                    }

                    if (subTarget == StandaloneBuildSubtarget.Server)
                    {
                        if (File.Exists(platformdependentfolder + "/" + "server"))
                            File.Delete(platformdependentfolder + "/" + "server");

                        File.Copy(file, platformdependentfolder + "/" + "server");

                        if (subTarget == StandaloneBuildSubtarget.Server)
                        {
                            string webGLPseudoServerPath = assetPath + "/" + FolderNameFromTargetPlatform(BuildTarget.WebGL) + "/server";
                            if (File.Exists(webGLPseudoServerPath))
                                File.Delete(webGLPseudoServerPath);

                            File.Copy(file, webGLPseudoServerPath);
                        }
                    }

                    //File.Copy(file, platformdependentfolder + "/" + Path.GetFileName(file));
                }
            }
        }
        else
        {
            UnityEngine.Debug.LogError(platformdependentfolder + " destination publish folder could not be created");
        }
    }

    private static void StoreInZip(string environmentPath)
    {
        string destzipfile = environmentPath + "/v" + packageVersion + ".zip";

        FolderZipper.ZipUtil.ZipFiles(environmentPath, destzipfile, null);
        UnityEngine.Debug.Log("Assets zipped to " + destzipfile);

        _pathsToDelete.Add(Path.GetFullPath(environmentPath));
    }

    //Removed Directory deletion to avoid build fails due to the OS locking the files
    private void OnDisable()
    {
        //DeleteTempFolders();
    }

    #endregion
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

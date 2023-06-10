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
using Codice.Client.BaseCommands;

//Fixes to realize
//DONE
// Remove the Server/Client subfolders from the Linux Folder
// Rename the bundles client and server for each plateform folder
// Copy the linux server bundle to the WebGL folder too
// Avoid crash when building for IOS or Android
// Show AssetBundle build setup in Asset Bundle build Editor
// Fixed ID formatting that can make CICD and local build fail
// Added automatic bundle association to avoid artists omissions

//DOING
// always build every standard bundle

//TODO
// Make sure two scenes cannot have the same ID
// Use AssetBundle Variant to
// test it on a mac

public class AssetbundleBuildEditor : EditorWindow
{
    private static AssetbundleBuildSetup _setup;
    private static List<AssetbundleBuildSetup.Environment> buildingsList;
    private static string tmpDirectoryName = "RLTYTmp2";

    private static List<PlayerTarget> _assetbundleTargets = new List<PlayerTarget>();
    private static List<string> _pathsToDelete = new List<string>();

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
        //Show Environment list in 

        if (_assetbundleTargets.Count == 0)
        {
            _assetbundleTargets.Add(new PlayerTarget { target = BuildTarget.StandaloneWindows64, server = true, headless = true });
            _assetbundleTargets.Add(new PlayerTarget { target = BuildTarget.StandaloneWindows64, server = false, headless = false });
            _assetbundleTargets.Add(new PlayerTarget { target = BuildTarget.StandaloneLinux64, server = true, headless = true });
            _assetbundleTargets.Add(new PlayerTarget { target = BuildTarget.StandaloneLinux64, server = false, headless = true });
            _assetbundleTargets.Add(new PlayerTarget { target = BuildTarget.WebGL, server = false, headless = false });
            _assetbundleTargets.Add(new PlayerTarget { target = BuildTarget.iOS, server = false, headless = false });
            _assetbundleTargets.Add(new PlayerTarget { target = BuildTarget.Android, server = false, headless = false });
        }

        FindSetup();

        _setup = (AssetbundleBuildSetup)EditorGUILayout.ObjectField("Setup", _setup, typeof(AssetbundleBuildSetup), false);
        for (int i = 0; i < _assetbundleTargets.Count; i++)
            _assetbundleTargets[i].build = GUILayout.Toggle(_assetbundleTargets[i].build, _assetbundleTargets[i].Name);

        


        if (GUILayout.Button("Build AssetBundles and S3 publication ZIP file(s)"))
        {
            List<PlayerTarget> list = new List<PlayerTarget>();

            //_pathsToDelete.Clear();

            for (int i = 0; i < _assetbundleTargets.Count; i++)
                if (_assetbundleTargets[i].build)
                    list.Add(_assetbundleTargets[i]);
            EditorCoroutineUtility.StartCoroutine(PerformBuildAssetBundles(list), this);
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

        string environmentpath = path + "/rlty-unity-assets/" + environment + "/v" + Application.version;

        Directory.CreateDirectory(environmentpath);

        //copy client assetbundles
        UnityEngine.Debug.Log("Copying client assetbundles");

        UnityEngine.Debug.Log("Copying WebGL client assetbundles");
        CopyToFinalDir(GetWebGLAssetBundlePath(environment), environmentpath, BuildTarget.WebGL, false);

        //copy linux server assetbundles
        UnityEngine.Debug.Log("Copying linux server assetbundles");
        CopyToFinalDir(GetLinuxAssetBundlePath(environment, StandaloneBuildSubtarget.Server), environmentpath, BuildTarget.StandaloneLinux64, false, StandaloneBuildSubtarget.Server);
        //Copy to WebGL folder too
        CopyToFinalDir(GetWebGLAssetBundlePath(environment), environmentpath, BuildTarget.StandaloneLinux64, false, StandaloneBuildSubtarget.Server);

        //copy linux client assetbundles
        UnityEngine.Debug.Log("Copying linux client assetbundles");
        CopyToFinalDir(GetLinuxAssetBundlePath(environment, StandaloneBuildSubtarget.Player), environmentpath, BuildTarget.StandaloneLinux64, false, StandaloneBuildSubtarget.Player);

        //copy windows server assetbundles
        UnityEngine.Debug.Log("Copying windows server assetbundles");
        CopyToFinalDir(GetWindowsAssetBundlePath(environment, StandaloneBuildSubtarget.Server), environmentpath, BuildTarget.StandaloneWindows64, false, StandaloneBuildSubtarget.Server);

        //copy windows client assetbundles
        UnityEngine.Debug.Log("Copying windows client assetbundles");
        CopyToFinalDir(GetWindowsAssetBundlePath(environment, StandaloneBuildSubtarget.Player), environmentpath, BuildTarget.StandaloneWindows64, false, StandaloneBuildSubtarget.Player);

        UnityEngine.Debug.Log("Copying Android client assetbundles");
        CopyToFinalDir(GetAndroidAssetBundlePath(environment), environmentpath, BuildTarget.Android, false);

        UnityEngine.Debug.Log("Copying iOS client assetbundles");
        CopyToFinalDir(GetiOSAssetBundlePath(environment), environmentpath, BuildTarget.iOS, false);

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

        DeleteTempFolders();
    }

    private static void CopyToFinalDir(string srcrdir, string assetPath, BuildTarget target, bool useSubTarget, StandaloneBuildSubtarget subTarget = StandaloneBuildSubtarget.Player)
    {
        // Create directory path and folder
        if (!Directory.Exists(srcrdir))
        {
            return;
        }
        string platformdependentfolder = assetPath + "/" + FolderNameFromTargetPlatform(target);
        if (useSubTarget)
        {
            platformdependentfolder += "/" + (subTarget == StandaloneBuildSubtarget.Server ? "Server" : "Client");
        }
        if (!Directory.Exists(platformdependentfolder))
        {
            Directory.CreateDirectory(platformdependentfolder);
        }

        // If created copy files
        if (Directory.Exists(platformdependentfolder))
        {
            foreach (string file in Directory.GetFiles(srcrdir))
            {
                if (!file.Contains("manifest"))
                {
                    if (subTarget == StandaloneBuildSubtarget.Player)
                    {
                        if (File.Exists(platformdependentfolder + "/" + "Client"))
                            File.Delete(platformdependentfolder + "/" + "Client");

                        File.Copy(file, platformdependentfolder + "/" + "Client");
                    }

                    if (subTarget == StandaloneBuildSubtarget.Server)
                    {
                        if (File.Exists(platformdependentfolder + "/" + "Server"))
                            File.Delete(platformdependentfolder + "/" + "Server");

                        File.Copy(file, platformdependentfolder + "/" + "Server");

                        if(subTarget == StandaloneBuildSubtarget.Server)
                        {
                            string webGLPseudoServerPath = assetPath + "/" + FolderNameFromTargetPlatform(BuildTarget.WebGL) + "/Server";
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

            //Meant to save logs in a file, as switching plateform clears the console
            //logs += "\n" + "\n" + DateTime.Now + ": " + platformdependentfolder + " destination publish folder could not be created";
        }
    }

    private static void StoreInZip(string environmentPath)
    {
        string destzipfile = environmentPath + "/.." + "/rlty-unity-assets_v" + Application.version + ".zip";
        FolderZipper.ZipUtil.ZipFiles(environmentPath, destzipfile, null);
        UnityEngine.Debug.Log("Assets zipped to " + destzipfile);

        _pathsToDelete.Add(Path.GetFullPath(environmentPath));
    }

    //Removed Directory deletion to avoid build fails due to the OS locking the files
    private void OnDisable()
    {
        DeleteTempFolders();
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

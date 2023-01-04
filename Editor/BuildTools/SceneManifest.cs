using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

namespace RLTY.Customisation
{
    [System.Serializable]
    public class SceneManifest
    {
        public List<CustomisableTypeDesc> entries = new List<CustomisableTypeDesc>();

        public void Populate(CustomisableType type, string key, string description)
        {
            CustomisableTypeDesc ct = null;
            string typeString = type.ToString();
            foreach (CustomisableTypeDesc entry in entries)
                if (entry.type == typeString)
                    ct = entry;
            if (ct == null)
            {
                ct = new CustomisableTypeDesc(typeString);
                entries.Add(ct);
            }
            foreach (KeyInfo ki in ct.list)
                if (ki.key == key)
                    return;
            ct.list.Add(new KeyInfo() { key = key, type = typeString, description = description });
        }

        [MenuItem("RLTY/DebugSceneManifest")]
        public static void DebugSceneManifest()
        {
            Customisable[] fullList = GameObject.FindObjectsOfType<Customisable>();
            SceneManifest manifest = new SceneManifest();
            foreach (Customisable c in fullList)
                manifest.Populate(c.type, c.key, c.commentary);
            Debug.Log("SceneManifest=" + JsonConvert.SerializeObject(manifest, Formatting.Indented));
        }
    }


    [System.Serializable]
    public class CustomisableTypeDesc
    {
        public string type;
        public string format;
        public List<KeyInfo> list = new List<KeyInfo>();

        public CustomisableTypeDesc(string t)
        {
            type = t;
            format = CustomisableUtility.Processors[CustomisableUtility.GetType(type)].formatInfo;
        }
    }
    [Serializable]
    public class KeyInfo
    {
        public string key;
        public string description;
        public string technicalInfo;
        public string type;
    }
}
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class Renamer : MonoBehaviour
{
    private GameObject[] toSave;

    [SerializeField, ReadOnly]
    private Transform[] toRename;

    [SerializeField]
    private int startIndex;

#if UNITY_EDITOR
    private void OnValidate()
    {
        toRename = gameObject.GetComponentsInChildren<Transform>();

        toSave = new GameObject[toRename.Length];
        for (int i = 0; i < toSave.Length; i++)
        {
            toSave[i] = toRename[i].gameObject;
        }
    }

    [Button, Tooltip("Will add _001,_022, etc at the end")]
    private void BatchRename(string newBaseName)
    {
        Undo.RecordObjects(toSave, "Batch Rename");
        int i = 0;

        foreach (Transform _object in toRename)
        {
            if (i != 0)
                _object.gameObject.name = newBaseName + "_" + (i+startIndex).ToString("000");

            i++;
        }
    }

    [Button]
    private void AddPrefix(string prefix)
    {
        Undo.RecordObjects(toSave, "Batch Rename");
        int i = 0;

        foreach (Transform _object in toRename)
        {
            if (i != 0)
                _object.gameObject.name = prefix + _object.gameObject.name;

            i++;
        }
    }

    [Button]
    private void RemoveNCharactersAtStart(int n)
    {
        Undo.RecordObjects(toSave, "Batch Rename");
        int i = 0;

        foreach (Transform _object in toRename)
        {
            if (i != 0)
                _object.gameObject.name = _object.gameObject.name.Remove(0, n);
            i++;
        }
    }
#endif
}

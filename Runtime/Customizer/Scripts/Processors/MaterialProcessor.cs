using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using RLTY;
using System.Drawing;

namespace RLTY.Customisation
{
    [AddComponentMenu("RLTY/Customisable/Processors/Material")]
    public class MaterialProcessor : Processor
    {
        #region Global Variables
        //This component should update everytime the customisable changes customisable type,
        //but OnValidate seems to miss its target, or is not suited to update other components
        [InfoBox("Reset this component if you change the Customisable's CustomisableType")]
        public List<MaterialSpecs> materialsSpecs;
        public bool modifyAllInstances;
        [SerializeField, ReadOnly, ShowIf("showUtilities")]
        private List<Material> materialInstances;

        public static List<ModifiableProperty> modifiedProperties = new List<ModifiableProperty>();
        [ReadOnly, ShowIf("showUtilities"), SerializeField]
        private List<ModifiableProperty> propertiesToModify = new List<ModifiableProperty>();

        #endregion

        #region Callbacks
#if UNITY_EDITOR
        //public void Awake()
        //{
        //    GetMaterialsProperties();
        //}

        public void Reset()
        {
            GetMaterialsProperties();
        }
#endif
        #endregion

        public override Component FindComponent(Component existingTarget)
        {
            Renderer renderer = GetComponentInChildren<Renderer>();
            Component target = null;
            if (!renderer)
            {
                if (!TryGetComponent<DecalProjector>(out DecalProjector proj))
                {
                    if (debug)
                        Debug.LogWarning("No Renderer or DecalProjector found in children" + commonWarning, this);

                    else
                        target = proj;
                }
            }
            else
                target = renderer;
            return target;
        }
        public override void Customize(Component target, RLTY.SessionInfo.KeyValueBase keyValue)
        {
            RLTY.SessionInfo.KeyValueObject o = keyValue as RLTY.SessionInfo.KeyValueObject;
            if (o!=null)
                SwapTextures((Texture)o.downloadedAsset);
            else
            {
                RLTY.SessionInfo.ColorKeyValue c= keyValue as RLTY.SessionInfo.ColorKeyValue;
                if (c!=null)
                    SwapColors(c.GetColor());
            }
        }

        #region EditorOnly Logic
        [Button("Get Materials")]
        public void GetMaterialsProperties()
        {
            materialsSpecs = new List<MaterialSpecs>();
            Renderer rdr = null;
            Material sharedMaterial;
            MaterialSpecs specs;

            if (TryGetComponent(out Renderer _rdr))
            {
                rdr = _rdr;

                if (rdr.sharedMaterials.Length > 0)
                    foreach (Material _sharedMat in rdr.sharedMaterials)
                    {
                        sharedMaterial = _sharedMat;
                        specs = new MaterialSpecs(sharedMaterial, GetComponent<Customisable>().type);
                        materialsSpecs.Add(new MaterialSpecs(sharedMaterial, GetComponent<Customisable>().type));
                    }
                else
                {
                    if (debug)
                        Debug.Log("This Renderer does not have a material, please assign one or remove this component and the customisable", this);
                }

            }

            if (TryGetComponent(out DecalProjector projector))
            {
                if (projector.material)
                    materialsSpecs.Add(new MaterialSpecs(projector.material, GetComponent<Customisable>().type));
            }
            else
            {
                if (!rdr)
                    if (debug) Debug.Log("Trying to modify on material on a GameObject with no renderer nor Decal Projector, please assign one or remove this customisable");
            }
        }

        [Button]
        public void GetPropertiesToModify()
        {
            if (materialsSpecs.Any())
                foreach (MaterialSpecs matSpecs in materialsSpecs)
                {
                    if (matSpecs.shaderProperties.Any() && matSpecs.customize)
                        foreach (ModifiableProperty property in matSpecs.shaderProperties)
                        {
                            if (!propertiesToModify.Contains(property) && property.modifyThis)
                                propertiesToModify.Add(property);
                        }
                }
        }
        #endregion

        #region Runtime Logic
        public void SwapTextures(Texture tex)
        {
            GetPropertiesToModify();

            if (modifyAllInstances)
            {
                foreach (ModifiableProperty property in propertiesToModify)
                {
                    if (!modifiedProperties.Contains(property))
                    {
                        property.mat.SetTexture(property.propertyName, tex);
                        modifiedProperties.Add(property);
                        if (debug) Debug.Log("Switched " + property.mat + "shared material property " + property.propertyName + " texture to " + tex, this);
                    }
                }
            }

            else
            {
                GetComponent<Renderer>().GetMaterials(materialInstances);

                foreach (ModifiableProperty property in propertiesToModify)
                {
                    foreach (Material mat in materialInstances)
                    {
                        if (mat.name.Contains(property.mat.name))
                        {
                            mat.SetTexture(property.propertyName, tex);
                            if (debug) Debug.Log("Switched " + property.mat + "instanced material property " + property.propertyName + " texture to " + tex, this);
                        }
                    }
                }
            }

            if (TryGetComponent(out ScaleDecalToTexture scaler))
                scaler.ResizeDecal();
        }

        public void SwapColors(Color32 color)
        {
            GetPropertiesToModify();

            if (modifyAllInstances)
            {
                foreach (ModifiableProperty property in propertiesToModify)
                {
                    if (!modifiedProperties.Contains(property))
                    {
                        property.mat.SetColor(property.propertyName, color);
                        modifiedProperties.Add(property);
                        if (debug) Debug.Log("Switched " + property.mat + "shared material property " + property.propertyName + " color to " + color, this);
                    }
                }

            }
            else
            {
                GetComponent<Renderer>().GetMaterials(materialInstances);

                foreach (ModifiableProperty property in propertiesToModify)
                {
                    foreach (Material mat in materialInstances)
                    {
                        if (mat.name.Contains(property.mat.name))
                        {
                            mat.SetColor(property.propertyName, color);
                            if (debug) Debug.Log("Switched " + property.mat + "instanced material property " + property.propertyName + " color to " + color, this);
                        }
                    }
                }
            }

        }

        //OLD VERSION
        //public void SwapColors(Color32 color)
        //{
        //            if (modifyAllInstances)
        //{
        //foreach (MaterialSpecs matSpecs in materialsSpecs)
        //{
        //    if (matSpecs.customize)
        //    {
        //        foreach (ModifiableProperty property in matSpecs.shaderProperties)
        //        {
        //            if (property.modifyThis)
        //            {
        //                matSpecs.sharedMaterial.SetColor(property.propertyName, color);
        //            }
        //        }
        //    }
        //}
        //}
        //else
        //{
        //    GetComponent<Renderer>().GetMaterials(materialInstances);
        //
        //    foreach (MaterialSpecs matSpecs in materialsSpecs)
        //    {
        //        if (matSpecs.customize)
        //        {
        //            foreach (ModifiableProperty property in matSpecs.shaderProperties)
        //            {
        //                if (property.modifyThis)
        //                {
        //                    foreach (Material instancedMat in materialInstances)
        //                    {
        //                        if (instancedMat.name.Contains(matSpecs.sharedMaterial.name))
        //                            instancedMat.SetColor(property.propertyName, color);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
    }
}
#endregion

#region Data
[System.Serializable]
public class MaterialSpecs
{
    //Only Global changes until i get a better grasp of the instancing system
    //[SerializeField]
    //private Material localMat;

    //WIP
    //Only Global changes until i get a better grasp of the instancing system
    //[SerializeField, ShowIf("customize", true)]
    //[Header("Parameters"), Tooltip("Do you want to modify all instances of this material or just the one in this GameObject ?")]
    //private bool global;

    //Doesn't Serialize, doesn't show properly, Local and GlobalKeywords are readonly structs
    //[SerializeField, ShowIf("customize", true)]
    //public LocalKeyword[] localKeyWordsFromKeywordSpace;

    //Keywords only returns the common parameters (see https://docs.unity3d.com/Packages/com.unity.shadergraph@14.0/api/UnityEditor.Rendering.BuiltIn.ShaderKeywordStrings.html)
    //[SerializeField, ShowIf("customize", true)]
    //private string[] localKeyWordsFromKeyWordSpaceStrings;
    //localKeyWordsFromKeyWordSpaceStrings = _mat.shader.keywordSpace.keywordNames;

    //Doesn't show anything
    //texturesKeywords = _mat.enabledKeywords;
    //keywordStrings = _mat.shaderKeywords;

    private CustomisableType customisableType;
    [ReadOnly]
    public Material sharedMaterial;
    [SerializeField]
    public bool customize;

    [SerializeField, ShowIf("customize", true)]
    public List<ModifiableProperty> shaderProperties;

    public MaterialSpecs(Material _sharedMat, CustomisableType _customisableType)
    {
        customisableType = _customisableType;
        sharedMaterial = _sharedMat;

        switch (_customisableType)
        {
            case CustomisableType.Texture:
                GetAllTextureProperties();
                break;
            case CustomisableType.Sprite:
                GetAllTextureProperties();
                break;
            case CustomisableType.Color:
                GetAllColorProperties();
                break;
            default:
                break;
        }
    }

    public void GetAllTextureProperties()
    {
        float nProperties = sharedMaterial.shader.GetPropertyCount();
        shaderProperties = new List<ModifiableProperty>();

        for (int i = 0; i < nProperties; i++)
        {
            if (sharedMaterial.shader.GetPropertyType(i) == ShaderPropertyType.Texture)
                shaderProperties.Add(new ModifiableProperty(sharedMaterial, i));
        }
    }

    public void GetAllColorProperties()
    {
        float nProperties = sharedMaterial.shader.GetPropertyCount();
        shaderProperties = new List<ModifiableProperty>();

        for (int i = 0; i < nProperties; i++)
        {
            if (sharedMaterial.shader.GetPropertyType(i) == ShaderPropertyType.Color)
                shaderProperties.Add(new ModifiableProperty(sharedMaterial, i));
        }
    }
}

[System.Serializable]
public class ModifiableProperty
{
    public Material mat;
    [ReadOnly]
    public string propertyName;
    public bool modifyThis;

    public ModifiableProperty(Material _mat, int propertyIndex)
    {
        mat = _mat;
        propertyName = mat.shader.GetPropertyName(propertyIndex);
    }
}
#endregion

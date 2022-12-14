using RLTY;
using RLTY.Customisation;
using UnityEngine;
using Sirenix.OdinInspector;

public class CustomisableV2 : RLTYMonoBehaviour
{
    public CustomisableType type;
    [SerializeField]
    private MonoBehaviour target;
    [SerializeField]
    private ProcessorV2 processor = new ProcessorV2();

    private string key;

    public string Key
    {
        get
        {
            return string.Empty;
        }

        set
        {
            key = Key;
        }
    }

    public override void EventHandlerRegister()
    {

    }

    public override void EventHandlerUnRegister()
    {

    }


}

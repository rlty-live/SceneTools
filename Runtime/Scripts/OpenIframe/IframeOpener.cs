using RLTY.SessionInfo;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

public class IframeOpener : MonoBehaviour
{
    public string url;
    [ReadOnly, HideIf("isValidURL")]
    public string warningMessage = "Wrong URL formatting, please correct it.";
    private bool isValidURL = false;

    private void OnValidate()
    {
        if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            isValidURL = true;
    }

    public void OpenIFrame()
    {
        SceneInteractionHandlerData.OpenIframe(url);
    }
}

using UnityEngine.Events;
using UnityEngine;

namespace RLTY.Boot
{
    public static class BootManagerHandlerData
    {
        public static event UnityAction OnSceneReadyForCustomization;

        /// <summary>
        /// Called by manager
        /// </summary>
        public static void NotifySceneReadyForCustomization() => OnSceneReadyForCustomization?.Invoke();
    }
}

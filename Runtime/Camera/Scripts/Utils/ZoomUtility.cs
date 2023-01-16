using Cinemachine;
using UnityEngine;

public static class ZoomUtility
{
    /**
    Set up the camera to position and focus on the target object
    @param cam - the camera object to set up
    @param target - the target object to focus on
    @return Vector3 - the final position of the camera
    */
    public static Vector3 CalculateCameraPosition(CinemachineVirtualCamera virtualCam, Transform target, float screenPercent = 1)
    {
        float objectHeight = 0;
        float objectWidth = 0;
        float objectDepth = 0;

        if (target.TryGetComponent(out Renderer renderer))
        {
            Bounds bounds = renderer.bounds;
            Vector3 objectSize = bounds.size;
            objectWidth = objectSize.x;
            objectHeight = objectSize.y;

            if (objectWidth > objectHeight && (objectWidth - objectHeight) >= 3)
                objectHeight = CalculateLargestDiagonal(bounds);

            objectHeight /= screenPercent;
            
            objectDepth = objectSize.z;
        }
        else if (target.TryGetComponent(out RectTransform rectTransform))
        {
            Rect rect = rectTransform.rect;
            Vector3 lossyScale = rectTransform.lossyScale;
            objectWidth = rect.width * lossyScale.x;
            objectHeight = rect.height * lossyScale.y;
            
            if (objectWidth > objectHeight && (objectWidth - objectHeight) >= 3)
                objectHeight = CalculateLargestDiagonal(rectTransform);

            objectHeight /= screenPercent;
        }

        Transform virtualCamTransform = virtualCam.transform;
        
        // Set the camera's distance to the object to the calculated distance plus half of object depth
        float theoreticalDistance = objectHeight / (2 * Mathf.Tan(virtualCam.m_Lens.FieldOfView * Mathf.Deg2Rad / 2));
        virtualCamTransform.position =
            target.position - virtualCamTransform.forward * (theoreticalDistance + (objectDepth != 0 ? objectDepth / 2 : 0));
        virtualCam.LookAt = target;
        
        return virtualCamTransform.position;
    }
    
    static float CalculateLargestDiagonal(Bounds bounds) {
        Vector3 size = bounds.size;
        return Mathf.Sqrt(Mathf.Pow(size.x, 2) + Mathf.Pow(size.y, 2) + Mathf.Pow(size.z, 2));
    }

    static float CalculateLargestDiagonal(RectTransform rectTransform)
    {
        Vector2 size = rectTransform.rect.size * rectTransform.lossyScale;
        return Mathf.Sqrt(Mathf.Pow(size.x, 2) + Mathf.Pow(size.y, 2));
    }
}
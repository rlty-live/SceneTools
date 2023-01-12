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
    public static Vector3 SetupCamera(CinemachineVirtualCamera virtualCam, Transform target, float screenPercent = 1)
    {
        float objectHeight = 0;
        float objectWidth = 0;
        float objectDepth = 0;

        if (target.TryGetComponent(out Renderer renderer))
        {
            Vector3 objectSize = renderer.bounds.size;
            objectHeight = objectSize.y;
            objectWidth = objectSize.x;
            objectDepth = objectSize.z;
        }
        else if (target.TryGetComponent(out RectTransform rectTransform))
        {
            Vector3 lossyScale = rectTransform.lossyScale;
            Rect rect = rectTransform.rect;
            objectHeight = rect.height * lossyScale.y;
            objectWidth = rect.width * lossyScale.x;
        }

        objectHeight /= screenPercent;
        
        Transform virtualCamTransform = virtualCam.transform;

        float theoricalDistance = 0;

        // Set the camera's distance to the object to the calculated distance plus half of object depth
        theoricalDistance = objectHeight / (2 * Mathf.Tan(virtualCam.m_Lens.FieldOfView * Mathf.Deg2Rad / 2));
        virtualCamTransform.position =
            target.position - virtualCamTransform.forward * (theoricalDistance + (objectDepth != 0 ? objectDepth / 2 : 0));
        virtualCam.LookAt = target;

        return virtualCamTransform.position;

        /*
        Debug.Log($"First placement {virtualCam.transform.position}");


        // Get the position of the target object
        Vector3 targetPosition = target.position;

        // Get the current position of the camera
        Vector3 camPosition = virtualCam.transform.position;

        // Move the target position closer to the camera by half of the depth of the target's renderer bounds
        targetPosition =
            Vector3.MoveTowards(targetPosition, camPosition, target.GetComponent<Renderer>().bounds.size.z / 2);

        // Get the distance between the camera and target position
        float distance = Vector3.Distance(camPosition, targetPosition);

        // Get the corners of the camera's frustum at the current distance
        Vector3[] frustumCorners =
            GetFrustumCameraAtDistance(virtualCam.m_Lens.Aspect, virtualCam.m_Lens.FieldOfView, distance);
        for (int i = 0; i < frustumCorners.Length; i++)
        {
            frustumCorners[i] = virtualCam.transform.TransformPoint(frustumCorners[i]);
        }

        // Get the corners of the target object
        Vector3[] boxCorners = GetTargetCorners(target);

        // Temporary variables to store the final position
        Vector3 finalCamPos = camPosition, tmpCamPosition = camPosition;

        // Check if the target corners are inside the camera frustum
        bool isInside = IsSquareInsideSquare(frustumCorners, boxCorners);

        // If the target is not fully inside the frustum, adjust the camera position and distance
        while (isInside)
        {
            // Move the camera towards the target
            tmpCamPosition = Vector3.MoveTowards(tmpCamPosition, targetPosition, 0.1f);
            // Update the distance between the camera and target
            float tmpDistance = Vector3.Distance(tmpCamPosition, targetPosition);

            // Update the camera frustum corners at the new distance
            frustumCorners =
                GetFrustumCameraAtDistance(virtualCam.m_Lens.Aspect, virtualCam.m_Lens.FieldOfView, tmpDistance);

            for (int i = 0; i < frustumCorners.Length; i++)
            {
                frustumCorners[i] = virtualCam.transform.TransformPoint(frustumCorners[i]);
            }

            // Check if the target is inside the frustum
            isInside = IsSquareInsideSquare(frustumCorners, boxCorners);

            // If the target is inside the frustum, update the final camera position
            if (isInside)
                finalCamPos = tmpCamPosition;
        }

        if (IsSquareInsideSquare(boxCorners, frustumCorners))
        {
            virtualCam.transform.position += -virtualCam.transform.forward * (target.lossyScale.z / 2);
            finalCamPos = virtualCam.transform.position;
            Debug.Log($"Second placement {virtualCam.transform.position}");
        }

        return finalCamPos;
        
        */
    }

    /**
    * Get the frustum corners of a camera at a specific distance
    * @param aspectRatio - the aspect ratio of the camera
    * @param fieldOfView - the field of view of the camera in degrees
    * @param distance - the distance from the camera to calculate the corners
    * @return Vector3[] - the array of 4 corners of the frustum
    */
    public static Vector3[] GetFrustumCameraAtDistance(float aspectRatio, float fieldOfView, float distance)
    {
        Vector3[] frustumCorners = new Vector3[4];

        // Convert the field of view to radians
        float fieldOfViewRad = Mathf.Deg2Rad * fieldOfView;

        // Calculate the height of the camera based on the distance and field of view
        float cameraHeight = 2.0f * distance * Mathf.Tan(fieldOfViewRad / 2.0f);
        // Calculate the width of the camera based on the aspect ratio
        float cameraWidth = cameraHeight * aspectRatio;

        // Calculate the frustum corners
        frustumCorners[0] = new Vector3(-cameraWidth / 2.0f, cameraHeight / 2.0f, distance);
        frustumCorners[1] = new Vector3(cameraWidth / 2.0f, cameraHeight / 2.0f, distance);
        frustumCorners[2] = new Vector3(cameraWidth / 2.0f, -cameraHeight / 2.0f, distance);
        frustumCorners[3] = new Vector3(-cameraWidth / 2.0f, -cameraHeight / 2.0f, distance);

        // Return the array of frustum corners
        return frustumCorners;
    }

    /**
     * Get the corners of the target object
     * @param target - the object to get the corners
     * @return Vector3[] - the array of 4 corners of the target object
     */
    public static Vector3[] GetTargetCorners(Transform target)
    {
        Vector3[] boxCorners = new Vector3[4];

        // Check if the target has a renderer component
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer)
        {
            // If it does, get the bounds of the renderer
            Bounds bounds = renderer.bounds;

            // Get the corners of the 3D target
            boxCorners = Get3DTargetCorners(bounds);
        }
        else if (target.TryGetComponent(out RectTransform targetRectTransform))
        {
            // If it doesn't have a renderer, check if it has a RectTransform component
            Rect targetRect = targetRectTransform.rect;

            // Get the corners of the UI target
            boxCorners = GetUITargetCorners(targetRect);
        }
        else
        {
            // If it doesn't have a renderer or RectTransform, raise a warning
            Debug.LogWarning(
                $"CalculateObjectBox for Target {target.gameObject.name} no Renderer or RectTransform found", target);
            return boxCorners;
        }

        // Scale the corners by the inverse lossy scale
        Vector3 lossyScale = target.lossyScale;
        Vector3 invLossyScale = new Vector3(1 / lossyScale.x, 1 / lossyScale.y, 1 / lossyScale.z);

        for (int i = 0; i < boxCorners.Length; i++)
        {
            // Scale the corners and transform them to world space
            boxCorners[i] = Vector3.Scale(boxCorners[i], invLossyScale);
            boxCorners[i] = target.TransformPoint(boxCorners[i]);
        }

        return boxCorners;
    }

    /**
     * Get the corners of a 3D target
     * @param targetBounds - the bounds of the target object
     * @return Vector3[] - the array of 4 corners of the target object
     */
    private static Vector3[] Get3DTargetCorners(Bounds targetBounds)
    {
        Vector3[] boxCorners = new Vector3[4];

        // Get the size of the bounds
        float boxWidth = targetBounds.size.x;
        float boxHeight = targetBounds.size.y;
        float boxDepth = targetBounds.size.z;

        // Create the 4 corners of the bounds
        Vector3 topLeft = new Vector3(-boxWidth / 2.0f, boxHeight / 2.0f, -boxDepth / 2);
        Vector3 topRight = new Vector3(boxWidth / 2.0f, boxHeight / 2.0f, -boxDepth / 2);
        Vector3 bottomRight = new Vector3(boxWidth / 2.0f, -boxHeight / 2.0f, -boxDepth / 2);
        Vector3 bottomLeft = new Vector3(-boxWidth / 2.0f, -boxHeight / 2.0f, -boxDepth / 2);

        // Add the corners to the array
        boxCorners[0] = topLeft;
        boxCorners[1] = topRight;
        boxCorners[2] = bottomRight;
        boxCorners[3] = bottomLeft;

        return boxCorners;
    }

    /**
     * Get the corners of a UI target
     * @param targetRect - the rect of the UI target
     * @return Vector3[] - the array of 4 corners of the UI target
     */
    private static Vector3[] GetUITargetCorners(Rect targetRect)
    {
        Vector3[] boxCorners = new Vector3[4];

        // Get the size of the rect
        float rectWidth = targetRect.width;
        float rectHeight = targetRect.height;

        // Create the 4 corners of the rect
        Vector3 topLeft = new Vector3(-rectWidth / 2.0f, rectHeight / 2.0f, 0);
        Vector3 topRight = new Vector3(rectWidth / 2.0f, rectHeight / 2.0f, 0);
        Vector3 bottomRight = new Vector3(rectWidth / 2.0f, -rectHeight / 2.0f, 0);
        Vector3 bottomLeft = new Vector3(-rectWidth / 2.0f, -rectHeight / 2.0f, 0);

        // Add the corners to the array
        boxCorners[0] = topLeft;
        boxCorners[1] = topRight;
        boxCorners[2] = bottomRight;
        boxCorners[3] = bottomLeft;

        return boxCorners;
    }

    private static float Get3DObjectHeight(Bounds targetBounds) => targetBounds.size.y;
    private static float Get3DObjectWidth(Bounds targetBounds) => targetBounds.size.x;

    private static float GetUIObjectHeight(Rect targetRect) => targetRect.height;
    private static float GetUIObjectWidth(Rect targetRect) => targetRect.width;

    /**
    * Check if the inner square is fully inside the outer square
    * @param outerSquare - the outer square represented by an array of 4 Vector3
    * @param innerSquare - the inner square represented by an array of 4 Vector3
    * @return bool - true if the inner square is fully inside the outer square, false otherwise
    */
    private static bool IsSquareInsideSquare(Vector3[] outerSquare, Vector3[] innerSquare)
    {
        // Iterate through each point of the inner square
        foreach (Vector3 innerPoint in innerSquare)
        {
            // Check if the inner point is within the x and y range of the outer square
            if (innerPoint.x < outerSquare[0].x || innerPoint.x > outerSquare[1].x || innerPoint.y > outerSquare[0].y ||
                innerPoint.y < outerSquare[2].y)
            {
                // If the inner point is not within the x and y range of the outer square, the inner square is not fully inside the outer square
                return false;
            }
        }

        // If all points of the inner square are within the x and y range of the outer square, the inner square is fully inside the outer square
        return true;
    }
}
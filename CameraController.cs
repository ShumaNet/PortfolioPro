using UnityEngine;

public class CameraController : MonoBehaviour
{
    [System.Serializable]
    public class CameraSettings
    {
        public Transform playerTarget;          
        public float distanceFromPlayer = 7f;   
        public Vector3 cameraOffset = new Vector3(0,1.5f,0);

        public float minVerticalAngle = -50f;
        public float maxVerticalAngle = 80f;
    }

    [System.Serializable]
    public class ZoomSettings
    {
        public float minZoomDistance = 2f;
        public float maxZoomDistance = 10f;
        public float zoomSpeed = 2f;
        public float zoomSmoothness = 5f;

        public float currentZoomDistance = 5;
    }

    [System.Serializable]
    public class MouseSettings
    {
        public float sensitivity = 2f;
        public bool invertY = false;
        public bool invertX = false;
    }

    [System.Serializable]
    public class CollisionSettings
    {
        public LayerMask collisionMask;
        public float cameraRadius = 0.3f;
        public float collisionOffset = 0.2f;
        public float smoothTime = 0.1f;
        public float minCameraDistance = 0.5f;
    }

    [Header("Основные настройки камеры")]
    [SerializeField]CameraSettings cameraSettings = new CameraSettings();

    [Header("Настройки зума")]
    [SerializeField] ZoomSettings zoomSettings = new ZoomSettings();

    [Header("Mouse Settings")]
    [SerializeField] MouseSettings mouseSettings = new MouseSettings();

    [Header("Collision Setting")]
    [SerializeField] CollisionSettings collisionSettings = new CollisionSettings();

    float distanceCameraFromCharacter;
    Vector2 currentRotation;
    float desiredZoomDistance;
    

    void Start()
    {
        InitializeCamera();

    }

    // Update is called once per frame
    void Update()
    {
        HandleZoom();
    }

    void LateUpdate()
    {
        if (cameraSettings.playerTarget == null) return;


        HandleMouseInput();

        Vector3 cameraPosition = CalculateCameraPosition();
        transform.position = cameraPosition;
        transform.LookAt(cameraSettings.playerTarget.position + cameraSettings.cameraOffset);
    }


    void InitializeCamera()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        distanceCameraFromCharacter = zoomSettings.currentZoomDistance;
        desiredZoomDistance = zoomSettings.currentZoomDistance;

        currentRotation = new Vector2(transform.eulerAngles.x, transform.eulerAngles.y);

        if(collisionSettings.collisionMask == 0)
            collisionSettings.collisionMask = ~(1 << LayerMask.NameToLayer("Ignore Raycast"));
    }

    void HandleMouseInput()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSettings.sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSettings.sensitivity;

        if (mouseSettings.invertX) mouseX = -mouseX;
        if (mouseSettings.invertY) mouseY = -mouseY;

        currentRotation.x -= mouseY;
        currentRotation.y -= mouseX;

        currentRotation.x = Mathf.Clamp(currentRotation.x, cameraSettings.minVerticalAngle, cameraSettings.maxVerticalAngle);
    }

    void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput != 0)
        {
            desiredZoomDistance -= scrollInput * zoomSettings.zoomSpeed;
            desiredZoomDistance = Mathf.Clamp(desiredZoomDistance, zoomSettings.minZoomDistance, zoomSettings.maxZoomDistance);
        }

        // Используем более плавную интерполяцию
        zoomSettings.currentZoomDistance = Mathf.Lerp(zoomSettings.currentZoomDistance, desiredZoomDistance, Time.deltaTime * zoomSettings.zoomSmoothness);

        // Синхронизируем переменные
        distanceCameraFromCharacter = zoomSettings.currentZoomDistance;
    }


    Vector3 CalculateCameraPosition()
    {
        Vector3 desiredPosition = GetDesiredCameraPosition();

        HandleCameraCollision(ref desiredPosition);
        return desiredPosition;
    }

    private Vector3 GetDesiredCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0);

        Vector3 desiredPosition = cameraSettings.playerTarget.position + cameraSettings.cameraOffset - (rotation * Vector3.forward * zoomSettings.currentZoomDistance);

        return desiredPosition;
    }

    void HandleCameraCollision(ref Vector3 desiredPosition)
    {
        Vector3 rayStart = cameraSettings.playerTarget.position + cameraSettings.cameraOffset;
        Vector3 directionToCamera = desiredPosition - rayStart;
        float targetDistance = directionToCamera.magnitude;
        Vector3 rayDirection = directionToCamera.normalized;

        RaycastHit hit;
        if (Physics.SphereCast(rayStart, collisionSettings.cameraRadius, rayDirection, out hit, targetDistance, collisionSettings.collisionMask))
        {
            float desiredDistance = Mathf.Max(hit.distance - collisionSettings.collisionOffset, collisionSettings.minCameraDistance);
            distanceCameraFromCharacter = Mathf.Lerp(distanceCameraFromCharacter, desiredDistance, collisionSettings.smoothTime * Time.deltaTime);
        }
        else
            distanceCameraFromCharacter = Mathf.Lerp(distanceCameraFromCharacter, zoomSettings.currentZoomDistance, collisionSettings.smoothTime * Time.deltaTime);
        

        Quaternion rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0);
        desiredPosition = rayStart - (rotation * Vector3.forward * distanceCameraFromCharacter);

        // Ограничение по высоте (опционально)
        float minHeight = cameraSettings.playerTarget.position.y + 0.5f;
        if (desiredPosition.y < minHeight)
            desiredPosition.y = minHeight;
    }

    Vector3 GetLookAtPosition()
    {
        return cameraSettings.playerTarget.position + cameraSettings.cameraOffset;
    }


    public void ResetCameraRotation()
    {
        currentRotation = Vector3.zero;
    }

    public void SetCameraRotation(Vector3 newRotation)
    { 
     currentRotation = newRotation;
    }

    public Vector3 GetCurrentRotation()
    {
        return currentRotation;
    }
}

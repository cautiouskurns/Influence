using UnityEngine;
using UnityEngine.EventSystems;

namespace Managers
{
    /// <summary>
    /// Controls camera movement, zooming, and positioning for the map.
    /// Allows panning with middle mouse button or touch drag, and zooming with scroll wheel or pinch.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        #region Singleton
        private static CameraController _instance;
        
        public static CameraController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<CameraController>();
                    
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("CameraController");
                        _instance = go.AddComponent<CameraController>();
                    }
                }
                
                return _instance;
            }
        }
        #endregion

        [Header("References")]
        [SerializeField] private Camera mainCamera;
        
        [Header("Movement Settings")]
        [SerializeField] private float panSpeed = 20f;
        [SerializeField] private float dragPanSpeed = 2f;
        [SerializeField] private float edgePanThreshold = 20f; // Distance from edge to trigger edge panning
        [SerializeField] private bool enableEdgePanning = true;
        [SerializeField] private bool invertDrag = false;
        
        [Header("Zoom Settings")]
        [SerializeField] private float zoomSpeed = 10f;
        [SerializeField] private float minZoom = 2f;
        [SerializeField] private float maxZoom = 15f;
        [SerializeField] private float zoomDampening = 5f;
        
        [Header("Boundaries")]
        [SerializeField] private bool enableBoundaries = true;
        [SerializeField] private float boundaryPadding = 2f;
        [SerializeField] private Vector2 minBoundary = new Vector2(-20f, -20f);
        [SerializeField] private Vector2 maxBoundary = new Vector2(20f, 20f);
        
        [Header("Input Options")]
        [SerializeField] private bool useMiddleMouseToPan = true;
        [SerializeField] private bool useRightMouseToPan = false;
        
        // Private tracking variables
        private Vector3 targetPosition;
        private float targetZoom;
        private Vector3 lastMousePosition;
        private bool isDragging = false;
        private float currentZoom;
        
        private void Awake()
        {
            // Auto-assign camera if not set
            if (mainCamera == null)
            {
                mainCamera = GetComponent<Camera>();
                
                if (mainCamera == null)
                {
                    mainCamera = Camera.main;
                    
                    if (mainCamera == null)
                    {
                        Debug.LogError("CameraController: No camera found. Please assign a camera in the inspector or make sure there's a Main Camera in the scene.");
                    }
                }
            }
            
            // Initialize target position and zoom
            targetPosition = transform.position;
            currentZoom = mainCamera.orthographicSize;
            targetZoom = currentZoom;
        }
        
        private void Start()
        {
            // Set initial camera properties
            if (mainCamera != null && !mainCamera.orthographic)
            {
                Debug.LogWarning("CameraController: Camera is not orthographic. This controller is designed for orthographic cameras.");
                mainCamera.orthographic = true;
            }
        }
        
        private void Update()
        {
            // Skip camera control if pointer is over UI element
            if (IsPointerOverUI())
            {
                return;
            }
            
            HandleKeyboardInput();
            HandleMouseInput();
            
            // Apply movement and zoom with smoothing
            ApplyCameraTransforms();
        }
        
        private void HandleKeyboardInput()
        {
            // WASD or arrow key movement
            Vector3 moveInput = Vector3.zero;
            
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                moveInput.y += 1;
            
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                moveInput.y -= 1;
            
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                moveInput.x -= 1;
            
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                moveInput.x += 1;
            
            if (moveInput != Vector3.zero)
            {
                moveInput = moveInput.normalized;
                targetPosition += moveInput * panSpeed * Time.deltaTime * (currentZoom / 5f);
            }
            
            // Handle edge panning
            if (enableEdgePanning)
            {
                HandleEdgePanning();
            }
        }
        
        private void HandleEdgePanning()
        {
            Vector3 moveInput = Vector3.zero;
            
            if (Input.mousePosition.x < edgePanThreshold)
                moveInput.x -= 1;
            
            if (Input.mousePosition.x > Screen.width - edgePanThreshold)
                moveInput.x += 1;
            
            if (Input.mousePosition.y < edgePanThreshold)
                moveInput.y -= 1;
            
            if (Input.mousePosition.y > Screen.height - edgePanThreshold)
                moveInput.y += 1;
            
            if (moveInput != Vector3.zero)
            {
                moveInput = moveInput.normalized;
                targetPosition += moveInput * panSpeed * Time.deltaTime * (currentZoom / 5f);
            }
        }
        
        private void HandleMouseInput()
        {
            // Mouse zoom
            float scrollDelta = Input.mouseScrollDelta.y;
            if (scrollDelta != 0)
            {
                targetZoom -= scrollDelta * (zoomSpeed * 0.1f);
                targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            }
            
            // Middle mouse drag
            bool middleMouseButton = Input.GetMouseButton(2) && useMiddleMouseToPan;
            bool rightMouseButton = Input.GetMouseButton(1) && useRightMouseToPan;
            bool shouldDrag = middleMouseButton || rightMouseButton;
            
            if (Input.GetMouseButtonDown(2) || (useRightMouseToPan && Input.GetMouseButtonDown(1)))
            {
                lastMousePosition = Input.mousePosition;
                isDragging = true;
            }
            
            if (isDragging && shouldDrag)
            {
                Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
                
                if (invertDrag)
                    mouseDelta = -mouseDelta;
                
                targetPosition -= new Vector3(
                    mouseDelta.x * dragPanSpeed * (currentZoom / 10f) * Time.deltaTime,
                    mouseDelta.y * dragPanSpeed * (currentZoom / 10f) * Time.deltaTime,
                    0
                );
                
                lastMousePosition = Input.mousePosition;
            }
            
            if (Input.GetMouseButtonUp(2) || (useRightMouseToPan && Input.GetMouseButtonUp(1)))
            {
                isDragging = false;
            }
        }
        
        private void ApplyCameraTransforms()
        {
            // Apply boundaries to target position
            if (enableBoundaries)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, minBoundary.x, maxBoundary.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, minBoundary.y, maxBoundary.y);
            }
            
            // Smooth movement
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);
            
            // Smooth zoom
            currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * zoomDampening);
            mainCamera.orthographicSize = currentZoom;
        }
        
        // Helper to check if pointer is over UI element
        private bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
        
        // Public methods to control camera
        public void SetPosition(Vector3 position)
        {
            targetPosition = position;
            transform.position = position;
        }
        
        public void SetZoom(float zoom)
        {
            targetZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
            currentZoom = targetZoom;
            mainCamera.orthographicSize = currentZoom;
        }
        
        public void ZoomInAtPosition(Vector3 targetPosition, float zoomAmount)
        {
            // Store current position and zoom
            Vector3 startPosition = transform.position;
            float startZoom = currentZoom;
            
            // Calculate new zoom level
            float newZoom = Mathf.Clamp(startZoom - zoomAmount, minZoom, maxZoom);
            
            // If zooming in, adjust position to zoom towards mouse position
            if (newZoom < startZoom)
            {
                float zoomFactor = 1.0f - (newZoom / startZoom);
                Vector3 offsetToTarget = targetPosition - startPosition;
                transform.position += offsetToTarget * zoomFactor;
                targetPosition = transform.position;
            }
            
            targetZoom = newZoom;
        }
        
        // Focus on a specific world position with custom zoom level
        public void FocusOn(Vector3 position, float zoom = -1)
        {
            targetPosition = position;
            
            if (zoom > 0)
            {
                targetZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
            }
        }
        
        // Auto-fit the camera to see the entire map
        public void FitToBounds(Bounds bounds)
        {
            // Calculate the size needed to fit the bounds
            float horizontal = bounds.size.x * 0.5f;
            float vertical = bounds.size.y * 0.5f;
            
            // Calculate the required orthographic size
            float newZoom = Mathf.Max(horizontal / mainCamera.aspect, vertical) + boundaryPadding;
            
            // Apply position and zoom
            targetPosition = bounds.center;
            targetZoom = Mathf.Clamp(newZoom, minZoom, maxZoom);
        }
        
        // Set boundaries based on the map size
        public void SetBoundaries(Vector2 min, Vector2 max)
        {
            minBoundary = min;
            maxBoundary = max;
        }
    }
}
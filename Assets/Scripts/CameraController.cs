using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public float normalOrthographicSize = 5f;    // Normal gameplay zoom
    public float bossFightOrthographicSize = 5.5f;  // Boss fight zoom (see more)
    public Vector3 normalPosition = new Vector3(0, 0, -10);
    public Vector3 bossFightPosition = new Vector3(0, 0, -10);

    [Header("Transition")]
    public float transitionSpeed = 2f;

    private Camera cam;
    private float targetSize;
    private Vector3 targetPosition;
    private bool isTransitioning = false;

    public static CameraController Instance;

    void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();
        
        if (!cam)
        {
            Debug.LogError("[CameraController] No Camera component found!");
            return;
        }

        // Set initial values
        targetSize = normalOrthographicSize;
        targetPosition = normalPosition;
        cam.orthographicSize = normalOrthographicSize;
        transform.position = normalPosition;
    }

    void Update()
    {
        if (!cam || !isTransitioning) return;

        // Smoothly transition camera
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.deltaTime * transitionSpeed);
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * transitionSpeed);

        // Check if transition complete
        if (Mathf.Abs(cam.orthographicSize - targetSize) < 0.01f &&
            Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            cam.orthographicSize = targetSize;
            transform.position = targetPosition;
            isTransitioning = false;
        }
    }

    public void SetNormalView()
    {
        targetSize = normalOrthographicSize;
        targetPosition = normalPosition;
        isTransitioning = true;
        Debug.Log("[CameraController] Transitioning to normal view");
    }

    public void SetBossFightView()
    {
        targetSize = bossFightOrthographicSize;
        targetPosition = bossFightPosition;
        isTransitioning = true;
        Debug.Log("[CameraController] Transitioning to boss fight view");
    }

    public void SetInstantView(float size, Vector3 position)
    {
        if (!cam) return;
        
        cam.orthographicSize = size;
        transform.position = position;
        targetSize = size;
        targetPosition = position;
        isTransitioning = false;
        
        Debug.Log($"[CameraController] Set instant view: size={size}, pos={position}");
    }
}
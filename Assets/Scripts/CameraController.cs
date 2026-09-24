using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Movement")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float smoothTime = 0.15f;

    [Header("Mouse Edge Scrolling")]
    [SerializeField] private bool useMouseEdgeScrolling = true;
    [SerializeField] private float edgeSize = 20f;

    [Header("Camera Bounds")]
    [SerializeField] private bool useBounds = true;
    [SerializeField] private Vector2 minBounds = new Vector2(-50f, -50f);
    [SerializeField] private Vector2 maxBounds = new Vector2(50f, 50f);

    private Vector3 targetPosition;
    private Vector3 velocity;

    private void Start()
    {
        targetPosition = transform.position;
    }

    private void Update()
    {
        HandleCameraMovement();
        ClampCameraPosition();
        SmoothMove();
    }

    private void HandleCameraMovement()
    {
        Vector3 moveDirection = Vector3.zero;

        // WASD / 方向键
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        moveDirection += transform.right * horizontal;
        moveDirection += transform.forward * vertical;

        // 鼠标屏幕边缘移动
        if (useMouseEdgeScrolling)
        {
            Vector3 mousePosition = Input.mousePosition;

            if (mousePosition.x <= edgeSize)
            {
                moveDirection += -transform.right;
            }
            else if (mousePosition.x >= Screen.width - edgeSize)
            {
                moveDirection += transform.right;
            }

            if (mousePosition.y <= edgeSize)
            {
                moveDirection += -transform.forward;
            }
            else if (mousePosition.y >= Screen.height - edgeSize)
            {
                moveDirection += transform.forward;
            }
        }

        // 防止斜向移动速度变快
        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        // 只在 XZ 平面移动
        moveDirection.y = 0f;

        targetPosition += moveDirection * moveSpeed * Time.deltaTime;
    }

    private void SmoothMove()
    {
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }

    private void ClampCameraPosition()
    {
        if (!useBounds)
            return;

        targetPosition.x = Mathf.Clamp(
            targetPosition.x,
            minBounds.x,
            maxBounds.x
        );

        targetPosition.z = Mathf.Clamp(
            targetPosition.z,
            minBounds.y,
            maxBounds.y
        );
    }
}
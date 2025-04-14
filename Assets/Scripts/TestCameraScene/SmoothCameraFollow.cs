using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;

    [Header("Position Settings")]
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private Vector2 offset = new Vector2(0f, 1f);

    [Header("Boundaries")]
    [SerializeField] private bool useBoundaries = false;
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;
    [SerializeField] private float minY = -10f;
    [SerializeField] private float maxY = 10f;

    [Header("Look Ahead")]
    [SerializeField] private bool useLookAhead = true;
    [SerializeField] private float lookAheadFactor = 2f;
    [SerializeField] private float lookAheadReturnSpeed = 2f;
    [SerializeField] private float lookAheadMoveThreshold = 0.1f;

    // Internal tracking variables
    private Vector3 currentVelocity;
    private float targetLookAheadX;
    private float currentLookAheadX;
    private float lastTargetPositionX;

    private void Start()
    {
        // If no target is set, try to find the player
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
            else
                Debug.LogWarning("No target set for camera and no GameObject with 'Player' tag found!");
        }

        lastTargetPositionX = target != null ? target.position.x : 0f;
        currentLookAheadX = 0f;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        // Calculate look-ahead offset
        float lookAheadDirX = 0;

        if (useLookAhead)
        {
            // Calculate direction to look ahead based on player movement
            float targetMoveDelta = target.position.x - lastTargetPositionX;
            lastTargetPositionX = target.position.x;

            if (Mathf.Abs(targetMoveDelta) > lookAheadMoveThreshold)
            {
                targetLookAheadX = lookAheadFactor * Mathf.Sign(targetMoveDelta);
            }
            else
            {
                targetLookAheadX = Mathf.MoveTowards(targetLookAheadX, 0, lookAheadReturnSpeed * Time.deltaTime);
            }

            currentLookAheadX = Mathf.MoveTowards(currentLookAheadX, targetLookAheadX, Time.deltaTime * lookAheadReturnSpeed);
            lookAheadDirX = currentLookAheadX;
        }

        // Calculate target position with offset and look-ahead
        Vector3 targetPosition = new Vector3(
            target.position.x + offset.x + lookAheadDirX,
            offset.y,  // Keep Y position fixed for consistent world curvature
            transform.position.z
        );

        // Apply boundaries if enabled
        if (useBoundaries)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
        }

        // Smoothly move camera toward target position
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocity,
            1 / followSpeed
        );
    }
}
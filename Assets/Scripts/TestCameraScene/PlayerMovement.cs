using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deceleration = 50f;
    [SerializeField] private float maxSpeed = 8f;

    // References
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // Movement variables
    private float horizontalInput;
    private float currentSpeed;

    private void Awake()
    {
        // Get component references
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Make sure we have a Rigidbody2D
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1f;
            rb.freezeRotation = true;
        }
    }

    private void Update()
    {
        // Get input from A and D keys
        horizontalInput = 0;
        if (Input.GetKey(KeyCode.A))
            horizontalInput -= 1;
        if (Input.GetKey(KeyCode.D))
            horizontalInput += 1;

        // Flip sprite based on movement direction
        if (horizontalInput < 0)
            spriteRenderer.flipX = true;
        else if (horizontalInput > 0)
            spriteRenderer.flipX = false;
    }

    private void FixedUpdate()
    {
        // Handle movement physics in FixedUpdate
        MovePlayer();
    }

    private void MovePlayer()
    {
        // Get current velocity
        Vector2 velocity = rb.linearVelocity;

        // Apply acceleration or deceleration
        if (horizontalInput != 0)
        {
            // Accelerate
            currentSpeed = Mathf.MoveTowards(currentSpeed, moveSpeed * horizontalInput, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            // Decelerate
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.fixedDeltaTime);
        }

        // Clamp speed
        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);

        // Apply velocity
        velocity.x = currentSpeed;
        rb.linearVelocity = velocity;
    }
}
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 3f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rigidBody;

    private ContactFilter2D contactFilter;
    [SerializeField] private Vector2 velocity;

    private bool isGrounded = false;
    private float minGroundDistance = 0.65f;
    private float minMoveDistance = 0.001f;

    // 초기화
    private void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidBody = GetComponent<Rigidbody2D>();

        contactFilter.useTriggers = false;
    }

    // 매 프레임 마다 실행되는 함수
    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        transform.position += 
            new Vector3(horizontal, 0f, 0f) * moveSpeed * Time.deltaTime;

        bool isRunning = horizontal != 0f;

        if (isRunning)
        {
            spriteRenderer.flipX = horizontal < 0f;
        }

        animator.SetBool("Running", isRunning);
    }

    private void FixedUpdate()
    {
        isGrounded = false;

        velocity += Physics2D.gravity * Time.deltaTime;

        var deltaPosition = velocity * Time.deltaTime;
        var move = Vector2.up * deltaPosition.y;

        Movement(move);

        animator.SetBool("IsGrounded", isGrounded);
    }

    private void Movement(Vector2 move)
    {
        var distance = move.magnitude;

        if (distance > minMoveDistance)
        {
            // 지면 체크
            RaycastHit2D[] hitBuffer = new RaycastHit2D[10];
            int hitCount = rigidBody.Cast(move, contactFilter, hitBuffer, distance);

            for (int i = 0; i < hitCount; i++)
            {
                float hitDistance = hitBuffer[i].distance;

                if (hitDistance < minGroundDistance)
                {
                    isGrounded = true;
                }

                if (isGrounded)
                {
                    velocity.y = 0f;
                }
                else // 공중에 있는 상태
                {
                    velocity.x *= 0;
                    velocity.y = Mathf.Min(velocity.y, 0f);
                }

                distance = Mathf.Min(hitDistance, distance);
            }

            rigidBody.position += move.normalized * distance;
        }
    }

    public void Death()
    {
        animator.SetTrigger("Death");
    }
}

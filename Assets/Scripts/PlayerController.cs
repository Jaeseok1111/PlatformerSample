using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public Transform _spawnPoint;
    public float moveSpeed;
    public float jumpPower;

    public Vector2 velocity;
    public bool isGrounded;

    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rigidBody;
    private Animator _animator;

    private ContactFilter2D contactFilter;
    private RaycastHit2D[] hitBuffer = new RaycastHit2D[16];

    private const float minMoveDistance = 0.001f;
    private const float minGroundNormalY = 0.65f;
    private const float shellRadius = 0.01f;

    private bool _control = true;

    public Bounds Bounds => GetComponent<Collider2D>().bounds;

    public void Hurt()
    {
        _animator.SetTrigger("hurt");
    }

    public void Die()
    {
        velocity = Vector2.zero;

        _control = false;
        _animator.SetBool("dead", true);
        _animator.SetTrigger("hurt");
    }

    public void Victory()
    {
        velocity = Vector2.zero;
        _control = false;
        _animator.SetTrigger("victory");
    }

    public void Spawn()
    {
        _animator.SetTrigger("spawn");
        _animator.SetBool("dead", false);
        _rigidBody.position = _spawnPoint.position;

        var virtualCamera = FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
        virtualCamera.Follow = transform;
        virtualCamera.LookAt = transform;

        StartCoroutine(SetEnableControl());
    }

    private IEnumerator SetEnableControl()
    {
        yield return new WaitForSeconds(0.3f);

        _control = true;
    }

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rigidBody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();

        contactFilter.useTriggers = false;
        contactFilter.useLayerMask = true;
        contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));

        Spawn();
    }

    private void Update()
    {
        if (_control == false) return;

        velocity.x = Input.GetAxis("Horizontal") * moveSpeed;
        
        if (velocity.x != 0.0f)
        {
            if (velocity.x < 0.01f)
                _spriteRenderer.flipX = true;
            else
                _spriteRenderer.flipX = false;
        }

        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            velocity.y = jumpPower;
        }
    }

    private void FixedUpdate()
    {
        isGrounded = false;

        velocity += Physics2D.gravity * Time.deltaTime;

        var deltaPosition = velocity * Time.deltaTime;
        var move = new Vector2(deltaPosition.x, 0.0f);

        PerformMovement(move);

        move = Vector2.up * deltaPosition.y;

        PerformMovement(move);

        _animator.SetFloat("velocityX", Mathf.Abs(velocity.x));
        _animator.SetBool("grounded", isGrounded);
    }

    private void PerformMovement(Vector2 move)
    {
        // 이동 거리를 계산합니다.
        var distance = move.magnitude;

        // 이동 거리가 최소 거리보다 클 때만 충돌 체크를 합니다.
        if (distance > minMoveDistance)
        {
            // 현재 이동 방향으로 충돌이 있는지 확인합니다.
            // body.Cast는 Rigidbody2D가 이동 경로를 따라 충돌체를 감지하도록 합니다.
            var count = _rigidBody.Cast(move, contactFilter, hitBuffer, distance + shellRadius);

            // 충돌된 모든 오브젝트에 대해 반복 처리합니다.
            for (var i = 0; i < count; i++)
            {
                // 충돌한 표면의 법선 벡터를 가져옵니다.
                var currentNormal = hitBuffer[i].normal;

                // 현재 표면이 캐릭터가 설 수 있는 평평한 바닥인지 확인합니다.
                if (currentNormal.y > minGroundNormalY)
                {
                    // 캐릭터가 바닥에 접촉하고 있다고 표시합니다.
                    isGrounded = true;
                }

                // 캐릭터가 바닥에 붙어 있는 경우 추가 계산을 수행합니다.
                if (isGrounded)
                {
                    // 속도 벡터와 법선 벡터의 내적을 계산합니다.
                    // 내적 값은 속도가 표면 법선과 얼마나 정렬되어 있는지를 나타냅니다.
                    var projection = Vector2.Dot(velocity, currentNormal);

                    // 만약 내적 값이 음수라면(법선에 반대 방향으로 이동 중이라면)
                    if (projection < 0)
                    {
                        // 법선 방향으로 속도를 조정하여 표면에 따라 움직이도록 합니다.
                        velocity = velocity - projection * currentNormal;
                    }
                }
                else
                {
                    // 캐릭터가 공중에 있을 경우 충돌 처리.
                    // 수평 속도를 0으로 줄이고, 수직 속도는 위로 가는 경우만 제한합니다.
                    velocity.x *= 0;
                    velocity.y = Mathf.Min(velocity.y, 0);
                }

                // 실제 이동 거리를 계산합니다.
                // 충돌 지점에서 껍질 반경(shellRadius)을 제외한 거리를 이동합니다.
                var modifiedDistance = hitBuffer[i].distance - shellRadius;

                // 현재 이동 거리보다 작은 경우 수정된 거리를 사용합니다.
                distance = modifiedDistance < distance ? modifiedDistance : distance;
            }

            // 최종적으로 Rigidbody2D의 위치를 업데이트합니다.
            // 방향 벡터(move.normalized)와 계산된 거리(distance)를 곱해 이동합니다.
            _rigidBody.position = _rigidBody.position + move.normalized * distance;
        }
    }
}

using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float moveSpeed;
    public float waitTime = 2f; // 목표 지점에서 대기 시간

    private float targetX; // 현재 목표 x좌표
    private bool isWaiting = false; // 대기 상태 확인

    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Collider2D _collder2D;

    public Bounds Bounds => _collder2D.bounds;

    void Start()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collder2D = GetComponent<Collider2D>();

        // 초기 목표는 pointA의 x좌표
        targetX = pointA.position.x;
    }

    void Update()
    {
        if (_collder2D.enabled == false) return;

        // 대기 중이면 움직임을 멈춤
        if (isWaiting) return;

        // X 좌표를 목표 x좌표로 이동
        transform.position = new Vector3(
            Mathf.MoveTowards(transform.position.x, targetX, moveSpeed * Time.deltaTime),
            transform.position.y, // Y 좌표는 유지
            transform.position.z  // Z 좌표는 유지
        );

        bool arrived = Mathf.Abs(transform.position.x - targetX) < 0.1f;

        // 목표 x좌표에 도달했는지 확인
        if (arrived)
        {
            StartCoroutine(WaitAndSwitchTarget());
        }

        if (targetX > transform.position.x)
        {
            _spriteRenderer.flipX = false;
        }
        else
        {
            _spriteRenderer.flipX = true;
        }

        _animator.SetBool("running", arrived == false);
    }

    private IEnumerator WaitAndSwitchTarget()
    {
        // 대기 상태 활성화
        isWaiting = true;

        // 대기 시간 동안 멈춤
        yield return new WaitForSeconds(waitTime);

        // 목표를 반전 (pointA <-> pointB)
        targetX = Mathf.Approximately(targetX, pointA.position.x) ? pointB.position.x : pointA.position.x;

        // 대기 상태 해제
        isWaiting = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var playerController = collision.gameObject.GetComponent<PlayerController>();
        if (playerController != null)
        {
            var willHurtEnemy = playerController.Bounds.center.y >= Bounds.max.y;

            if (willHurtEnemy)
            {
                _animator.SetTrigger("death");
                _collder2D.enabled = false;
                Destroy(gameObject, 0.3f);
            }
            else
            {
                playerController.Hurt();
            }
        }
    }
}

using UnityEngine;
using System;

public class Target : MonoBehaviour
{
    public event Action OnTargetDestroyed;

    private bool isPeeking;
    private int currentHealth;

    // 클래스 변수로 애니메이터 참조 추가
    [SerializeField] private Animator animator;
    [SerializeField] private int maxHealth = 3;

    void Start()
    {
        currentHealth = maxHealth;

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void Initialize(bool isPeeking)
    {
        this.isPeeking = isPeeking;

        // 타겟 타입에 따른 초기화 로직
        if (isPeeking)
        {
            SetupPeekingBehavior();
        }
        else
        {
            SetupMovingBehavior();
        }
    }

    private void SetupPeekingBehavior()
    {
        // 랜덤하게 오른쪽(true) 또는 왼쪽(false)으로 설정
        bool isRight = UnityEngine.Random.Range(0, 2) == 0;
        animator.SetBool("right", isRight);

        // 디버그 로그로 방향 확인
        Debug.Log(isRight ? "Peeking to the right" : "Peeking to the left");
    }

    private void SetupMovingBehavior()
    {
        Debug.Log(transform.position.x);

        if (transform.position.x > -12f)
        {
            Debug.Log("left"); //x좌표 증가
            animator.SetBool("right", false);
        }
        else
        {
            Debug.Log("right"); //x좌표 감소
            animator.SetBool("right", true);
        }
    }

    // 타겟이 맞았을 때 호출되는 메서드
    public bool Hit(int damage)
    {
        if (currentHealth <= 0) return false;

        currentHealth -= damage;
        
        /* 
        타겟 피격 대미지 테스트
        Debug.Log($"Hit! Remaining HP: {currentHealth}");
        */

        // 히트 효과, 점수 등의 로직
        if (currentHealth <= 0)
        {
            ScoreManager.Instance?.AddScore(1); // 킬 점수 추가
            DestroyTarget();
            return true; // 타겟이 파괴되었음을 알림
        }
        return false;
    }

    // 타겟이 파괴될 때 (히트 또는 시간 초과)
    public void DestroyTarget()
    {
        OnTargetDestroyed?.Invoke();
        Destroy(gameObject);
    }

    /* Test
    private void Test()
    {
        float randomDelay = UnityEngine.Random.Range(1f, 3f);
        Invoke("DestroyTarget", randomDelay);
    }
    private void Start()
    {
        Test();
    }
    */
}

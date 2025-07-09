using UnityEngine;

enum HitType
{
    None = 0,
    Body = 1,
    Head = 2,
    Kill = 3
}

public class Gun : MonoBehaviour
{
	[SerializeField] private Transform cameraTransform;
    private readonly int headDamage = 3;
    private readonly int bodyDamage = 1;

    // 카메라 자동 설정
    void Start()
	{
		if (cameraTransform == null)
		{
			cameraTransform = Camera.main.transform;
		}
	}

    public (int hitType, bool isPeeking) Shoot()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            // 1. 먼저 벽인지 확인
            if (hit.collider.CompareTag("Wall"))
            {
                Debug.Log("벽에 먼저 부딪힘.");
                return ((int)HitType.None, false);
            }

            // 2. 타겟 판정
            var target = hit.collider.GetComponentInParent<Target>();
            if (target != null)
            {
                int damage = hit.collider.CompareTag("Head") ? headDamage : bodyDamage;
                bool killed = target.Hit(damage);

                if (killed)
                {
                    // 타겟이 죽었다면, 그것이 헤드샷 때문인지 확인
                    if (damage == headDamage)
                    {
                        // 헤드샷으로 처치
                        return ((int)HitType.Head, target.isPeeking);
                    }
                    else
                    {
                        // 몸샷으로 처치
                        return ((int)HitType.Kill, target.isPeeking);
                    }
                }
                else // 타겟이 아직 살아있다면
                {
                    // 타겟이 아직 살아있다면, 그것은 데미지만 입은 몸샷입니다.
                    return ((int)HitType.Body, target.isPeeking);
                }
            }
        }

        return ((int)HitType.None, false);
    }

}

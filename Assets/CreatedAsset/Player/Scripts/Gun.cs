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

    public int Shoot()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            // 1. 먼저 벽인지 확인
            if (hit.collider.CompareTag("Wall"))
            {
                Debug.Log("벽에 먼저 부딪힘.");
                return (int)HitType.None;
            }

            // 2. 타겟 판정
            var target = hit.collider.GetComponentInParent<Target>();
            if (target != null)
            {
                int damage = hit.collider.CompareTag("Head") ? headDamage : bodyDamage;
                bool killed = target.Hit(damage);

                if (damage == 3)
                {
                    return (int)HitType.Head;
                }
                else if (killed)
                {
                    return (int)HitType.Kill;
                }
                else
                {
                    return (int)HitType.Body;
                }
            }
        }

        return (int)HitType.None;
    }

}

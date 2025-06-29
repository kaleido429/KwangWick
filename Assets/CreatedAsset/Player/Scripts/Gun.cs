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
        	//Debug.Log($"[SHOOT] Hit: {hit.collider.name}, Tag: {hit.collider.tag}");

        	var target = hit.collider.GetComponentInParent<Target>();
        	if (target == null)
        	{
            	//Debug.Log("Target component not found on hit object!");
            }
        	else
        	{
            	//Debug.Log("Target found, calling Hit()");
                int damage = hit.collider.CompareTag("Head") ? headDamage : bodyDamage;
                bool killed = target.Hit(damage);

                if(damage == 3)
                {
                    //Debug.Log("Headshot!");
                    return (int)HitType.Head;
                }
                else if (killed)
                {
                    //Debug.Log("Target killed!");
                    return (int)HitType.Kill;
                }
                else
                {
                    //Debug.Log("Target hit but not killed.");
                    return (int)HitType.Body;
                }
            }
    	}
    	else
    	{
        	//Debug.Log("Nothing hit by raycast!");
    	}
        return (int)HitType.None;
    }
}

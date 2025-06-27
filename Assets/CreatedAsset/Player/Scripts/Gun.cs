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
        	Debug.Log($"[SHOOT] Hit: {hit.collider.name}, Tag: {hit.collider.tag}");

        	var target = hit.collider.GetComponentInParent<Target>();
        	if (target == null)
        	{
            	Debug.Log("Target component not found on hit object!");
            }
        	else
        	{
            	Debug.Log("Target found, calling Hit()");
                int damage = hit.collider.CompareTag("Head") ? 3 : 1;
                bool killed = target.Hit(damage);

                if(killed)
                {
                    Debug.Log("Target killed!");
                    return (int)HitType.Kill;
                }
                else
                {
                    Debug.Log("Target hit but not killed.");
                    return damage > 1 ? (int)HitType.Head : (int)HitType.Body;
                }
            }
    	}
    	else
    	{
        	Debug.Log("Nothing hit by raycast!");
    	}
        return (int)HitType.None;
    }
}

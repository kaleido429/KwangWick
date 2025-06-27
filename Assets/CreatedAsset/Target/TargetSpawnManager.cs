using UnityEngine;
using System.Collections.Generic;

public class TargetSpawnManager : MonoBehaviour
{
    [System.Serializable]
    public class TargetSettings
    {
        public GameObject targetPrefab;
        public Transform[] spawnPoints;
        public int maxTargets;
        public float respawnDelay;
    }

    [SerializeField] private TargetSettings peekingTargets;  // 장애물 뒤에서 피킹하는 타겟
    [SerializeField] private TargetSettings movingTargets;   // 멀리서 움직이는 타겟

    private List<GameObject> activePeekingTargets = new List<GameObject>();
    private List<GameObject> activeMovingTargets = new List<GameObject>();
    private List<float> peekingTargetTimers = new List<float>();
    private List<float> movingTargetTimers = new List<float>();

    void Start()
    {
        if (peekingTargets.maxTargets <= 0) peekingTargets.maxTargets = 4;
        if (movingTargets.maxTargets <= 0) movingTargets.maxTargets = 3;
        InitialSpawn();
    }

    // Update is called once per frame
    void Update()
    {
        if (peekingTargetTimers.Count > 0)
        {
            List<int> indicesToRemove = new List<int>();
            for (int i = 0; i < peekingTargetTimers.Count; i++)
            {
                peekingTargetTimers[i] -= Time.deltaTime;
                if (peekingTargetTimers[i] <= 0f)
                {
                    SpawnTarget(true);
                    indicesToRemove.Add(i);
                }
            }

            // 역순 제거
            for (int i = indicesToRemove.Count - 1; i >= 0; i--)
            {
                if (indicesToRemove[i] < peekingTargetTimers.Count)
                    peekingTargetTimers.RemoveAt(indicesToRemove[i]);
            }
        }

        if (movingTargetTimers.Count > 0)
        {
            List<int> indicesToRemove = new List<int>();
            for (int i = 0; i < movingTargetTimers.Count; i++)
            {
                movingTargetTimers[i] -= Time.deltaTime;
                if (movingTargetTimers[i] <= 0f)
                {
                    SpawnTarget(false);
                    indicesToRemove.Add(i);
                }
            }

            for (int i = indicesToRemove.Count - 1; i >= 0; i--)
            {
                if (indicesToRemove[i] < movingTargetTimers.Count)
                    movingTargetTimers.RemoveAt(indicesToRemove[i]);
            }
        }
    }



    private void InitialSpawn()
    {
        // 초기 피킹 타겟 생성
        for (int i = 0; i < peekingTargets.maxTargets; i++)
        {
            SpawnTarget(true);
        }

        // 초기 움직이는 타겟 생성
        for (int i = 0; i < movingTargets.maxTargets; i++)
        {
            SpawnTarget(false);
        }
    }

    private void SpawnTarget(bool isPeeking)
    {
        TargetSettings settings = isPeeking ? peekingTargets : movingTargets;
        List<GameObject> activeList = isPeeking ? activePeekingTargets : activeMovingTargets;

        // 최대 수에 도달했는지 확인
        if (activeList.Count >= settings.maxTargets) return;

        // 스폰 포인트 확인
        if (settings.spawnPoints == null || settings.spawnPoints.Length == 0)
        {
            Debug.LogWarning($"{(isPeeking ? "피킹" : "움직이는")} 타겟의 스폰 포인트가 설정되지 않았습니다!");
            return;
        }

        // 사용 가능한 스폰 포인트만 선택
        List<Transform> availablePoints = new List<Transform>();
        foreach (Transform point in settings.spawnPoints)
        {
            var sp = point.GetComponent<PreventSpawnOverlap>();
            if (sp != null && !sp.IsOccupied)
            {
                availablePoints.Add(point);
            }
        }

        if (availablePoints.Count == 0)
        {
            Debug.LogWarning("모든 스폰 포인트가 점유 중입니다!");
            return;
        }
        
        Transform chosenPoint = availablePoints[Random.Range(0, availablePoints.Count)];
        var chosenScript = chosenPoint.GetComponent<PreventSpawnOverlap>();
        chosenScript.SetOccupied(true);

        GameObject target = Instantiate(settings.targetPrefab, chosenPoint.position, chosenPoint.rotation);

        if (target.TryGetComponent<Target>(out Target targetScript))
        {
            targetScript.Initialize(isPeeking);
            targetScript.OnTargetDestroyed += () =>
            {
                HandleTargetDestroyed(target, isPeeking);
                chosenScript.SetOccupied(false); // 자리 비우기
            };
        }

    activeList.Add(target);
    }
    
    private void HandleTargetDestroyed(GameObject target, bool isPeeking)
    {
        // 활성 리스트에서 제거
        List<GameObject> activeList = isPeeking ? activePeekingTargets : activeMovingTargets;
        if (activeList.Contains(target))
        {
            activeList.Remove(target);
        }
        
        // 리스폰 타이머 추가
        float delay = isPeeking ? peekingTargets.respawnDelay : movingTargets.respawnDelay;
        if (isPeeking)
            peekingTargetTimers.Add(delay);
        else
            movingTargetTimers.Add(delay);
    }

    private void OnDrawGizmos()
    {
        // 피킹 타겟 스폰 포인트 시각화 (빨간색)
        if (peekingTargets != null && peekingTargets.spawnPoints != null)
        {
            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.8f); // 빨간색
            DrawSpawnPoints(peekingTargets.spawnPoints, "피킹 타겟");
        }

        // 움직이는 타겟 스폰 포인트 시각화 (파란색)
        if (movingTargets != null && movingTargets.spawnPoints != null)
        {
            Gizmos.color = new Color(0.3f, 0.3f, 1f, 0.8f); // 파란색
            DrawSpawnPoints(movingTargets.spawnPoints, "움직이는 타겟");
        }
    }

    private void DrawSpawnPoints(Transform[] spawnPoints, string prefix)
    {
        if (spawnPoints == null) return;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Transform point = spawnPoints[i];
            if (point == null) continue;

            // 구체 그리기
            Gizmos.DrawSphere(point.position, 0.3f);
            
            // 포인트의 방향 표시 (화살표 효과)
            Gizmos.DrawRay(point.position, point.forward * 1.5f);
            
#if UNITY_EDITOR
            // 이름 표시 (에디터에서만 작동)
            UnityEditor.Handles.color = Gizmos.color;
            UnityEditor.Handles.Label(point.position + Vector3.up * 0.5f, $"{prefix} {i+1}");
#endif
        }
    }
}
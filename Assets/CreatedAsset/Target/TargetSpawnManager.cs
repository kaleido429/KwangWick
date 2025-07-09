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

    [SerializeField] private TargetSettings peekingTargets;
    [SerializeField] private TargetSettings movingTargets;

    private List<GameObject> activePeekingTargets = new List<GameObject>();
    private List<GameObject> activeMovingTargets = new List<GameObject>();
    private List<float> peekingTargetTimers = new List<float>();
    private List<float> movingTargetTimers = new List<float>();

    // 피킹 타겟과 움직이는 타겟의 마지막 스폰 포인트를 별도로 추적
    private Transform lastUsedPeekingSpawnPoint;
    private Transform lastUsedMovingSpawnPoint;

    void Start()
    {
        if (peekingTargets.maxTargets <= 0) peekingTargets.maxTargets = 4;
        if (movingTargets.maxTargets <= 0) movingTargets.maxTargets = 3;
        InitialSpawn();
    }

    void Update()
    {
        // 리스폰 타이머 로직 (기존과 동일)
        HandleRespawnTimers(peekingTargetTimers, true);
        HandleRespawnTimers(movingTargetTimers, false);
    }
    
    // spawn delay
    private void HandleRespawnTimers(List<float> timers, bool isPeeking)
    {
        for (int i = timers.Count - 1; i >= 0; i--)
        {
            timers[i] -= Time.deltaTime;
            if (timers[i] <= 0f)
            {
                SpawnTarget(isPeeking);
                timers.RemoveAt(i);
            }
        }
    }

    private void InitialSpawn()
    {
        for (int i = 0; i < peekingTargets.maxTargets; i++) SpawnTarget(true);
        for (int i = 0; i < movingTargets.maxTargets; i++) SpawnTarget(false);
    }

    private void SpawnTarget(bool isPeeking)
    {
        TargetSettings settings = isPeeking ? peekingTargets : movingTargets;
        List<GameObject> activeList = isPeeking ? activePeekingTargets : activeMovingTargets;
        
        // 현재 타겟 타입에 맞는 마지막 스폰 포인트를 가져옴
        Transform lastPointToCheck = isPeeking ? lastUsedPeekingSpawnPoint : lastUsedMovingSpawnPoint;

        if (activeList.Count >= settings.maxTargets) return;
        if (settings.spawnPoints == null || settings.spawnPoints.Length < 2)
        {
            //Debug.LogWarning($"스폰 포인트가 부족하여 중복 방지 로직을 실행할 수 없습니다. (타입: {(isPeeking ? "peeking" : "moveing")})");
            // 스폰 포인트가 하나뿐일 경우 그냥 진행
            if (settings.spawnPoints.Length == 1 && !settings.spawnPoints[0].GetComponent<PreventSpawnOverlap>().IsOccupied)
            {
                 // 로직 생략하고 바로 스폰...
            }
            else return;
        }

        List<Transform> availablePoints = new List<Transform>();
        foreach (Transform point in settings.spawnPoints)
        {
            var sp = point.GetComponent<PreventSpawnOverlap>();
            // 현재 타겟 타입의 마지막 스폰 포인트와 비교
            if (sp != null && !sp.IsOccupied && point != lastPointToCheck)
            {
                availablePoints.Add(point);
            }
        }

        if (availablePoints.Count == 0)
        {
            /*
            사용 가능한 포인트가 없다면, 마지막 포인트를 제외한 모든 포인트가 점유된 상태일 수 있음
            이 경우, 마지막 포인트를 포함하여 다시 시도
            */
            if (lastPointToCheck != null && !lastPointToCheck.GetComponent<PreventSpawnOverlap>().IsOccupied)
            {
                availablePoints.Add(lastPointToCheck);
            }

            if(availablePoints.Count == 0)
            {
                Debug.Log("모든 스폰 포인트가 점유 중입니다!");
                return;
            }
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
                chosenScript.SetOccupied(false);
            };
        }

        // 현재 타겟 타입에 맞는 마지막 스폰 포인트를 업데이트
        if (isPeeking)
            lastUsedPeekingSpawnPoint = chosenPoint;
        else
            lastUsedMovingSpawnPoint = chosenPoint;

        activeList.Add(target);
    }
    
    private void HandleTargetDestroyed(GameObject target, bool isPeeking)
    {
        List<GameObject> activeList = isPeeking ? activePeekingTargets : activeMovingTargets;
        if (activeList.Contains(target))
        {
            activeList.Remove(target);
        }
        
        float delay = isPeeking ? peekingTargets.respawnDelay : movingTargets.respawnDelay;
        if (isPeeking)
            peekingTargetTimers.Add(delay);
        else
            movingTargetTimers.Add(delay);
    }

    // OnDrawGizmos (Unity 에디터에서만 사용)
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
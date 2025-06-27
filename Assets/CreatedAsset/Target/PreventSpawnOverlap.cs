using UnityEngine;

public class PreventSpawnOverlap : MonoBehaviour
{
    public bool IsOccupied { get; private set; }

    public void SetOccupied(bool occupied)
    {
        IsOccupied = occupied;
    }
}

using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    /*
     * [플레이어의 정보를 저장하는 스크립트입니다.]
     */

    #region Variables

    [Tooltip("플레이어 정보")]
    [field: SerializeField, Header("Plyaer Info")] public string PlayerName { get; set; } = "Player";
    [field: SerializeField] public int PlayerScore { get; set; } = 0;
    [field: SerializeField] public int Kills { get; set; } = 0;

    #endregion
}

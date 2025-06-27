using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    /*
     * [사운드를 관리하는 스크립트입니다.]
     */

    #region Variables

    [field: SerializeField, Header("Audio Settings")] public List<AudioSource> AudioSourceList { get; set; }

    #endregion

    #region User Methods

    // 총기를 발사할 때 호출되는 메소드입니다.
    public void GunFire()
    {
        if (AudioSourceList == null || AudioSourceList.Count == 0)
        {
            Debug.LogWarning("SoundManager: AudioSourceList가 비어 있음!");
            return;
        }

        if (AudioSourceList[0].clip == null)
        {
            Debug.LogWarning("SoundManager: AudioSourceList[0]에 클립이 없음!");
            return;
        }

        AudioSourceList[0].PlayOneShot(AudioSourceList[0].clip);
    }

    #endregion
}

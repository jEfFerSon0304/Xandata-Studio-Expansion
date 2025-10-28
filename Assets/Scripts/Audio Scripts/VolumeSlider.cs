using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public enum VolumeType { Music, SFX }
    public VolumeType type;
    private Slider slider;

    void Start()
    {
        slider = GetComponent<Slider>();
        slider.value = PlayerPrefs.GetFloat(type.ToString() + "Volume", 1f);
        slider.onValueChanged.AddListener(OnVolumeChange);
    }

    void OnVolumeChange(float value)
    {
        PlayerPrefs.SetFloat(type.ToString() + "Volume", value);
        if (type == VolumeType.Music)
            SoundManager.Instance.SetMusicVolume(value);
        else
            SoundManager.Instance.SetSFXVolume(value);
    }
}

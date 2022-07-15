using Maroon.GlobalEntities;
using UnityEngine;
using UnityEngine.UI;


public class scrMenuColumnAudio : MonoBehaviour
{
    // #################################################################################################################
    // Members

    // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    // Buttons/Sliders

    [SerializeField] private GameObject _musicSlider;

    [SerializeField] private GameObject _soundEffectSlider;

    // #################################################################################################################
    // Methods

    void Start()
    {
        var musicSliderComponent = _musicSlider.GetComponent<Slider>();
        var soundEffectSliderComponent = _soundEffectSlider.GetComponent<Slider>();
        
        // Initial values
        musicSliderComponent.value = SoundManager.Instance.MusicVolume;
        soundEffectSliderComponent.value = SoundManager.Instance.SoundEffectVolume;

        // Listen to music slider
        musicSliderComponent.onValueChanged
                    .AddListener((value) => this.OnChangeMusicSlider(value));

        // Listen to sound effect slider
        soundEffectSliderComponent.onValueChanged
                          .AddListener((value) => this.OnChangeSoundEffectSlider(value));
    }

    void OnChangeMusicSlider(float value)
    {
        SoundManager.Instance.MusicVolume = value;
    }

    void OnChangeSoundEffectSlider(float value)
    {
        SoundManager.Instance.SoundEffectVolume = value;
    }
}

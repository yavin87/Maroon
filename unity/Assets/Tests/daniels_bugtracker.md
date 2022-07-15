# Van de Graff Generator
* While in audio menu click through on generator triggers helper dialog

# Main Menu/Pause Menu
* SoundEffectSlider does not listen/observe actual sound value and resets to default (0) on any menu reload
	- Proposed fix: on menu open, apply volume of audio sources to sliders in `Start()` of *srcMenuColumnAudio.cs*
	```C#
	var musicSliderComponent = _musicSlider.GetComponent<Slider>();
	var soundEffectSliderComponent = _soundEffectSlider.GetComponent<Slider>();
	// Set slider values from sources
	musicSliderComponent.value = SoundManager.Instance.MusicVolume;
	soundEffectSliderComponent.value = SoundManager.Instance.SoundEffectVolume;
	```
* MusicSource is not assigned to SoundManager due to *SoundManager.cs*, line 116
	- Proposed fix: remove said line
	```C#
	_musicSource = GetComponent<AudioSource>();
	```
* Player sounds (footstep/jump/land) are not affected by sound effects slider
	- Proposed fix: find sound effect audio source in `Start()` of *ModeFirstPerson.cs*
	```C#
	audioSource = GameObject.Find("SoundEffectSource").GetComponent<AudioSource>();
	```
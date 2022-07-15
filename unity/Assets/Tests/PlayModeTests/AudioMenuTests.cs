using System.Collections;
using GEAR.Localization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

/*
 * Tests audio sliders of Main/Pause audio menu
 */

namespace Tests.PlayModeTests
{
    using static PlaymodeTestUtils;
    public class AudioMenuTests
    {
        [UnityTest]
        public IEnumerator WhenChangeSoundFxAudioSliderValueThenSoundFxAudioSourceVolumeChanges()
        {
            var gameObjectName = "SoundEffectSource";
            var audioSourceGameObject = GameObject.Find(gameObjectName);
            Assert.NotNull(audioSourceGameObject, $"Could not find GameObject '{gameObjectName}'");

            var audioSourceComponent = audioSourceGameObject.GetComponent<AudioSource>();
            Assert.NotNull(audioSourceComponent, $"Could not find AudioSource component in GameObject '{gameObjectName}'");
            
            var componentInitialVolume = audioSourceComponent.volume;
            
            
            // ValueSource thingy/ parameterized test: provide both sources/sliders for same test
            // 1. Get slider value
            // 2. Get sound src value
            // 3. Compare if equal

            float oldVolumeValue = 0f;
            float newVolumeValue = 1f;
            
            // Make sure audio setting 'Sound FX' is set to 0 (off)
            var audioSourceFx = GetComponentInChildrenFromGameObjectWithName<AudioSource>("SoundEffectSource");
            Assert.AreEqual(oldVolumeValue, audioSourceFx.volume, "audio should be off");
            
            // Click Audio button to open audio menu
            string mainMenuAudioButtonLabel = LanguageManager.Instance.GetString("Menu Audio");
            GetButtonViaText(mainMenuAudioButtonLabel).onClick.Invoke();
            yield return null;
            
            var slider = GetComponentInChildrenFromGameObjectWithName<Slider>("preMenuButtonSliderFx");
            // change value of slider
            slider.value = newVolumeValue;
            yield return null;
            
            Assert.AreEqual(newVolumeValue, audioSourceFx.volume);
        }
    }
}
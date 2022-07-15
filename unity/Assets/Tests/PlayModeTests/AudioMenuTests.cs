using System.Collections;
using GEAR.Localization;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

/*
 * Tests audio sliders of Main/Pause audio menu
 */

namespace Tests.PlayModeTests
{
    using static PlaymodeTestUtils;
    // TODO apply TestFixture with soundFx and music as inputs
    public class AudioMenuTests
    {
        private const string MainMenuScenePath = "Assets/Maroon/scenes/special/MainMenu.pc.unity";

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // TODO extract load main menu to util method
            
            // Start from Main Menu for every following test
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(MainMenuScenePath, new LoadSceneParameters(LoadSceneMode.Single));

            var currentSceneName = SceneManager.GetActiveScene().name;
            Assert.AreEqual("MainMenu.pc", currentSceneName, "'MainMenu.pc' scene did not load");
            
            // Enter playmode to initialize scene (button labels, etc.)
            yield return new EnterPlayMode();
            
            // Click Audio button to open audio menu
            string mainMenuAudioButtonLabel = LanguageManager.Instance.GetString("Menu Audio");
            var audioButton = GetButtonViaText(mainMenuAudioButtonLabel);
            audioButton.onClick.Invoke();
            
            yield return null;
        }
        
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            // TODO not necessary?
            yield return new ExitPlayMode();
        }
        
        [UnityTest]
        public IEnumerator WhenOpenAudioMenuInitialSliderValueEqualsAudioSourceVolume()
        {
            var gameObjectName = "SoundEffectSource";
            var audioSourceGameObject = GameObject.Find(gameObjectName);
            Assert.NotNull(audioSourceGameObject, $"Could not find GameObject '{gameObjectName}'");

            var audioSourceComponent = audioSourceGameObject.GetComponent<AudioSource>();
            Assert.NotNull(audioSourceComponent, $"Could not find AudioSource component in GameObject '{gameObjectName}'");
            
            var audioSourceInitialVolume = audioSourceComponent.volume;
            
            var slider = GetComponentInChildrenFromGameObjectWithName<Slider>("preMenuButtonSliderFx");
            var audioSliderInitialValue = slider.value;

            Assert.AreEqual(audioSourceInitialVolume, audioSliderInitialValue, 0.0, 
                "Mismatched initial values of AudioSource.volume and Slider.value");

            yield return null;

            // ValueSource thingy/ parameterized test: provide both sources/sliders for same test
            // 1. Get slider value
            // 2. Get sound src value
            // 3. Compare if equal
            //
            // float oldVolumeValue = 0f;
            // float newVolumeValue = 1f;
            //
            // // Make sure audio setting 'Sound FX' is set to 0 (off)
            // var audioSourceFx = GetComponentInChildrenFromGameObjectWithName<AudioSource>("SoundEffectSource");
            // Assert.AreEqual(oldVolumeValue, audioSourceFx.volume, "audio should be off");
            //
            //
            //
            // var slider = GetComponentInChildrenFromGameObjectWithName<Slider>("preMenuButtonSliderFx");
            // // change value of slider
            // slider.value = newVolumeValue;
            // yield return null;
            //
            // Assert.AreEqual(newVolumeValue, audioSourceFx.volume);
        }
        
        // TODO further test cases
        // 1. change value, check if ok
        // 2. change multiple times, check if ok
        // 3. change value and reload menu (switch to other, or press button again), check if ok
    }
}
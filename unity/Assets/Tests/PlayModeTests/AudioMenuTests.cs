using System;
using System.Collections;
using GEAR.Localization;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/*
 * Tests audio sliders of Main/Pause audio menu
 */

namespace Tests.PlayModeTests
{
    using static PlaymodeTestUtils;
    // TODO apply TestFixture with soundFx and music as inputs
    
    [TestFixtureSource(nameof(_fixtureArgs))]
    public class AudioMenuTests
    {
        private static object [] _fixtureArgs = {
            new object[] { "MusicSource", "preMenuButtonSliderMusic" },
            new object[] { "SoundEffectSource", "preMenuButtonSliderFx" }
        };
        
        private readonly string _audioSourceName;
        private readonly string _sliderName;

        private AudioSource _audioSourceComponent;
        private Slider _audioSlider;
        
        private const string MainMenuScenePath = "Assets/Maroon/scenes/special/MainMenu.pc.unity";

        // A constructor is required for TestFixtureSource to pass arguments
        public AudioMenuTests(string audioSourceName, string sliderName)
        {
            _audioSourceName = audioSourceName;
            _sliderName = sliderName;
        }

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // TODO extract load main menu to util method and use everywhere
            
            // Start from Main Menu for every following test
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(MainMenuScenePath, new LoadSceneParameters(LoadSceneMode.Single));

            var currentSceneName = SceneManager.GetActiveScene().name;
            Assert.AreEqual("MainMenu.pc", currentSceneName, "'MainMenu.pc' scene did not load");
            
            // Enter playmode to initialize scene (especially the button labels)
            yield return new EnterPlayMode();
            
            // Click Audio button to open audio menu
            string mainMenuAudioButtonLabel = LanguageManager.Instance.GetString("Menu Audio");
            var audioButton = GetButtonViaText(mainMenuAudioButtonLabel);
            audioButton.onClick.Invoke();
            
            yield return null;
            
            var audioSourceGameObject = GameObject.Find(_audioSourceName);
            Assert.NotNull(audioSourceGameObject, $"Could not find GameObject '{_audioSourceName}'");

            _audioSourceComponent = audioSourceGameObject.GetComponent<AudioSource>();
            Assert.NotNull(_audioSourceComponent, $"Could not find AudioSource component in GameObject '{_audioSourceName}'");
            
            _audioSlider = GetComponentInChildrenFromGameObjectWithName<Slider>(_sliderName);
        }
        
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            // TODO not necessary?
            yield return new ExitPlayMode();
        }
        
        [UnityTest, Description("On opening audio menu, slider value and audio source volume must match")]
        public IEnumerator WhenOpenAudioMenuInitialSliderValueEqualsAudioSourceVolume()
        {
            var audioSourceInitialVolume = _audioSourceComponent.volume;
            var audioSliderInitialValue = _audioSlider.value;

            Assert.AreEqual(audioSourceInitialVolume, audioSliderInitialValue, 0.0, 
                "Mismatched initial values of AudioSource.volume and Slider.value");
            
            yield return null;
        }
        
        
        // 1. change value, check if ok
        [UnityTest]
        public IEnumerator WhenChangeSliderValueThenAudioSourceVolumeMatches([Values(0f, 0.25f, 0.5f, 0.75f, 0.9999999f)] float volumeLevel)
        {
            float newVolumeLevel = Random.Range(0f, 1f);
            _audioSlider.value = newVolumeLevel;
            
            yield return null;
            
            var audioSourceVolume = _audioSourceComponent.volume;
            
            Assert.AreEqual(newVolumeLevel, audioSourceVolume, 0.0, 
                "After slider value change, mismatched AudioSource.volume");
        }
        
        // 2. change multiple times, check if ok
        [UnityTest]
        public IEnumerator WhenChangeSliderValueMultipleTimesThenAudioSourceVolumeMatches()
        {
            float newVolumeLevel = 0.5f;
            
            var slider = GetComponentInChildrenFromGameObjectWithName<Slider>("preMenuButtonSliderFx");
            slider.value = newVolumeLevel;
            
            yield return null;
            
            var gameObjectName = "SoundEffectSource";
            var audioSourceGameObject = GameObject.Find(gameObjectName);
            Assert.NotNull(audioSourceGameObject, $"Could not find GameObject '{gameObjectName}'");

            var audioSourceComponent = audioSourceGameObject.GetComponent<AudioSource>();
            Assert.NotNull(audioSourceComponent, $"Could not find AudioSource component in GameObject '{gameObjectName}'");
            
            var audioSourceVolume = audioSourceComponent.volume;
            
            Assert.AreEqual(newVolumeLevel, audioSourceVolume, 0.0, 
                "After slider value change, mismatched AudioSource.volume");
        }
        
        // TODO further test cases
        // 3. change value and reload menu (switch to other, or press button again), check if ok
    }
}
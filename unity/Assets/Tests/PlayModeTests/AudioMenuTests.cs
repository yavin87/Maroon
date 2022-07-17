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
        private Button _audioButton;
        
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
            _audioButton = GetButtonViaText(mainMenuAudioButtonLabel);
            _audioButton.onClick.Invoke();
            
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
        
        [UnityTest, Order(1), Description("On opening audio menu, slider value and audio source volume must match")]
        public IEnumerator WhenOpenAudioMenuInitialSliderValueEqualsAudioSourceVolume()
        {
            var audioSourceInitialVolume = _audioSourceComponent.volume;
            var audioSliderInitialValue = _audioSlider.value;

            Assert.AreEqual(audioSourceInitialVolume, audioSliderInitialValue, 0.0, 
                $"Initial values should be equal: slider '{_sliderName}' value and audio source '{_audioSourceName}' volume");
            
            yield return null;
        }
        
        [UnityTest, Order(2), Description("Change the audio slider's value, then the audio source's volume must match")]
        public IEnumerator WhenChangeSliderValueThenAudioSourceVolumeMatches()
        {
            _audioSlider.value = 0.5f;
            
            yield return null;
            
            var audioSourceVolume = _audioSourceComponent.volume;
            
            Assert.AreEqual(_audioSlider.value, audioSourceVolume, 0.0, 
                $"After slider '{_sliderName}' value change, mismatched audio source '{_audioSourceName}' volume");
        }
        
        [UnityTest, Order(3), Description("Change the audio slider's value multiple times, then the audio source's volume must match")]
        public IEnumerator WhenChangeSliderValueMultipleTimesThenAudioSourceVolumeMatches()
        {
            float[] volumeLevels = { 0f,  0.25f, 1/3f, 2/3f, 0.75f, 1f };

            // https://docs.nunit.org/articles/nunit/writing-tests/assertions/multiple-asserts.html
            // Assert multiple not supported by Unity test framework :(
            foreach (var volumeLevel in volumeLevels)
            {
                _audioSlider.value = volumeLevel;
        
                yield return null;
        
                var audioSourceVolume = _audioSourceComponent.volume;
                
                Assert.AreEqual(_audioSlider.value, audioSourceVolume, 0.0, 
                    $"After slider '{_sliderName}' value change, mismatched audio source '{_audioSourceName}' volume");
            }
        }
        
        [UnityTest, Order(4), Description("Change the audio slider's value, then reload menu, the audio source's volume must match")]
        public IEnumerator WhenChangeSliderValueThenReloadMenuAudioSourceVolumeMatches()
        {
            var expectedVolume = 0.5f;
            _audioSlider.value = expectedVolume;
            
            yield return null;
            
            // Reload menu
            _audioButton.onClick.Invoke();
            
            yield return null;
            
            // Get newly created slider
            _audioSlider = GetComponentInChildrenFromGameObjectWithName<Slider>(_sliderName);

            Assert.AreEqual(expectedVolume, _audioSlider.value, 0.0, 
                $"After audio menu reload, unexpected slider '{_sliderName}' value");
            Assert.AreEqual(expectedVolume, _audioSourceComponent.volume, 0.0, 
                $"After audio menu reload, unexpected audio source '{_audioSourceName}' volume");
        }
    }
}
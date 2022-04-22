using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GEAR.Localization;
using NUnit.Framework;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

/*
 * Inspired by https://forum.unity.com/threads/play-mode-tests-scenehandling.751049/
 *
 * "End to end" testing
 */

namespace Tests.PlayModeTests
{
    public class MainMenuPcNavigationTests // TODO change name to MainMenuPcTests (didnt do it on laptop, recompiling takes forever)
    {
        private const string MainMenuScenePath = "Assets/Maroon/scenes/special/MainMenu.pc.unity";
        private const SystemLanguage DefaultLanguage = SystemLanguage.English;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            // Start from Main Menu for every following test
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(MainMenuScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            
            var currentSceneName = SceneManager.GetActiveScene().name;
            Assert.AreEqual("MainMenu.pc", currentSceneName, "'MainMenu.pc' scene did not load");
            
            Assert.AreEqual(DefaultLanguage, LanguageManager.Instance.CurrentLanguage,
                $"Default language is not set to '{DefaultLanguage.ToString()}'");
        }
        
        public static readonly TopLevelMenuPathSource[] TopLevelMenuPaths =
        {
            new TopLevelMenuPathSource("Menu Lab", "preMenuColumnLaboratorySelection(Clone)"),
            new TopLevelMenuPathSource("Menu Audio", "preMenuColumnAudio(Clone)"),
            new TopLevelMenuPathSource("Menu Language", "preMenuColumnLanguage(Clone)"),
            new TopLevelMenuPathSource("Menu Credits", "preMenuColumnCredits(Clone)")
        };
        
        [UnityTest]
        public IEnumerator WhenClickTopLevelMenuItemThenOpenIt([ValueSource(nameof(TopLevelMenuPaths))]TopLevelMenuPathSource source)
        {
            string buttonLabel = LanguageManager.Instance.GetString(source.LabelToTranslate);
            string expectedMenuColumn = source.ExpectedMenuColumn;
            
            GetButtonViaText(buttonLabel).onClick.Invoke();
            yield return null;

            var menuColumn = GameObject.Find(expectedMenuColumn);
            Assert.NotNull(menuColumn, $"Could not find '{buttonLabel}' menu Gameobject '{expectedMenuColumn}'");
        }

        [UnityTest]
        public IEnumerator WhenSelectGermanLanguageInMenuThenLanguageManagerLanguageIsGerman()
        {
            // Find and click Language button
            string languageButtonLabel = LanguageManager.Instance.GetString("Menu Language", DefaultLanguage);
            GetButtonViaText(languageButtonLabel).onClick.Invoke();
            yield return null;
            
            // Find and click German button
            string germanButtonLabel = LanguageManager.Instance.GetString("German", DefaultLanguage);
            GetButtonViaText(germanButtonLabel).onClick.Invoke();
            yield return null;

            // Check if LanguageManager's language property is now German
            var expectedLanguage = SystemLanguage.German;
            var actualLanguage = LanguageManager.Instance.CurrentLanguage;
            
            Assert.AreEqual(expectedLanguage, actualLanguage,
                $"LanguageManager's 'CurrentLanguage' property is not {expectedLanguage.ToString()}");
        }
        
        [UnityTest]
        public IEnumerator WhenSelectGermanLanguageInMenuThenLanguageSubMenuIsGerman()
        {
            var buttonLabel = "Menu Language";
            var newLanguage = SystemLanguage.German;
            
            var button = GetButtonViaText(LanguageManager.Instance.GetString(buttonLabel, DefaultLanguage));
            button.onClick.Invoke();
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator WhenSelectGermanLanguageInMenuThenLanguageManagerSettingAndMenuLabelsChanged()
        {
            string[] topLevelButtonLabels = { "Menu Lab", "Menu Audio", "Menu Language", "Menu Credits", "Menu Exit" };
            
            // Find and store top level main menu buttons by their English label
            var preLanguageChangeButtons = topLevelButtonLabels.ToDictionary(
                buttonLabel => buttonLabel,
                buttonLabel => GetButtonViaText(LanguageManager.Instance.GetString(buttonLabel, DefaultLanguage))
            );

            // Find and click Language button
            string languageButtonLabel = LanguageManager.Instance.GetString("Menu Language", DefaultLanguage);
            GetButtonViaText(languageButtonLabel).onClick.Invoke();
            yield return null;
            
            // Find and click German button
            string germanButtonLabel = LanguageManager.Instance.GetString("German", DefaultLanguage);
            GetButtonViaText(germanButtonLabel).onClick.Invoke();
            yield return null;

            var expectedLanguage = SystemLanguage.German;
            
            // Find and store top level main menu buttons by their German label
            var postLanguageChangeButtons = topLevelButtonLabels.ToDictionary(
                buttonLabel => buttonLabel,
                buttonLabel => GetButtonViaText(LanguageManager.Instance.GetString(buttonLabel, expectedLanguage))
            );

            // Compare old English and new German labeled buttons if they are identical
            topLevelButtonLabels.ToList().ForEach(buttonLabel =>
                Assert.AreEqual(preLanguageChangeButtons[buttonLabel], postLanguageChangeButtons[buttonLabel])
            );

            /*
             * TODO
             * split language tests up, in case any of the steps fail it will be easier to debug
             * one test for languagemanager setting changed
             * one test for each toplevel button and its related submenu (maybe include main menu banner)
             *
             * More ideas:
             * make a new test suite for all menu language tests
             * get current language in setup and store as default language
             * test for changing it to English if everything is labeled correctly
             * test for changing it to German if everything is labeled correctly
             *
             * reasoning: independant of any future changes regarding the default language setting
             */
        }

        [UnityTest]
        public IEnumerator WhenChangeSoundFxAudioSliderValueThenSoundFxAudioSourceVolumeChanges()
        {
            /**
             * TODO
             * bug encountered, test won't work
             */
            float oldVolumeValue = 0f;
            float newVolumeValue = 1f;
            
            // Make sure audio setting 'Sound FX' is set to 0 (off)
            var audioSourceFx = GetComponentInChildrenFromGameObjectWithName<AudioSource>("SoundEffectSource");
            Assert.AreEqual(oldVolumeValue, audioSourceFx.volume);
            
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
        
        // TODO Test "selected button" idea:
        // draft: check if select icon activates on button click (small square with arrow inside)
        // 1. preliminary check that no button has the icon active
        // 2. then check clicked button if icon is active
        
        public static readonly LabMenuPathSource[] LaboratoryMenuPaths =
        {
            new LabMenuPathSource("Physics", "CoulombsLaw"),
            new LabMenuPathSource("Physics", "FallingCoil"),
            new LabMenuPathSource("Physics", "FaradaysLaw"),
            new LabMenuPathSource("Physics", "HuygensPrinciple"),
            new LabMenuPathSource("Physics", "PointWaveExperiment"),
            new LabMenuPathSource("Physics", "Pendulum"),
            new LabMenuPathSource("Physics", "VandeGraaffBalloon"),
            new LabMenuPathSource("Physics", "VandeGraaffGenerator"),
            new LabMenuPathSource("Physics", "Optics"),
            new LabMenuPathSource("Physics", "3DMotionSimulation"),
            new LabMenuPathSource("Physics", "Whiteboard"),
            new LabMenuPathSource("Chemistry", "TitrationExperiment"),
            new LabMenuPathSource("Computer Science", "Sorting")
        };
        
        [UnityTest]
        public IEnumerator WhenClickLabCategoryExperimentThenLoadScene([ValueSource(nameof(LaboratoryMenuPaths))] LabMenuPathSource source)
        {
            string labsButtonLabel = LanguageManager.Instance.GetString("Menu Lab");
            string categoryButtonLabel = source.Category;
            string experimentButtonLabel = source.Experiment;
            string sceneName = experimentButtonLabel + ".pc";

            GetButtonViaText(labsButtonLabel).onClick.Invoke();
            yield return null;
            
            GetButtonViaText(categoryButtonLabel).onClick.Invoke();
            yield return null;
            
            GetButtonViaText(experimentButtonLabel).onClick.Invoke();
            yield return null;
            
            var currentSceneName = SceneManager.GetActiveScene().name;
            Assert.AreEqual(sceneName, currentSceneName, $"Scene '{sceneName}' did not load");
        }

        
        public readonly struct LabMenuPathSource
        {
            public LabMenuPathSource(string category, string experiment)
            {
                Category = category;
                Experiment = experiment;
            }

            public string Category { get; }
            public string Experiment { get; }

            public override string ToString() => $"{Category} -> {Experiment}";
        }
        
        public readonly struct TopLevelMenuPathSource
        {
            public TopLevelMenuPathSource(string labelToTranslate, string expectedMenuColumn)
            {
                LabelToTranslate = labelToTranslate;
                ExpectedMenuColumn = expectedMenuColumn;
            }

            public string LabelToTranslate { get; }
            public string ExpectedMenuColumn { get; }

            public override string ToString() => $"{LabelToTranslate}";
        }

        private static Button GetButtonViaText(string buttonText)
        {
            Button buttonToReturn = null;

            var buttonGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            
            foreach (var buttonGameObject in buttonGameObjects)
            {
                var buttonComponent = buttonGameObject.GetComponent<Button>();
                var textComponent = buttonGameObject.GetComponentInChildren<TextMeshProUGUI>();
                
                if (buttonComponent && textComponent && textComponent.text == buttonText)
                {
                    if (buttonToReturn != null)
                    {
                        Assert.Fail($"Found more than one Button with Text '{buttonText}'");
                    }
                    buttonToReturn = buttonGameObject.GetComponent<Button>();
                }
            }
            
            Assert.NotNull(buttonToReturn, $"Could not find any Button with Text '{buttonText}'");
            
            return buttonToReturn;
        }
        
        private static T GetComponentInChildrenFromGameObjectWithName<T>(string gameObjectName)
        {
            var gameObject = GameObject.Find(gameObjectName);
            Assert.NotNull(gameObject, $"Could not find '{gameObjectName}' GameObject");

            var component = gameObject.GetComponentInChildren<T>();
            Assert.NotNull(component, $"Could not find '{typeof(T).Name}' component in GameObject '{gameObject.name}'");

            return component;
        }
    }
}

using System;
using System.Collections;
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
    public class MainMenuPcNavigationTests
    {
        private const string MainMenuScenePath = "Assets/Maroon/scenes/special/MainMenu.pc.unity";

        [UnitySetUp]
        public IEnumerator Setup()
        {
            // Start from Main Menu for every following test
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(MainMenuScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            
            var currentSceneName = SceneManager.GetActiveScene().name;
            Assert.AreEqual("MainMenu.pc", currentSceneName, "'MainMenu.pc' scene did not load");
        }
        
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
            string labsButtonLabel = GetTranslatedString("Menu Lab");
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
            string buttonLabel = GetTranslatedString(source.LabelToTranslate);
            string expectedMenuColumn = source.ExpectedMenuColumn;
            
            GetButtonViaText(buttonLabel).onClick.Invoke();
            yield return null;

            var menuColumn = GameObject.Find(expectedMenuColumn);
            Assert.NotNull(menuColumn, $"Could not find '{buttonLabel}' menu Gameobject '{expectedMenuColumn}'");
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
            public TopLevelMenuPathSource(string labelToTranslateToTranslate, string expectedMenuColumn)
            {
                LabelToTranslate = labelToTranslateToTranslate;
                ExpectedMenuColumn = expectedMenuColumn;
            }

            public string LabelToTranslate { get; }
            public string ExpectedMenuColumn { get; }

            public override string ToString() => $"{LabelToTranslate}";
        }

        // TODO can be removed, don't need general component from object func anymore
        private static T GetComponentFromGameObjectWithName<T>(string gameObjectName)
        {
            var gameObject = GameObject.Find(gameObjectName);
            Assert.NotNull(gameObject, $"Could not find '{gameObjectName}' GameObject");
            
            var component = gameObject.GetComponent<T>();
            Assert.NotNull(component, $"Could not find '{typeof(T).Name}' component in GameObject '{gameObject.name}'");

            return component;
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
        
        // TODO attribute can be removed. the test runner window still shows debug.log messages :(
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        public class DebugLoggerSwitch : Attribute
        {
            public DebugLoggerSwitch(bool enabled)
            {
                Debug.unityLogger.logEnabled = enabled;
            }
        }

        private static string GetTranslatedString(string text)
        {
            var translatedText = LanguageManager.Instance.GetString(text);
            Assert.AreNotEqual(text, translatedText, $"No translation found for {text}");

            return translatedText;
        }
    }
}

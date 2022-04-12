using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

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
            yield return null;
        }

        [UnityTest]
        public IEnumerator LoadExperimentFallingCoilTest()
        {
            GetComponentFromGameObjectWithName<Button>("preMenuButtonLab").onClick.Invoke();
            yield return null;
            
            // TODO idea: do language specific checks as well! (LanguageManager)
            GetButtonViaTextFromAllGameObjectsWithName("Physics", "preMenuButton(Clone)").onClick.Invoke();
            yield return null;
            
            GetButtonViaTextFromAllGameObjectsWithName("FallingCoil", "preMenuButton(Clone)").onClick.Invoke();
            yield return null;
            
            var currentSceneName = SceneManager.GetActiveScene().name;
            Assert.AreEqual("FallingCoil.pc", currentSceneName, "Scene 'FallingCoil.pc' did not load");
            
            yield return null;
        }
        
        // TODO cannot run tests in sequence through test runner :(
        [UnityTest]
        // [TestCaseSource(typeof(ExperimentMenuPaths))] // no return value possible?
        // [TestCaseSource(nameof(_experimentMenuPaths))] // no return value possible?
        [TestCase("Physics", "CoulombsLaw", "CoulombsLaw.pc", ExpectedResult = null)]
        [TestCase("Physics", "FallingCoil", "FallingCoil.pc", ExpectedResult = null)]
        [TestCase("Physics", "FaradaysLaw", "FaradaysLaw.pc", ExpectedResult = null)]
        public IEnumerator LoadExperiment(string category, string experimentName, string sceneName)
        {
            Debug.Log($"Test case called with: {category} {experimentName} {sceneName}");
            GetComponentFromGameObjectWithName<Button>("preMenuButtonLab").onClick.Invoke();
            yield return null;
            
            GetButtonViaTextFromAllGameObjectsWithName(category, "preMenuButton(Clone)").onClick.Invoke();
            yield return null;
            
            GetButtonViaTextFromAllGameObjectsWithName(experimentName, "preMenuButton(Clone)").onClick.Invoke();
            yield return null;
            
            var currentSceneName = SceneManager.GetActiveScene().name;
            Assert.AreEqual(sceneName, currentSceneName, $"Scene '{sceneName}' did not load");
            // TODO cannot access initTestScene here anymore :/
            // Debug.Log("XXXXXXXXXXXXXX " + _initTestScene.name);
            // SceneManager.LoadScene(_initTestScene.name, LoadSceneMode.Single);
            yield return null;
        }
        
        private static object[] _experimentMenuPaths =
        {
            new object[] { "Physics", "CoulombsLaw", "CoulombsLaw.pc" },
            new object[] { "Physics", "FallingCoil", "FallingCoil.pc" },
            new object[] { "Physics", "FaradaysLaw", "FaradaysLaw.pc" }
        };
        
        class ExperimentMenuPaths : IEnumerable
        {
            public IEnumerator GetEnumerator()
            {
                yield return new object[] { "Physics", "CoulombsLaw", "CoulombsLaw.pc", null };
            }
        }
        
        private static T GetComponentFromGameObjectWithName<T>(string gameObjectName)
        {
            var gameObject = GameObject.Find(gameObjectName);
            Assert.NotNull(gameObject, $"Could not find '{gameObjectName}' GameObject");
            
            var component = gameObject.GetComponent<T>();
            Assert.NotNull(component, $"Could not find '{typeof(T).Name}' component in GameObject '{gameObject.name}'");

            return component;
        }
        
        private static Button GetButtonViaTextFromAllGameObjectsWithName(string buttonText, string gameObjectName)
        {
            var buttonList = new List<Button>();

            var gameObjects = Resources.FindObjectsOfTypeAll<GameObject>()
                                                .Where(obj => obj.name == gameObjectName);
            
            foreach (var gameObject in gameObjects)
            {
                var textObject = gameObject.GetComponentInChildren<TextMeshProUGUI>();
                Assert.NotNull(textObject, $"Could not find a 'TextMeshProUGUI' component in GameObject '{gameObject.name}'");
                
                if (textObject.text == buttonText)
                {
                    buttonList.Add(gameObject.GetComponent<Button>());
                }
            }
            
            Assert.True(buttonList.Count > 0, $"Could not find any Button with Text '{buttonText}'");
            Assert.AreEqual(1, buttonList.Count, $"Found more than one Button with Text '{buttonText}'");
            
            return buttonList[0];
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
    }
}

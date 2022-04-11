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

/*
 * Inspired by https://forum.unity.com/threads/play-mode-tests-scenehandling.751049/
 *
 * "End to end" testing
 */

namespace Tests.PlayModeTests
{
    public class MainMenuNavigationTests
    {
        private Scene _initTestScene;
        
        [UnitySetUp]
        public IEnumerator Setup()
        {
            // Store scene in field to keep a reference for cleanup
            _initTestScene = SceneManager.GetActiveScene();
            
            // Load the initial scene for each test
            // TODO Ask Michael if bootstrapping or main menu is the default initial scene
            yield return EditorSceneManager
                .LoadSceneAsyncInPlayMode(
                    "Assets/Maroon/scenes/special/Bootstrapping.pc.unity",
                    new LoadSceneParameters(LoadSceneMode.Single));
        }

        [UnityTest]
        public IEnumerator BootStrappingToMainMenuTest()
        {
            var currentSceneName = SceneManager.GetActiveScene().name;
            Assert.AreEqual("MainMenu.pc", currentSceneName, "Bootstrapping scene did not load Main Menu scene");
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
        
        private T GetComponentFromGameObjectWithName<T>(string gameObjectName)
        {
            var gameObject = GameObject.Find(gameObjectName);
            Assert.NotNull(gameObject, $"Could not find '{gameObjectName}' GameObject");
            
            var component = gameObject.GetComponent<T>();
            Assert.NotNull(component, $"Could not find '{typeof(T).Name}' component in GameObject '{gameObject.name}'");

            return component;
        }
        
        private Button GetButtonViaTextFromAllGameObjectsWithName(string buttonText, string gameObjectName)
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

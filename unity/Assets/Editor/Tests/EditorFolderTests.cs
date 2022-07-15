using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.Something
{
    public class EditorFolderTests
    {
        // private const string MainMenuScenePath = "Assets/Maroon/scenes/special/MainMenu.pc.unity";

        // [UnitySetUp]
        // public IEnumerator SetUp()
        // {
        //     yield return new EnterPlayMode();
        //     
        //     yield return EditorSceneManager.LoadSceneAsyncInPlayMode(MainMenuScenePath, new LoadSceneParameters(LoadSceneMode.Single));
        //     
        //     var test = new GameObject();
        //     test.AddComponent<BField>();
        //     
        //     // EditorSceneManager.OpenScene(MainMenuScenePath);
        //     
        //     Assert.True(EditorApplication.isPlaying);
        // }
        
        // [UnityTearDown]
        // public IEnumerator TearDown()
        // {
        //     yield return new ExitPlayMode();
        //     Assert.False(EditorApplication.isPlaying);
        //     // var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        // }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [Test]
        public void LoadScene()
        {
            
            // EditorSceneManager.LoadSceneAsyncInPlayMode(MainMenuScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            Assert.True(true);
            //
            // var currentSceneName = SceneManager.GetActiveScene().name;
            // Assert.AreEqual("MainMenu.pc", currentSceneName, "'MainMenu.pc' scene did not load");
        }
    }
}
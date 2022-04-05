using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tests.ContentValidation
{
    // Runs OneTimeSetUp and all Tests once for each provided experiment scene path
    [TestFixtureSource(typeof(VrScenesProvider))]
    public class VrSceneValidationTests
    {
        private readonly string _experimentName;
        private readonly string _scenePath;
        private Scene _scene;

        public VrSceneValidationTests(string experimentName, string scenePath)
        {
            _experimentName = experimentName;
            _scenePath = scenePath;
        }
        
        // Load scene if not yet loaded before running the scene's tests
        [OneTimeSetUp]
        public void LoadScene()
        {
            _scene = SceneManager.GetSceneAt(0);
            if (SceneManager.sceneCount > 1 || _scene.path != _scenePath)
                _scene = EditorSceneManager.OpenScene(_scenePath, OpenSceneMode.Single);
        }
        
        [Test]
        public void SceneHasPlayer()
        {
            Assert.That(GameObject.FindWithTag("Player"));
        }

        [Test]
        public void SceneHasIndoorWorld()
        {
            Assert.That(
                GameObject.Find("Indoor World") ||
                GameObject.Find("IndoorWorld"));
        }
        
        [Test]
        public void SceneHasDoorMesh()
        {
            Assert.That(GameObject.Find("mshDoor"));
        }
        
        [Test]
        public void SceneHasWhiteboardInteractive()
        {
            if (_experimentName.Contains("FaradaysLaw"))
                Assert.Ignore("FaradaysLaw scene has intentionally no WhiteboardInteractive");
            Assert.That(GameObject.Find("WhiteboardInteractive"));
        }
        
        [Test]
        public void SceneHasLanguageManager()
        {
            if (_experimentName.Contains("Whiteboard"))
                Assert.Ignore("Whiteboard scene has intentionally no LanguageManager");
            Assert.That(GameObject.Find("LanguageManager"));
        }
        
        [Test]
        public void SceneHasGlobalEntities()
        {
            Assert.That(GameObject.Find("GlobalEntities"));
        }
        
        // Provides experiment names and scene paths to the test fixture
        private class VrScenesProvider : IEnumerable
        {
            private readonly Regex _experimentNameRegex = new Regex(@"\w+\.vr");
        
            IEnumerator IEnumerable.GetEnumerator()
            {
                foreach (var scene in EditorBuildSettings.scenes)
                {
                    // Only return VR scenes from the experiments folder that are enabled in build settings
                    if (scene.enabled && scene.path != null && scene.path.Contains("experiments") && scene.path.EndsWith(".vr.unity"))
                    {
                        // Return the experiment name extracted from its path - this defines test fixture name
                        var experimentName = _experimentNameRegex.Match(scene.path).ToString();
                        yield return new object[] { experimentName, scene.path };
                    }
                }
            }
        }
    }
}

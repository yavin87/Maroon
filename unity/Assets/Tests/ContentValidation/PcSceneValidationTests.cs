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
    [TestFixtureSource(typeof(PcScenesProvider))]
    public class PcSceneValidationTests
    {
        private readonly string _experimentName;
        private readonly string _scenePath;
        private Scene _scene;

        public PcSceneValidationTests(string experimentName, string scenePath)
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
        public void SceneHasMainCamera()
        {
            Assert.That(GameObject.FindWithTag("MainCamera"));
        }
        
        [Test]
        public void SceneHasUICamera()
        {
            Assert.That(GameObject.Find("UICamera"));
        }
        
        [Test]
        public void SceneHasExperimentRoom()
        {
            Assert.That(GameObject.Find("ExperimentRoom"));
        }
        
        [Test]
        public void SceneHasSimulationController()
        {
            if (_experimentName.Contains("Whiteboard"))
                Assert.Ignore("Whiteboard scene has intentionally no SimulationController");
            Assert.That(GameObject.Find("SimulationController"));
        }
        
        [Test]
        public void SceneHasLanguageManager()
        {
            Assert.That(GameObject.Find("LanguageManager"));
        }
        
        [Test]
        public void SceneHasGlobalEntities()
        {
            Assert.That(GameObject.Find("GlobalEntities"));
        }
        
        // Provides experiment names and scene paths to the test fixture
        private class PcScenesProvider : IEnumerable
        {
            readonly Regex _experimentNameRegex = new Regex(@"\w+\.pc");
        
            IEnumerator IEnumerable.GetEnumerator()
            {
                foreach (var scene in EditorBuildSettings.scenes)
                {
                    // Only return PC scenes from the experiments folder that are enabled in build settings
                    if (scene.enabled && scene.path != null && scene.path.Contains("experiments") && scene.path.EndsWith(".pc.unity"))
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

using System.Collections;
using System.Text.RegularExpressions;
using GEAR.Localization;
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
        
        [Test, Description("Must have a Game Object with enabled <Camera> component tagged as 'MainCamera'")]
        public void CheckMainCamera()
        {
            // Check Game Object exists
            var cameraGameObject = GameObject.FindWithTag("MainCamera");
            Assert.NotNull(cameraGameObject, "No 'MainCamera' Game Object found");

            // Check Camera component and its settings
            var cameraComponent = cameraGameObject.GetComponent<Camera>();
            Assert.NotNull(cameraComponent, "No 'Camera' component in Game Object 'MainCamera'");
            
            Assert.True(cameraComponent.enabled, 
                "The 'Camera' component of 'MainCamera' is disabled");
        }
        
        [Test, Description("Must have a Game Object named 'UICamera' with configured <Camera> component")]
        public void CheckUICamera()
        {
            // Check Game Object exists
            var cameraGameObject = GameObject.Find("UICamera");
            Assert.NotNull(cameraGameObject, "No 'UICamera' Game Object found");

            // Check Camera component and its settings
            var cameraComponent = cameraGameObject.GetComponent<Camera>();
            Assert.NotNull(cameraComponent, "No 'Camera' component in Game Object 'UICamera'");
            
            Assert.True(cameraComponent.enabled,
                "The 'Camera' component of 'UICamera' is disabled");
            
            Assert.AreEqual(LayerMask.GetMask("UI"), cameraComponent.cullingMask,
                "Wrong culling mask for 'Camera' component of 'UICamera'");
            
            Assert.True(cameraComponent.orthographic,
                "Wrong projection type for 'Camera' component of 'UICamera'");
        }

        [Test, Description("Must have a Game Object named 'UI' with configured 'Canvas' component")]
        public void CheckUI()
        {
            // Check Game Object exists
            var uiGameObject = GameObject.Find("UI");
            Assert.NotNull(uiGameObject, "No 'UI' Game Object found");
            
            // Check layer
            Assert.AreEqual(LayerMask.GetMask("UI"), uiGameObject.layer);
            
            // Check Canvas component and its settings
            var canvasComponent = uiGameObject.GetComponent<Canvas>();
            Assert.NotNull(canvasComponent, "No 'Canvas' component in Game Object 'UI'");
            Assert.AreEqual(RenderMode.ScreenSpaceCamera,canvasComponent.renderMode);
        }
        
        // TODO stopped here with updating old tests (also VR tests not started updating yet)
        [Test, Description("Experiment scenes must use the ExperimentRoom Prefab")]
        public void SceneHasExperimentRoom()
        {
            var experimentRoomGameObject = GameObject.Find("ExperimentRoom");
            Assert.That(experimentRoomGameObject);
        }
        
        [Test, Description("Experiment scenes must include the SimulationController Prefab")]
        public void SceneHasSimulationController()
        {
            // This scene is an exception
            if (_experimentName.Contains("Whiteboard"))
                Assert.Ignore("Whiteboard scene has intentionally no SimulationController");
            Assert.That(GameObject.Find("SimulationController"));
        }
        
        [Test, Description("Experiment scenes must include the LanguageManager Prefab")]
        public void SceneHasLanguageManager()
        {
            var languageManagerGameObject = GameObject.Find("LanguageManager");
            Assert.That(languageManagerGameObject);
            
            // The package provides an assembly definition, enabling us to directly check for the script component
            var languageManagerScriptComponent = languageManagerGameObject.GetComponent<LanguageManager>();
            Assert.That(languageManagerScriptComponent);
        }
        
        [Test, Description("Experiment scenes must include the GlobalEntities Prefab")]
        public void SceneHasGlobalEntities()
        {
            Assert.That(GameObject.Find("GlobalEntities"));
            var asd = GameObject.Find("GlobalEntities");
            // var dfg = asd.GetComponent<GlobalEntities>();
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

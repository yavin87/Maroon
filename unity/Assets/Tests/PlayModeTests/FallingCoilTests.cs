using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.EndToEnd
{
    public class FallingCoilTests
    {
        [UnitySetUp]
        public IEnumerator Setup()
        {
            yield return EditorSceneManager
                .LoadSceneAsyncInPlayMode(
                    "Assets/Maroon/scenes/experiments/FallingCoil.pc.unity",
                    new LoadSceneParameters(LoadSceneMode.Single));
        }

        [UnityTest]
        public IEnumerator Test1()
        {
            var a = GameObject.Find("a");
            Debug.Assert(a != null);
            Object.Destroy(a);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Test2()
        {
            var a = GameObject.Find("a");
            Debug.Assert(a != null);
            Object.Destroy(a);
            yield return null;
        }
    }
}

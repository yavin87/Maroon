using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    /*
     * Check if its possible to generalize language tests, e.g. get available languages from LM
     * then compare new language objects to objects found with default language
     * for main menu and each submenu
     */
    public class MainMenuLanguageTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void MainMenuLanguageTestsSimplePasses()
        {
            // Use the Assert class to test conditions
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator MainMenuLanguageTestsWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}

using UnityEngine;
using System.IO;

namespace Maroon.GlobalEntities
{
    public class UniumWrapper : UniumComponent, GlobalEntity
    {
        bool openBrowser = true;
        bool openOnce = true;
        public GameObject testBannerPrefab;
        
        public static UniumWrapper Instance => (UniumWrapper)Singleton;

        MonoBehaviour GlobalEntity.Instance => Instance;
        
#if !UNIUM_DISABLE && ( DEVELOPMENT_BUILD || UNITY_EDITOR || UNIUM_ENABLE )
        void Start()
        {
            var assetPath = "TestAutomation/index.html";
            var index = Path.Combine( Application.streamingAssetsPath, assetPath );
            var testBanner = Instantiate(testBannerPrefab, transform, false);

            if( File.Exists( index ) )
            {
                testBanner.gameObject.SetActive(true);
                if( openBrowser && openOnce )
                {
                    openOnce = false;
                    System.Diagnostics.Process.Start( "http://localhost:" + Port + "/" + assetPath );
                }
            }
            else
            {
                testBanner.gameObject.SetActive(false);
                Debug.LogError( "Failed to find file: " + index);
            }
        }
#endif
    }
}
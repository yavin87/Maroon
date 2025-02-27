﻿using GEAR.Localization.Text;
using Maroon.GlobalEntities;
using UnityEngine;
using UnityEngine.UI;

public class scrMenuColumnLaboratorySelectionDetail : MonoBehaviour
{

    // Scenes
    [SerializeField] private Maroon.CustomSceneAsset targetLabScenePC;

    [SerializeField] private Maroon.CustomSceneAsset targetLabSceneVR;

    // Title
    [SerializeField] private GameObject Title;

    // Buttons
    [SerializeField] private GameObject ButtonGo;

    [SerializeField] private GameObject ExperimentButtonsContainer;

    [SerializeField] private GameObject ExperimentButtonPrefab;

    private void Start()
    {
        // Do not init anything if no category is selected, because no lab can be built and no experiments will show up
        if(SceneManager.Instance.ActiveSceneCategory == null)
        {
            return;
        }

        // Update Title
        // TODO: Make this work with localization
        Title.transform.GetComponent<LocalizedTMP>().Key = SceneManager.Instance.ActiveSceneCategory.Name;

        // Link go button action
        this.ButtonGo.GetComponent<Button>().onClick.AddListener(() => this.OnClickGo());

        // Get experiment scenes based on current category
        var scenes = SceneManager.Instance.ActiveSceneCategory.Scenes;
        ButtonGo.transform.Find("ContentContainer").transform.Find("Text (TMP)").GetComponent<LocalizedTMP>().Key =
            "Go to " + SceneManager.Instance.ActiveSceneCategory.Name + " Lab";

        // Create buttons based on category experiments
        for(int iScenes = 0; iScenes < scenes.Length; iScenes++)
        {
            // Extract category
            Maroon.CustomSceneAsset current_scene = scenes[iScenes];

            // Create new button, add to panel and scale
            GameObject newButton = Instantiate(this.ExperimentButtonPrefab, Vector3.zero, Quaternion.identity,
                this.ExperimentButtonsContainer.transform) as GameObject;
            newButton.transform.localScale = Vector3.one;

            // Set text
            // TODO: Make this work with localization
            Transform text = newButton.transform.Find("ContentContainer").transform.Find("Text (TMP)");
            text.GetComponent<LocalizedTMP>().Key = current_scene.SceneNameWithoutPlatformExtension;

            // Link function
            newButton.GetComponent<Button>().onClick.AddListener(() =>
                SceneManager.Instance.LoadSceneRequest(current_scene));
        }
    }

    private void OnClickGo()
    {
        if(PlatformManager.Instance.CurrentPlatformIsVR)
        {
            SceneManager.Instance.LoadSceneRequest(this.targetLabSceneVR);
        }

        else
        {
            SceneManager.Instance.LoadSceneRequest(this.targetLabScenePC);
        }
    }
}

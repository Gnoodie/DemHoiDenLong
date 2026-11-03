using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.EditorTools
{
    /// <summary>
    /// Kiem tra va dam bao 3 scene Build Settings: Main_Menu, LevelSelect, Gameplay.
    /// Dong thoi dam bao rang buoc component (backButton) trong LevelSelect.unity duoc bind bang Unity API.
    /// </summary>
    public static class ValidateSceneNames
    {
        private static readonly string[] RequiredScenes = new string[]
        {
            "Assets/Scenes/Main_Menu.unity",
            "Assets/Scenes/LevelSelect.unity",
            "Assets/Scenes/Gameplay.unity"
        };

        [MenuItem("DemHoiDenLong/Validate Scene Names")]
        public static bool ValidateAndFixSceneNames()
        {
            Debug.Log("[ValidateSceneNames] Checking Build Settings scenes...");
            bool changed = false;
            var currentScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            bool needsRebuild = false;
            if (currentScenes.Count < RequiredScenes.Length)
            {
                needsRebuild = true;
            }
            else
            {
                for (int i = 0; i < RequiredScenes.Length; i++)
                {
                    if (currentScenes[i].path != RequiredScenes[i] || !currentScenes[i].enabled)
                    {
                        needsRebuild = true;
                        break;
                    }
                }
            }

            if (needsRebuild)
            {
                Debug.LogWarning("[ValidateSceneNames] Build Settings scenes mismatch or disabled. Fixing...");
                var newScenes = new List<EditorBuildSettingsScene>();
                for (int i = 0; i < RequiredScenes.Length; i++)
                {
                    newScenes.Add(new EditorBuildSettingsScene(RequiredScenes[i], true));
                }

                for (int i = 0; i < currentScenes.Count; i++)
                {
                    bool exists = false;
                    for (int j = 0; j < RequiredScenes.Length; j++)
                    {
                        if (currentScenes[i].path == RequiredScenes[j])
                        {
                            exists = true;
                            break;
                        }
                    }
                    if (!exists)
                    {
                        newScenes.Add(currentScenes[i]);
                    }
                }

                EditorBuildSettings.scenes = newScenes.ToArray();
                changed = true;
                Debug.Log("[ValidateSceneNames] Updated EditorBuildSettings.scenes successfully.");
            }
            else
            {
                Debug.Log("[ValidateSceneNames] EditorBuildSettings.scenes is VALID and correct (0: Main_Menu, 1: LevelSelect, 2: Gameplay).");
            }

            BindLevelSelectSceneReferences();

            return true;
        }

        public static void BindLevelSelectSceneReferences()
        {
            string scenePath = "Assets/Scenes/LevelSelect.unity";
            var currentActive = EditorSceneManager.GetActiveScene();
            bool openedTemp = false;

            UnityEngine.SceneManagement.Scene levelSelectScene;
            if (currentActive.path == scenePath)
            {
                levelSelectScene = currentActive;
            }
            else
            {
                levelSelectScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                openedTemp = true;
            }

            bool sceneDirty = false;
            var actSelectView = Object.FindAnyObjectByType<DemHoiDenLong.UI.ActSelectView>();
            var levelSelectFlow = Object.FindAnyObjectByType<DemHoiDenLong.UI.LevelSelectFlow>();

            if (actSelectView != null)
            {
                var so = new SerializedObject(actSelectView);
                var backBtnProp = so.FindProperty("backButton");
                if (backBtnProp != null && backBtnProp.objectReferenceValue == null)
                {
                    var backBtnObj = GameObject.Find("BackBtn") ?? GameObject.Find("BackButton");
                    if (backBtnObj == null)
                    {
                        Transform t = actSelectView.transform.Find("SafeAreaRoot/HeaderBar/BackBtn")
                                   ?? actSelectView.transform.Find("SafeAreaRoot/HeaderBar/BackButton")
                                   ?? actSelectView.transform.Find("HeaderBar/BackBtn")
                                   ?? actSelectView.transform.Find("HeaderBar/BackButton");
                        if (t != null) backBtnObj = t.gameObject;
                    }

                    if (backBtnObj != null)
                    {
                        var btn = backBtnObj.GetComponent<UnityEngine.UI.Button>();
                        if (btn != null)
                        {
                            backBtnProp.objectReferenceValue = btn;
                            so.ApplyModifiedProperties();
                            EditorUtility.SetDirty(actSelectView);
                            sceneDirty = true;
                            Debug.Log("[ValidateSceneNames] Successfully bound backButton on ActSelectView via SerializedObject API.");
                        }
                    }
                }
            }

            if (levelSelectFlow != null)
            {
                var soFlow = new SerializedObject(levelSelectFlow);
                var actViewCtrlProp = soFlow.FindProperty("actSelectViewController");
                if (actViewCtrlProp != null && actViewCtrlProp.objectReferenceValue == null && actSelectView != null)
                {
                    actViewCtrlProp.objectReferenceValue = actSelectView;
                    soFlow.ApplyModifiedProperties();
                    EditorUtility.SetDirty(levelSelectFlow);
                    sceneDirty = true;
                    Debug.Log("[ValidateSceneNames] Successfully bound actSelectViewController on LevelSelectFlow via SerializedObject API.");
                }
            }

            if (sceneDirty)
            {
                EditorSceneManager.SaveScene(levelSelectScene);
                Debug.Log("[ValidateSceneNames] Saved LevelSelect.unity.");
            }

            if (openedTemp && !string.IsNullOrEmpty(currentActive.path) && currentActive.path != scenePath)
            {
                EditorSceneManager.OpenScene(currentActive.path, OpenSceneMode.Single);
            }
        }
    }
}

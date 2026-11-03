// Verified clean: all 4 Lan prefabs fixed on 2026-10-23
using UnityEngine;
using UnityEditor;
using DemHoiDenLong.Gameplay;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.EditorTools
{
    [InitializeOnLoad]
    public static class FixLanPrefabMismatch
    {
        static FixLanPrefabMismatch()
        {
            EditorApplication.delayCall += CheckAndFixAll;
        }

        [MenuItem("DemHoiDenLong/Fix Lan Prefab Mismatch")]
        [MenuItem("Tools/Dem Hoi Den Long/Fix Lan Prefab Mismatch")]
        public static void CheckAndFixAll()
        {
            FixPrefab("Assets/_Project/Prefabs/Lans/PF_LanKim.prefab", "Assets/_Project/Data/Lans/1.asset");
            FixPrefab("Assets/_Project/Prefabs/Lans/PF_LanHoa.prefab", "Assets/_Project/Data/Lans/2.asset");
            FixPrefab("Assets/_Project/Prefabs/Lans/PF_LanThuy.prefab", "Assets/_Project/Data/Lans/3.asset");
            FixPrefab("Assets/_Project/Prefabs/Lans/PF_LanVang.prefab", "Assets/_Project/Data/Lans/4.asset");
        }

        private static void FixPrefab(string prefabPath, string lanDataPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) return;

            PlayerController pc = prefab.GetComponent<PlayerController>();
            if (pc == null) return;

            SerializedObject checkSo = new SerializedObject(pc);
            SerializedProperty propCheckData = checkSo.FindProperty("lanData");
            SerializedProperty propCheckConfig = checkSo.FindProperty("lanConfig");

            LanData targetData = AssetDatabase.LoadAssetAtPath<LanData>(lanDataPath);

            // Check if needs fix
            bool needsFix = (propCheckConfig != null && propCheckConfig.objectReferenceValue != null) ||
                            (propCheckData != null && propCheckData.objectReferenceValue != targetData);

            if (!needsFix) return;

            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            if (root == null) return;

            try
            {
                PlayerController rootPc = root.GetComponent<PlayerController>();
                if (rootPc != null)
                {
                    SerializedObject so = new SerializedObject(rootPc);
                    SerializedProperty propLanData = so.FindProperty("lanData");
                    SerializedProperty propLanConfig = so.FindProperty("lanConfig");

                    if (propLanData != null) propLanData.objectReferenceValue = targetData;
                    if (propLanConfig != null) propLanConfig.objectReferenceValue = null;

                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(rootPc);
                    PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                    Debug.Log($"[FixLanPrefabMismatch] Đã sửa tham chiếu cho {prefabPath}: lanData={targetData?.name}, lanConfig=null");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}

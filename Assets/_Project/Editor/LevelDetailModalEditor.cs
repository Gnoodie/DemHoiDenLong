using UnityEditor;
using UnityEngine;
using DemHoiDenLong.UI;

namespace DemHoiDenLong.Editor
{
    [CustomEditor(typeof(LevelDetailModal))]
    public class LevelDetailModalEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            LevelDetailModal modal = (LevelDetailModal)target;

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Quick Test Mocks (Xem nhanh giao diện)", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUI.backgroundColor = new Color(0.4f, 0.9f, 0.7f);
            if (GUILayout.Button("⭐ Màn 2-3 (Màn 8: 0 sao, nhãn MỚI, CHƯA ĐẠT)", GUILayout.Height(32)))
            {
                modal.Test_Act2_Node3_Demo();
                EditorUtility.SetDirty(modal);
            }
            if (GUILayout.Button("⭐ Màn 2-2 (Màn 7: 2 sao, 2 ĐÃ NHẬN, 1 CHƯA ĐẠT)", GUILayout.Height(28)))
            {
                modal.Test_Act2_Node2_Demo();
                EditorUtility.SetDirty(modal);
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(6);

            if (GUILayout.Button("1. Màn thường 0 sao (Chưa đạt)"))
            {
                modal.Test_Normal_0Star();
                EditorUtility.SetDirty(modal);
            }

            if (GUILayout.Button("2. Màn thường 2 sao (Mốc 1 & 2 đã nhận)"))
            {
                modal.Test_Normal_2Star();
                EditorUtility.SetDirty(modal);
            }

            if (GUILayout.Button("3. Màn thường 3 sao (Cả 3 mốc đã nhận)"))
            {
                modal.Test_Normal_3Star();
                EditorUtility.SetDirty(modal);
            }

            EditorGUILayout.Space(4);

            if (GUILayout.Button("4. Màn Boss 0 sao (Mốc 3 Kim Cương chưa đạt)"))
            {
                modal.Test_Boss_0Star();
                EditorUtility.SetDirty(modal);
            }

            if (GUILayout.Button("5. Màn Boss 3 sao (Đã nhận Kim Cương)"))
            {
                modal.Test_Boss_3Star();
                EditorUtility.SetDirty(modal);
            }

            EditorGUILayout.Space(4);

            GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);
            if (GUILayout.Button("Đóng Popup"))
            {
                modal.Close();
                EditorUtility.SetDirty(modal);
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndVertical();
        }
    }
}

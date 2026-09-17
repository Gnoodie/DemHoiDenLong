using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using DemHoiDenLong.Data;

namespace DemHoiDenLong.EditorTools
{
    public class CSVImporter : EditorWindow
    {
        private string csvFilePath = "";

        [MenuItem("Tools/Dem Hoi Den Long/Import Data from CSV")]
        public static void ShowWindow()
        {
            GetWindow<CSVImporter>("Import Data");
        }

        private void OnGUI()
        {
            GUILayout.Label("Import Lan Config from CSV", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Lưu sheet 'Lân - Nhân vật' từ Excel sang CSV UTF-8, sau đó nhấn Browse để chọn file.\n" +
                "File có thể nằm ngoài thư mục Assets.\n" +
                "Tên cột bắt buộc trong CSV (hàng đầu tiên):\n" +
                "LanId | LanName | LanNickname | UnlockCostStar | UnlockCostDiamond\n" +
                "BaseHp | BaseSpeed | BaseDamage | BaseFireRate\n" +
                "SkillName | SkillDescription | SkillDamage | SkillCooldown\n" +
                "StoryDescription | LanRole",
                MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField("File CSV", string.IsNullOrEmpty(csvFilePath) ? "(chưa chọn)" : csvFilePath);
            if (GUILayout.Button("Browse...", GUILayout.Width(80)))
            {
                string picked = EditorUtility.OpenFilePanel("Chọn file CSV", "d:/Dev_Game", "csv");
                if (!string.IsNullOrEmpty(picked)) csvFilePath = picked;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(csvFilePath));
            if (GUILayout.Button("Import Lan Configs"))
            {
                string content = File.ReadAllText(csvFilePath, System.Text.Encoding.UTF8);
                ImportLanConfigs(content);
            }
            EditorGUI.EndDisabledGroup();
        }

        private void ImportLanConfigs(string csvData)
{
    List<string[]> rows = ParseCSV(csvData);
    if (rows.Count <= 1)
    {
        EditorUtility.DisplayDialog("Error", "CSV trống hoặc không có dữ liệu.", "OK");
        return;
    }

    string[] headers = rows[0];
    int imported = 0;
    int skipped = 0;

    if (!AssetDatabase.IsValidFolder("Assets/_Project/Data"))
        AssetDatabase.CreateFolder("Assets/_Project", "Data");
    if (!AssetDatabase.IsValidFolder("Assets/_Project/Data/Lans"))
        AssetDatabase.CreateFolder("Assets/_Project/Data", "Lans");

    for (int i = 1; i < rows.Count; i++)
    {
        string[] values = rows[i];
        string lanId = GetValue(headers, values, "LanId").Trim();

        if (string.IsNullOrEmpty(lanId)) { skipped++; continue; }

        string assetPath = $"Assets/_Project/Data/Lans/{lanId}.asset";
        LanConfig config = AssetDatabase.LoadAssetAtPath<LanConfig>(assetPath);

        bool isNew = config == null;
        if (isNew) config = ScriptableObject.CreateInstance<LanConfig>();

        config.LanId   = lanId;
        config.LanName = GetValue(headers, values, "LanName");

        int.TryParse(GetValue(headers, values, "UnlockCostStar"),    out config.UnlockCostStar);
        int.TryParse(GetValue(headers, values, "UnlockCostDiamond"), out config.UnlockCostDiamond);

        int.TryParse(  GetValue(headers, values, "BaseHp"),       out config.BaseHp);
        float.TryParse(GetValue(headers, values, "BaseSpeed"),     out config.BaseSpeed);
        int.TryParse(  GetValue(headers, values, "BaseDamage"),    out config.BaseDamage);
        float.TryParse(GetValue(headers, values, "BaseFireRate"),  out config.BaseFireRate);

        config.lanNickname = GetValue(headers, values, "LanNickname");

        config.SkillName        = GetValue(headers, values, "SkillName");
        config.SkillDescription = GetValue(headers, values, "SkillDescription");
        int.TryParse(  GetValue(headers, values, "SkillDamage"),   out config.SkillDamage);
        float.TryParse(GetValue(headers, values, "SkillCooldown"), out config.SkillCooldown);

        string storyDesc = GetValue(headers, values, "StoryDescription");
        if (!string.IsNullOrEmpty(storyDesc))
            config.storyDescription = storyDesc;

        string lanRole = GetValue(headers, values, "LanRole");
        if (!string.IsNullOrEmpty(lanRole))
            config.lanRole = lanRole;

        if (isNew) AssetDatabase.CreateAsset(config, assetPath);
        EditorUtility.SetDirty(config);
        imported++;
    }

    AssetDatabase.SaveAssets();
    AssetDatabase.Refresh();
    EditorUtility.DisplayDialog("Hoàn tất",
        $"✅ Đã import: {imported} Lân\n⏭ Bỏ qua (không có LanId): {skipped}", "OK");
}

// Parser toàn file: tôn trọng quote xuyên suốt file, xử lý được field chứa
// dấu phẩy VÀ xuống dòng thật bên trong "...", cũng như "" (escaped quote)
private static List<string[]> ParseCSV(string content)
{
    var rows = new List<string[]>();
    var current = new List<string>();
    var field = new System.Text.StringBuilder();
    bool inQuotes = false;
    int i = 0;
    int len = content.Length;

    // Bỏ BOM nếu còn sót
    if (len > 0 && content[0] == '\uFEFF') i = 1;

    while (i < len)
    {
        char c = content[i];

        if (inQuotes)
        {
            if (c == '"')
            {
                if (i + 1 < len && content[i + 1] == '"') { field.Append('"'); i += 2; continue; }
                inQuotes = false;
                i++;
                continue;
            }
            field.Append(c);
            i++;
            continue;
        }

        if (c == '"') { inQuotes = true; i++; continue; }

        if (c == ',')
        {
            current.Add(field.ToString().Trim());
            field.Clear();
            i++;
            continue;
        }

        if (c == '\r' || c == '\n')
        {
            // Kết thúc 1 record (chỉ khi ngoài quote)
            current.Add(field.ToString().Trim());
            field.Clear();

            bool isEmptyRow = current.TrueForAll(string.IsNullOrEmpty);
            if (!isEmptyRow) rows.Add(current.ToArray());
            current = new List<string>();

            // Bỏ qua \r\n như 1 cặp
            if (c == '\r' && i + 1 < len && content[i + 1] == '\n') i++;
            i++;
            continue;
        }

        field.Append(c);
        i++;
    }

    // Field/record cuối cùng nếu file không kết thúc bằng newline
    if (field.Length > 0 || current.Count > 0)
    {
        current.Add(field.ToString().Trim());
        if (!current.TrueForAll(string.IsNullOrEmpty)) rows.Add(current.ToArray());
    }

    return rows;
}

private static string GetValue(string[] headers, string[] values, string columnName)
{
    for (int i = 0; i < headers.Length; i++)
    {
        if (headers[i].Trim().Equals(columnName, System.StringComparison.OrdinalIgnoreCase))
            return i < values.Length ? values[i] : "";
    }
    return "";
}

    }
}

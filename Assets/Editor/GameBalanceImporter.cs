#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using DemHoiDenLong.Data;
using DemHoiDenLong.Gameplay;

namespace DemHoiDenLong.EditorTools
{
    public class GameBalanceImporter : EditorWindow
    {
        [MenuItem("DemHoiDenLong/Import Balance Data (CSV)")]
        public static void ShowWindow()
        {
            GetWindow<GameBalanceImporter>("CSV Importer");
        }

        private void OnGUI()
        {
            GUILayout.Label("Game Balancing CSV Importer", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("Import Lan Data"))
            {
                string path = EditorUtility.OpenFilePanel("Select Lan CSV", Application.dataPath, "csv");
                if (!string.IsNullOrEmpty(path))
                {
                    ImportLanData(path);
                }
            }
            
            if (GUILayout.Button("Import Enemy Data"))
            {
                string path = EditorUtility.OpenFilePanel("Select Enemy CSV", Application.dataPath, "csv");
                if (!string.IsNullOrEmpty(path))
                {
                    ImportEnemyData(path);
                }
            }
            
            if (GUILayout.Button("Import Level Data"))
            {
                string path = EditorUtility.OpenFilePanel("Select Level CSV", Application.dataPath, "csv");
                if (!string.IsNullOrEmpty(path))
                {
                    ImportLevelData(path);
                }
            }
        }

        private string ReadFileSafe(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs, System.Text.Encoding.UTF8))
            {
                return sr.ReadToEnd();
            }
        }

        private void ImportLanData(string filePath)
        {
            string content = ReadFileSafe(filePath);
            List<string[]> rows = ParseCSV(content);

            int count = 0;
            // Skip header (row 0)
            for (int i = 1; i < rows.Count; i++)
            {
                string[] cols = rows[i];
                if (cols.Length < 14) continue;
                if (string.IsNullOrWhiteSpace(cols[0])) continue;

                string id = cols[0].Trim();
                string lanName = cols[1].Trim();
                
                string assetPath = $"Assets/_Project/Data/Lans/{id}.asset";
                LanData data = AssetDatabase.LoadAssetAtPath<LanData>(assetPath);
                bool isNew = false;
                
                if (data == null)
                {
                    data = ScriptableObject.CreateInstance<LanData>();
                    isNew = true;
                }

                data.lanId = id;
                data.lanName = lanName;
                
                int.TryParse(cols[2], out data.unlockCostStars);
                int.TryParse(cols[3], out data.unlockCostGems);
                float.TryParse(cols[4], out data.maxHp);
                float.TryParse(cols[5], out data.moveSpeed);
                float.TryParse(cols[6], out data.baseDamage);
                float.TryParse(cols[7], out data.fireRate);
                
                data.skillName = cols[8];
                float.TryParse(cols[9], out data.skillDamage);
                float.TryParse(cols[10], out data.skillCooldown);

                if (isNew)
                {
                    AssetDatabase.CreateAsset(data, assetPath);
                }
                
                EditorUtility.SetDirty(data);
                count++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"<color=green>Imported {count} Lan Data assets.</color>");
        }

        private void ImportEnemyData(string filePath)
        {
            string content = ReadFileSafe(filePath);
            List<string[]> rows = ParseCSV(content);

            int count = 0;
            // The enemy data seems to have headers at row 2 (0-indexed). Data starts at row 3.
            for (int i = 3; i < rows.Count; i++)
            {
                string[] cols = rows[i];
                if (cols.Length < 6) continue;
                if (string.IsNullOrWhiteSpace(cols[0])) continue;

                string enemyName = cols[0].Trim();
                string safeName = enemyName.Replace(" ", "").Replace("(", "").Replace(")", "");
                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    safeName = safeName.Replace(c.ToString(), "");
                }
                // also explicitly remove colon if somehow missed
                safeName = safeName.Replace(":", "");
                
                string dirPath = "Assets/_Project/Data/Enemies";
                if (!AssetDatabase.IsValidFolder(dirPath))
                {
                    AssetDatabase.CreateFolder("Assets/_Project/Data", "Enemies");
                }
                
                string assetPath = $"{dirPath}/{safeName}.asset";
                EnemyData data = AssetDatabase.LoadAssetAtPath<EnemyData>(assetPath);
                bool isNew = false;
                
                if (data == null)
                {
                    data = ScriptableObject.CreateInstance<EnemyData>();
                    isNew = true;
                }

                data.enemyName = enemyName;
                data.enemyId = safeName.ToLower();

                float hp, speed, dmg;
                int stars;
                float.TryParse(cols[2], out hp);
                float.TryParse(cols[3], out speed);
                float.TryParse(cols[4], out dmg);
                int.TryParse(cols[5], out stars);

                data.maxHp = hp > 0 ? hp : data.maxHp;
                data.moveSpeed = speed > 0 ? speed : data.moveSpeed;
                data.damage = dmg > 0 ? dmg : data.damage;
                data.starReward = stars > 0 ? stars : data.starReward;

                if (isNew)
                {
                    AssetDatabase.CreateAsset(data, assetPath);
                }
                
                EditorUtility.SetDirty(data);
                count++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"<color=green>Imported {count} Enemy Data assets.</color>");
        }

        private void ImportLevelData(string filePath)
        {
            string content = ReadFileSafe(filePath);
            List<string[]> rows = ParseCSV(content);

            int count = 0;
            // The level data seems to have the actual table starting from line 11 (index 10 is headers)
            for (int i = 11; i < rows.Count; i++)
            {
                string[] cols = rows[i];
                if (cols.Length < 5) continue;
                if (string.IsNullOrWhiteSpace(cols[0])) continue;

                string levelNumStr = cols[0].Trim();
                if (!int.TryParse(levelNumStr, out int levelNum)) continue;

                string assetPath = $"Assets/_Project/Data/Levels/SO_Level_{levelNum:D2}.asset";
                LevelData data = AssetDatabase.LoadAssetAtPath<LevelData>(assetPath);
                
                if (data == null)
                {
                    Debug.LogWarning($"Could not find {assetPath}. Please generate Level Data assets first.");
                    continue;
                }

                float.TryParse(cols[1], out data.baseHpMultiplier);
                float.TryParse(cols[2], out data.baseSpeedMultiplier);
                float.TryParse(cols[3], out data.baseDamageMultiplier);
                
                // If the CSV has commas or percentage signs, we should parse it cleanly, but float.TryParse handles standard decimals.
                // In CSV, they are "1.00", "1.08", so float.TryParse works.

                EditorUtility.SetDirty(data);
                count++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"<color=green>Imported {count} Level Data assets.</color>");
        }

        private static List<string[]> ParseCSV(string text)
        {
            List<string[]> result = new List<string[]>();
            bool inQuotes = false;
            string currentValue = "";
            List<string> currentRow = new List<string>();

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                
                if (c == '\"')
                {
                    if (inQuotes && i + 1 < text.Length && text[i + 1] == '\"')
                    {
                        currentValue += '\"';
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    currentRow.Add(currentValue.Trim());
                    currentValue = "";
                }
                else if ((c == '\n' || c == '\r') && !inQuotes)
                {
                    if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                    {
                        i++;
                    }
                    currentRow.Add(currentValue.Trim());
                    result.Add(currentRow.ToArray());
                    currentRow = new List<string>();
                    currentValue = "";
                }
                else
                {
                    currentValue += c;
                }
            }
            
            if (currentRow.Count > 0 || currentValue != "")
            {
                currentRow.Add(currentValue.Trim());
                result.Add(currentRow.ToArray());
            }

            return result;
        }
    }
}
#endif

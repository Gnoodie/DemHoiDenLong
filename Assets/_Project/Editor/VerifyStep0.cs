using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DemHoiDenLong.Data;
using DemHoiDenLong.UI;

namespace DemHoiDenLong.EditorTools
{
    public static class VerifyStep0
    {
        [MenuItem("DemHoiDenLong/Step 0 Verification")]
        public static void RunVerification()
        {
            Debug.Log("=================================================");
            Debug.Log("            BƯỚC 0: BÁO CÁO XÁC MINH             ");
            Debug.Log("=================================================");

            // 0b: Tìm các asset ActData và LevelDatabase
            string[] actGuids = AssetDatabase.FindAssets("t:ActData");
            Debug.Log($"[0b] Tìm thấy {actGuids.Length} asset ActData:");
            foreach (var guid in actGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ActData act = AssetDatabase.LoadAssetAtPath<ActData>(path);
                if (act != null)
                {
                    string bannerName = act.banner != null ? act.banner.name : "NULL";
                    string mapBgName = act.mapBackground != null ? act.mapBackground.name : "NULL";
                    Debug.Log($"   • Path: {path} | ActIndex: {act.actIndex} | Title: '{act.title}' | Banner: '{bannerName}' | MapBg: '{mapBgName}' | Levels: {act.levels?.Count ?? 0}");
                }
            }

            string[] dbGuids = AssetDatabase.FindAssets("t:LevelDatabase");
            Debug.Log($"[0b] Tìm thấy {dbGuids.Length} asset LevelDatabase:");
            foreach (var guid in dbGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                LevelDatabase db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(path);
                Debug.Log($"   • Path: {path} | Acts Count: {db?.acts?.Count ?? 0} | Total Levels: {db?.TotalLevels ?? 0}");
            }

            // Boss sprites check
            Debug.Log("[0b] Kiểm tra ảnh Boss:");
            string[] bossPaths = new string[]
            {
                "Assets/_Project/Art/Bosses/boss_act1.png",
                "Assets/_Project/Art/Bosses/boss_act2.png",
                "Assets/_Project/Art/Bosses/boss_act3.png",
                "Assets/_Project/Art/LevelSelect/node_boss_act1.png",
                "Assets/_Project/Art/LevelSelect/node_boss_act2.png",
                "Assets/_Project/Art/LevelSelect/node_boss_act3.png"
            };
            foreach (var bp in bossPaths)
            {
                Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(bp);
                Debug.Log($"   • {bp}: {(sp != null ? $"OK (Sprite: {sp.name}, rect: {sp.rect.width}x{sp.rect.height})" : "NULL")}");
            }

            // 0c: Kiểm tra LanData
            string[] lanGuids = AssetDatabase.FindAssets("t:LanData");
            Debug.Log($"[0c] Tìm thấy {lanGuids.Length} asset LanData:");
            foreach (var guid in lanGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                LanData lan = AssetDatabase.LoadAssetAtPath<LanData>(path);
                Debug.Log($"   • Path: {path} | LanId: '{lan.lanId}' | Name: '{lan.lanName}' | Icon: {(lan.icon != null ? lan.icon.name : "NULL")}");
            }
            Debug.Log("=================================================");
        }
    }
}

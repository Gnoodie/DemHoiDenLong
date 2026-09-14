using UnityEngine;
using System.IO;
using DemHoiDenLong.Data;
using System;

namespace DemHoiDenLong.Meta
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }
        
        public PlayerData CurrentData { get; private set; }
        
        private string SaveFilePath => Application.persistentDataPath + "/playerData.json";

        public event Action OnDataLoaded;
        public event Action OnDataSaved;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                if (Application.isPlaying)
                    Destroy(gameObject);
                else
                    DestroyImmediate(gameObject);
                return;
            }
            
            Instance = this;
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
            
            LoadData();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void LoadData()
        {
            if (File.Exists(SaveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(SaveFilePath);
                    CurrentData = JsonUtility.FromJson<PlayerData>(json);
                    
                    if (CurrentData == null)
                    {
                        Debug.LogWarning("Failed to parse save file. Creating new data.");
                        CreateNewData();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error loading data: {e.Message}");
                    CreateNewData();
                }
            }
            else
            {
                CreateNewData();
            }
            
            OnDataLoaded?.Invoke();
        }

        public void SaveData()
        {
            if (CurrentData == null) return;
            
            try
            {
                string json = JsonUtility.ToJson(CurrentData, true);
                File.WriteAllText(SaveFilePath, json);
                OnDataSaved?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving data: {e.Message}");
            }
        }

        private void CreateNewData()
        {
            CurrentData = new PlayerData();
            SaveData();
        }
        
        // Phương thức hỗ trợ xoá data, hữu ích khi test hoặc có nút "Reset Account" trong game
        public void DeleteSaveData()
        {
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
            }
            CreateNewData();
        }
    }
}

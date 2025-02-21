using Newtonsoft.Json;
using NUnit.Framework;
using Unity.Services.CloudSave.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Unity.VisualScripting;

[System.Serializable]
public class Data
{
    public string playerName;
    public int level;
    public int Wave;

    public Data(string playerName, int level, int wave)
    {
        this.playerName = playerName;
        this.level = level;
        this.Wave = wave;
    }
}

public class Cloud_Mng : MonoBehaviour
{
    public Data m_Data;
    public Data Other_Data;
    public static Cloud_Mng instance = null;
    private float timer = 0.0f;
    bool isSaving = false;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;

            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Update()
    {
        if (!isSaving)
        {
            timer += Time.unscaledDeltaTime;
            if (timer >= 10.0f)
            {
                timer = 0.0f;
                Save(() => isSaving = false);
            }
        }
    }

    public async void Save(Action action = null)
    {
        isSaving = true;
        await SavePlayerData(m_Data);
        if(action != null)
        {
            action?.Invoke();
        }
    }

    private const string playerDataKey = "PlayerData";
    public async Task SavePlayerData(Data m_Data)
    {
        try
        {
            string jsonData = JsonConvert.SerializeObject(m_Data);
            var data = new Dictionary<string, object> { { playerDataKey, jsonData } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(data);
            Debug.Log("플레이어 데이터가 저장되었습니다.");
        }
        catch(System.Exception e)
        {
            Debug.LogError("SavePlayerData Error: " + e.Message);
        }
    }

    public async Task<Data> LoadPlayerData()
    {
        try
        {
            var savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { playerDataKey });

            if(savedData.TryGetValue(playerDataKey, out Item item))
            {
                string jsonString = item.Value.GetAs<string>();
                Data loadedData = JsonConvert.DeserializeObject<Data>(jsonString);

                return loadedData;
            }
            else
            {
                Debug.LogWarning("저장된 플레이어 데이터가 없습니다.");
                return new Data("", 1, 0);
            }
        }
        catch(System.Exception e)
        {
            Debug.LogError("LoadPlayerData Error: " + e.Message);
        }
        return null;
    }
 
}

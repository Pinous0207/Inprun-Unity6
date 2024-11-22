using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class Hero_Holder : NetworkBehaviour
{
    [SerializeField] private Hero _spawn_Hero;
    [SerializeField] Transform Circle_Range;
    [SerializeField] Transform SetClick;
    [SerializeField] Transform GetClick;
    [SerializeField] private GameObject CanvasObject;

    public string Holder_Part_Name;
    public string Holder_Name;
    public List<Hero> m_Heroes = new List<Hero>();
    public int index;
    HeroData m_Data;

    public readonly Vector2[] One = { Vector2.zero };
    public readonly Vector2[] Two =
        {
        new Vector2(-0.1f, 0.05f),
        new Vector2(0.1f, -0.1f)
    };
    public readonly Vector2[] Three =
        {
        new Vector2(-0.1f, 0.1f),
        new Vector2(0.1f, -0.05f),
        new Vector2(-0.15f, -0.15f)
    };

    public Button SellButton, CompositionButton;

    private void Start()
    {
        MakeCollider();

        SellButton.onClick.AddListener(() => Sell());
        CompositionButton.onClick.AddListener(() => Composition());
    }

    #region SELL
    private void Sell()
    {
        Net_Utils.HostAndClientMethod(
            () => SellServerRpc(Net_Utils.LocalID()),
            () => SellCharacter(Net_Utils.LocalID()));
    }

    [ServerRpc(RequireOwnership = false)]
    private void SellServerRpc(ulong clientID)
    {
        SellCharacter(clientID);
    }

    private void SellCharacter(ulong clientID)
    {
        var hero = m_Heroes[m_Heroes.Count - 1];
        ulong heroId = hero.NetworkObjectId;
        NetworkObject obj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[heroId];
        SellClientRpc(heroId, clientID);
        obj.Despawn();
    }

    [ClientRpc]
    private void SellClientRpc(ulong heroKey, ulong clientID)
    {
        var obj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[heroKey];
        m_Heroes.Remove(obj.GetComponent<Hero>());
        if (m_Heroes.Count == 0)
        {
            DestroyServerRpc(clientID);
        }
        CheckGetPosition();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyServerRpc(ulong clientID)
    {
        DestroyClientRpc(clientID);
        NetworkObject holderObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[NetworkObjectId];
        holderObj.Despawn();
    }

    [ClientRpc]
    private void DestroyClientRpc(ulong clientID)
    {
        Spawner.instance.Hero_Holders.Remove(Holder_Part_Name);
        if (Net_Utils.IsClientCheck(clientID))
        {
            Spawner.Player_spawn_list_Array[index] = false;
        }
        else
        {
            Spawner.Other_spawn_list_Array[index] = false;
        }
    }
    #endregion

    public void Composition()
    {
        List<Hero_Holder> holders = new List<Hero_Holder>();

        holders.Add(this);

        foreach(var data in Spawner.instance.Hero_Holders)
        {
            if(data.Value.Holder_Name == Holder_Name && data.Value != this)
            {
                string temp = Net_Utils.LocalID() == (ulong)0 ? "HOST" : "CLIENT";
                
                if(data.Value.Holder_Part_Name.Contains(temp))
                    holders.Add(data.Value);
            }
        }
        int cnt = 0;
        string[] holderTemp = new string[2];
        bool GetBreak = false;
        for(int i = 0; i < holders.Count; i++)
        {
            for (int j = 0; j < holders[i].m_Heroes.Count; j++)
            {
                if (holders[i].m_Heroes.Count > 0)
                {
                    holderTemp[cnt] = holders[i].Holder_Part_Name;
                    cnt++;
                    if (cnt >= 2)
                    {
                        GetBreak = true;
                        break;
                    }
                }
            }
            if (GetBreak) break;
        }
        for (int i = 0; i < holderTemp.Length; i++)
        {
            if (holderTemp[i] == "" || holderTemp[i] == null)
            {
                Debug.Log("합성에 필요한 영웅이 부족합니다.");
                return;
            }

        }
        for (int i = 0; i < holderTemp.Length; i++) Spawner.instance.Hero_Holders[holderTemp[i]].Sell();

        Spawner.instance.Summon("UnCommon");
    }

    public void HeroChange(Hero_Holder holder)
    {
        List<Vector2> poss = new List<Vector2>();
        switch(m_Heroes.Count)
        {
            case 1: poss = new List<Vector2>(One); break;
            case 2: poss = new List<Vector2>(Two); break;
            case 3: poss = new List<Vector2>(Three); break;
        }

        for(int i = 0; i < poss.Count; i++)
        {
            Vector2 worldPosition = holder.transform.TransformPoint(poss[i]);
            poss[i] = worldPosition;
        }

        for (int i = 0; i < m_Heroes.Count; i++)
        {
            m_Heroes[i].Position_Change(holder, poss, i);
        }
    }

    public void G_GetClick(bool Active)
    {
        GetClick.gameObject.SetActive(Active);
    }
    public void S_SetClick(bool Active)
    {
        SetClick.gameObject.SetActive(Active);
    }

    public void GetRange()
    {
        float range = m_Data.heroRange * 2;
        Circle_Range.localScale = new Vector2(range, range);

        Circle_Range.gameObject.SetActive(true);
        CanvasObject.SetActive(true);
    }
    public void ReturnRange()
    {
        Circle_Range.gameObject.SetActive(false);
        CanvasObject.SetActive(false);
        Circle_Range.localScale = Vector2.zero;
    }

    private void MakeCollider()
    {
        var collider = gameObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(Spawner.xValue, Spawner.yValue);
    }

    public void SpawnCharacter(HeroData data, string rarity)
    {
        Holder_Name = data.heroName;
        m_Data = data;
        if (IsServer)
        {
            HeroSpawn(Net_Utils.LocalID(), data, rarity);
        }
    }

    private void HeroSpawn(ulong clientId, HeroData data, string rarity)
    {
        var go = Instantiate(_spawn_Hero);
      
        NetworkObject networkObject = go.GetComponent<NetworkObject>();
        networkObject.Spawn();

        go.transform.parent = this.transform;

        ClientSpawnHeroClientRpc(networkObject.NetworkObjectId, clientId, data, rarity);
    }

    void CheckGetPosition()
    {
        for(int i = 0; i < m_Heroes.Count; i++)
        {
            m_Heroes[i].transform.localPosition = Hero_Vector_Pos(m_Heroes.Count)[i];
            m_Heroes[i].OrderChange(i + 1);
        }
    }

    private Vector2[] Hero_Vector_Pos(int count)
    {
        switch(count)
        {
            case 1: return One;
            case 2: return Two;
            case 3: return Three;
        }
        return null;
    }
    

    [ClientRpc]
    private void ClientSpawnHeroClientRpc(ulong networkID, ulong clientId, HeroData data, string rarity)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkID, out NetworkObject heroNetworkObject))
        {
            Hero hero = heroNetworkObject.GetComponent<Hero>();

            m_Heroes.Add(hero);
            hero.Initalize(data, this, rarity);
            CheckGetPosition();
        }
    }
}

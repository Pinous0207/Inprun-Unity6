using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor.PackageManager;
using UnityEngine;

public class Hero_Holder : NetworkBehaviour
{
    [SerializeField] private Hero _spawn_Hero;
    [SerializeField] Transform Circle_Range;

    public string Holder_Name;
    public List<Hero> m_Heroes = new List<Hero>();
    public Vector2 pos;
    HeroData m_Data;

    private readonly Vector2[] One = {Vector2.zero};
    private readonly Vector2[] Two =
        {
        new Vector2(-0.1f, 0.05f),
        new Vector2(0.1f, -0.1f)
    };
    private readonly Vector2[] Three =
        {
        new Vector2(-0.1f, 0.1f),
        new Vector2(0.1f, -0.05f),
        new Vector2(-0.15f, -0.15f)
    };

    private void Start()
    {
        MakeCollider();
    }

    public void GetRange()
    {
        float range = m_Data.heroRange * 2;
        Circle_Range.localScale = new Vector2(range, range);

        Circle_Range.gameObject.SetActive(true);
    }
    public void ReturnRange()
    {
        Circle_Range.gameObject.SetActive(false);
        Circle_Range.localScale = Vector2.zero;
    }

    private void MakeCollider()
    {
        var collider = gameObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(Spawner.xValue, Spawner.yValue);
    }

    public void SpawnCharacter(HeroData data)
    {
        Holder_Name = data.heroName;
        m_Data = data;
        if (IsServer)
        {
            HeroSpawn(LocalID(), data);
        }
    }

    private void HeroSpawn(ulong clientId, HeroData data)
    {
        var go = Instantiate(_spawn_Hero);
      
        NetworkObject networkObject = go.GetComponent<NetworkObject>();
        networkObject.Spawn();

        go.transform.parent = this.transform;

        ClientSpawnHeroClientRpc(networkObject.NetworkObjectId, clientId, data);
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
    private void ClientSpawnHeroClientRpc(ulong networkID, ulong clientId, HeroData data)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkID, out NetworkObject heroNetworkObject))
        {
            Hero hero = heroNetworkObject.GetComponent<Hero>();

            m_Heroes.Add(hero);
            hero.Initalize(data, this);
            CheckGetPosition();
        }
    }

    private ulong LocalID()
    {
        return NetworkManager.Singleton.LocalClientId;
    }
}

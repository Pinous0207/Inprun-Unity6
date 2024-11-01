using Unity.VisualScripting;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Collections;
using UnityEditor.Build.Content;
using Unity.Netcode;
using UnityEditor.PackageManager;
public class Spawner : NetworkBehaviour
{
    [SerializeField] private GameObject _spawn_Prefab;
    [SerializeField] private Monster _spawn_Monster_Prefab;

    public List<Vector2> Player_move_List = new List<Vector2>();
    public List<Vector2> Other_move_List = new List<Vector2>();

    List<Vector2> Player_spawn_list = new List<Vector2>();
    List<Vector2> Other_spawn_list = new List<Vector2>();

    List<bool> Player_spawn_list_Array= new List<bool>();
    List<bool> Other_spawn_list_Array = new List<bool>();

    private void Start()
    {
        SetGrid();
        StartCoroutine(Spawn_Monster_Coroutine());
    }

    private void SetGrid()
    {
        Grid_Start(transform.GetChild(0), true);
        Grid_Start(transform.GetChild(1), false);
        for (int i = 0; i < transform.GetChild(0).childCount; i++)
        {
            Player_move_List.Add(transform.GetChild(0).GetChild(i).position);
        }

        for (int i = 0; i < transform.GetChild(1).childCount; i++)
        {
            Other_move_List.Add(transform.GetChild(1).GetChild(i).position);
        }
    }

    #region Make Grid
    private void Grid_Start(Transform tt, bool Player)
    {
        SpriteRenderer parentSprite = tt.GetComponent<SpriteRenderer>();

        float parentwidth = parentSprite.bounds.size.x;
        float parentheight = parentSprite.bounds.size.y;

        float xCount = tt.localScale.x / 6;
        float yCount = tt.localScale.y / 3;
        for (int row = 0; row < 3; row++) // 상하 = 3개
        {
            for (int col = 0; col < 6; col++) // 좌우 = 6개
            {
                float xPos = (-parentwidth / 2) + (col * xCount) + (xCount / 2);
                float yPos = ((Player ? parentheight : -parentheight) / 2) + ((Player ? -1 : 1) * (row * yCount)) + (yCount / 2);
                switch (Player)
                {
                    case true:
                        Player_spawn_list.Add(new Vector2(xPos, yPos + tt.localPosition.y - yCount));
                        Player_spawn_list_Array.Add(false);
                        break;
                    case false:
                        Other_spawn_list.Add(new Vector2(xPos, yPos + tt.localPosition.y));
                        Other_spawn_list_Array.Add(false);
                        break;
                }
            }
        }
    }
    #endregion

    #region 캐릭터 소환
    public void Summon()
    {
        //if (Game_Mng.instance.Money < Game_Mng.instance.SummonCount)
        //{
        //    return;
        //}

        //Game_Mng.instance.Money -= Game_Mng.instance.SummonCount;
        //Game_Mng.instance.SummonCount += 2;

        if(IsClient)
        {
            ServerSpawnHeroServerRpc(LocalID());
        }
        else if(IsServer)
        {
            HeroSpawn(LocalID());
        }

        
    }

    [ServerRpc(RequireOwnership =false)]
    private void ServerSpawnHeroServerRpc(ulong clientId)
    {
        HeroSpawn(clientId);
    }

    private void HeroSpawn(ulong clientId)
    {
        var go = Instantiate(_spawn_Prefab);
        NetworkObject networkObject = go.GetComponent<NetworkObject>();
        networkObject.Spawn();

        ClientSpawnHeroClientRpc(networkObject.NetworkObjectId, clientId);
    }

    [ClientRpc]
    private void ClientSpawnHeroClientRpc(ulong networkID, ulong clientId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkID, out NetworkObject heroNetworkObject))
        {
            if (clientId == LocalID())
            {
                SetPositionHero(heroNetworkObject, true);
            }
            else
            {
                SetPositionHero(heroNetworkObject, false);
            }
        }
    }

    private void SetPositionHero(NetworkObject obj, bool Player)
    {
        List<bool> spawnListArray = Player ? Player_spawn_list_Array : Other_spawn_list_Array;
        List<Vector2> spawnList = Player ? Player_spawn_list : Other_spawn_list;
        int position_value = -1;
        for (int i = 0; i < spawnListArray.Count; i++)
        {
            if (spawnListArray[i] == false)
            {
                position_value = i;
                spawnListArray[i] = true;
                break;
            }
        }
        obj.transform.position = spawnList[position_value];
    }

    #endregion

    #region 몬스터 소환

    IEnumerator Spawn_Monster_Coroutine()
    {
        yield return new WaitForSeconds(1.0f);

        if (IsClient)
        {
            ServerMonsterSpawnServerRpc(LocalID());
        }
        else if(IsServer)
        {
            MonsterSpawn(LocalID());
        }

        StartCoroutine(Spawn_Monster_Coroutine());
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerMonsterSpawnServerRpc(ulong clientId)
    {
        MonsterSpawn(clientId);
    }

    private void MonsterSpawn(ulong clientId)
    {
        var go = Instantiate(_spawn_Monster_Prefab);
        NetworkObject networkObject = go.GetComponent<NetworkObject>();
        networkObject.Spawn();
        
        Game_Mng.instance.AddMonster(go);
        ClientMonsterSetClientRpc(networkObject.NetworkObjectId, clientId);
    }

    [ClientRpc]
    private void ClientMonsterSetClientRpc(ulong networkObjectId, ulong clientId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject monsterNetworkObject))
        {
            if (clientId == LocalID())
            {
                monsterNetworkObject.transform.position = Player_move_List[0];
                monsterNetworkObject.GetComponent<Monster>().Init(Player_move_List);
            }
            else
            {
                monsterNetworkObject.transform.position = Other_move_List[0];
                monsterNetworkObject.GetComponent<Monster>().Init(Other_move_List);
            }
        }
    }

    private ulong LocalID()
    {
        return NetworkManager.Singleton.LocalClientId;
    }

    #endregion
}

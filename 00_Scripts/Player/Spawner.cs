using Unity.VisualScripting;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Collections;
using UnityEditor.Build.Content;
using Unity.Netcode;
using UnityEditor.PackageManager;
using static UnityEngine.Rendering.DebugUI;
using UnityEditor.Search;
public class Spawner : NetworkBehaviour
{
    public static Spawner instance = null;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    [SerializeField] private GameObject _spawn_Holder;
    [SerializeField] private Monster _spawn_Monster_Prefab;

    public List<Vector2> Player_move_List = new List<Vector2>();
    public List<Vector2> Other_move_List = new List<Vector2>();

    public static List<Vector2> Player_spawn_list = new List<Vector2>();
    public static List<Vector2> Other_spawn_list = new List<Vector2>();

    List<bool> Player_spawn_list_Array= new List<bool>();
    List<bool> Other_spawn_list_Array = new List<bool>();

    public Dictionary<string, Hero_Holder> Hero_Holders = new Dictionary<string, Hero_Holder>();
    private int[] Host_Client_Value_Index = new int[2];

    public static float xValue, yValue;

    public void Holder_Position_Set(string Value01, string Value02)
    {
        if(IsServer)
        {
            GetPositionSet(Value01, Value02);
        }
        else if(IsClient)
        {
            GetPositionSetServerRpc(Value01, Value02);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void GetPositionSetServerRpc(string Value01, string Value02)
    {
        GetPositionSet(Value01, Value02);
    }

    private void GetPositionSet(string Value01, string Value02)
    {
        GetPositionSetClientRpc(Value01, Value02);
    }

    [ClientRpc]
    private void GetPositionSetClientRpc(string Value01, string Value02)
    {
        Hero_Holder holder01 = Hero_Holders[Value01];
        Hero_Holder holder02 = Hero_Holders[Value02];

        holder01.HeroChange(holder02);
        holder02.HeroChange(holder01);

        List<Hero> Heroes01 = new List<Hero>(holder01.m_Heroes);
        List<Hero> Heroes02 = new List<Hero>(holder02.m_Heroes);

        holder01.m_Heroes = new List<Hero>(Heroes02);
        holder02.m_Heroes = new List<Hero>(Heroes01);
    }

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

        xValue = xCount;
        yValue = yCount;

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
        Hero_Scriptable[] m_Character_Datas = Resources.LoadAll<Hero_Scriptable>("Character_Scriptable");
        var data = m_Character_Datas[UnityEngine.Random.Range(0, m_Character_Datas.Length)];
       
        bool GetHero = false;
        string temp = clientId == 0 ? "HOST" : "CLIENT";
        int value = clientId == 0 ? 0 : 1;
        string Organizers = temp + Host_Client_Value_Index[value].ToString();

        foreach (var dd in Hero_Holders)
        {
            if (dd.Key.Contains(temp))
            {
                if (dd.Value.m_Heroes.Count < 3 && dd.Value.Holder_Name == data.Name)
                {
                    dd.Value.SpawnCharacter(data.GetHeroData());
                    GetHero = true;
                    break;
                }
            }
        }
        if (GetHero == false)
        {
            var go = Instantiate(_spawn_Holder);
            NetworkObject networkObject = go.GetComponent<NetworkObject>();
            networkObject.Spawn();


            ClientSpawnHeroClientRpc(networkObject.NetworkObjectId, clientId, data.GetHeroData(), Organizers, value);
        }
    }

    [ClientRpc]
    private void ClientSpawnHeroClientRpc(ulong networkID, ulong clientId, HeroData data, string Organizers, int value)
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

            Hero_Holder goHolder = heroNetworkObject.GetComponent<Hero_Holder>();

            Hero_Holders.Add(Organizers, goHolder);
            Host_Client_Value_Index[value]++;

            goHolder.Holder_Part_Name = Organizers;
            heroNetworkObject.GetComponent<Hero_Holder>().SpawnCharacter(data);
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
        
        Hero_Holder holder = obj.GetComponent<Hero_Holder>();
        
        holder.pos = spawnList[position_value];
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

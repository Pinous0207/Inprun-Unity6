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
using Unity.Android.Gradle.Manifest;
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
    public static List<bool> Player_spawn_list_Array= new List<bool>();
    public static List<bool> Other_spawn_list_Array = new List<bool>();

    public Dictionary<string, Hero_Holder> Hero_Holders = new Dictionary<string, Hero_Holder>();
    private int[] Host_Client_Value_Index = new int[2];
    public static float xValue, yValue;
    Hero_Scriptable data;
    public void Holder_Position_Set(string Value01, string Value02)
    {
        Net_Utils.HostAndClientMethod(
            () => GetPositionSetServerRpc(Value01, Value02),
            () => GetPositionSet(Value01, Value02));
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

        (holder01.Holder_Name, holder02.Holder_Name) = (holder02.Holder_Name, holder01.Holder_Name);
        (holder01.m_Heroes, holder02.m_Heroes) = (new List<Hero>(holder02.m_Heroes), new List<Hero>(holder01.m_Heroes));
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

                if (IsServer)
                {
                    StartCoroutine(DelayHeroHolderSpawn(Player));
                }
            }
        }
        Host_Client_Value_Index[0] = 0; // HOST
        Host_Client_Value_Index[1] = 0; // CLIENT
    }
    IEnumerator DelayHeroHolderSpawn(bool Player)
    {
        var go = Instantiate(_spawn_Holder);
        NetworkObject networkObject = go.GetComponent<NetworkObject>();
        networkObject.Spawn();

        string temp = Player == true ? "HOST" : "CLIENT";
        int value = Player == true ? 0 : 1;
        string Organizers = temp + Host_Client_Value_Index[value].ToString();
        Host_Client_Value_Index[value]++;

        yield return new WaitForSeconds(0.1f);

        SpawnGridClientRpc(networkObject.NetworkObjectId, Organizers);
    }

    #endregion

    #region 캐릭터 소환
    public void Summon(string Rarity, Hero_Scriptable scriptData = null)
    {
        data = scriptData;
        if (scriptData == null)
            data = Data(Rarity);

        Net_Utils.HostAndClientMethod(
            () => ServerSpawnHeroServerRpc(Net_Utils.LocalID(), Rarity),
            () => HeroSpawn(Net_Utils.LocalID(), Rarity));
    }

    [ServerRpc(RequireOwnership =false)]
    private void ServerSpawnHeroServerRpc(ulong clientId, string rarity)
    {
        HeroSpawn(clientId, rarity);
    }

    public Hero_Scriptable Data(string rarity)
    {
        Hero_Scriptable[] m_Character_Datas = Resources.LoadAll<Hero_Scriptable>("Character_Scriptable/" + rarity);
        var data = m_Character_Datas[UnityEngine.Random.Range(0, m_Character_Datas.Length)];

        return data;
    }

    private void HeroSpawn(ulong clientId, string rarity)
    {
        string temp = clientId == 0 ? "HOST" : "CLIENT";
        int value = clientId == 0 ? 0 : 1;
        string Organizers = temp + Host_Client_Value_Index[value].ToString();

        var existingHolder = GetExistingHolder(temp, data.Name);

        ClientNavigationClientRpc(clientId, data.Name, data.rare.ToString());

        if (existingHolder != null)
        {
            existingHolder.SpawnCharacter(data.GetHeroData(), rarity);
            return;
        }

        var networkObject = Hero_Holders[Organizers].GetComponent<NetworkObject>();

        ClientSpawnHeroClientRpc(networkObject.NetworkObjectId, data.GetHeroData(), value, rarity);
    }

    public Vector3 HolderPosition(Hero_Scriptable data)
    {
        string temp = Net_Utils.LocalID() == 0 ? "HOST" : "CLIENT";
        int value = Net_Utils.LocalID() == 0 ? 0 : 1;
        string Organizers = temp + Host_Client_Value_Index[value].ToString();

        var existingHolder = GetExistingHolder(temp, data.Name);

        if (existingHolder != null)
        {
            return existingHolder.transform.position;
        }

        return Hero_Holders[Organizers].transform.position;
    }

    private Hero_Holder GetExistingHolder(string clientKey, string heroName)
    {
        foreach (var holder in Hero_Holders)
        {
            if(holder.Key.Contains(clientKey) && holder.Value.m_Heroes.Count < 3 && holder.Value.Holder_Name == heroName)
            {
                return holder.Value;
            }
        }
        return null;
    }

    [ClientRpc]
    private void SpawnGridClientRpc(ulong networkID, string Organizers)
    {
        if (Net_Utils.TryGetSpawnedObject(networkID, out NetworkObject heroNetworkObject))
        {
            bool isPlayer;
            if (Organizers.Contains("HOST"))
            {
                isPlayer = Net_Utils.LocalID() == 0 ? true : false;
            }
            else isPlayer = Net_Utils.LocalID() == 0 ? false : true;

            Hero_Holder goHolder = heroNetworkObject.GetComponent<Hero_Holder>();
            SetPositionHero(heroNetworkObject,
                   isPlayer ? Player_spawn_list : Other_spawn_list,
                   isPlayer ? Player_spawn_list_Array : Other_spawn_list_Array);

            Hero_Holders.Add(Organizers, goHolder);

            goHolder.Holder_Part_Name = Organizers;
        }
    }

    [ClientRpc]
    private void ClientSpawnHeroClientRpc(ulong networkID, HeroData data, int value,string rarity)
    {
        if (Net_Utils.TryGetSpawnedObject(networkID, out NetworkObject heroNetworkObject))
        {
            Hero_Holder goHolder = heroNetworkObject.GetComponent<Hero_Holder>();
            Host_Client_Value_Index[value]++;

            goHolder.SpawnCharacter(data, rarity);
        }
    }

    [ClientRpc]
    private void ClientNavigationClientRpc(ulong networkID, string heroName, string rarity)
    {
        if (networkID == Net_Utils.LocalID())
        {
            UI_Main.instance.GetNavigation(string.Format("영웅을 획득하였습니다. {0}{1}", 
                Net_Utils.RarityColor((Rarity)Enum.Parse(typeof(Rarity), rarity)), 
                heroName));
        }
    }

    private void SetPositionHero(NetworkObject obj, List<Vector2> spawnList, List<bool> spawnArrayList)
    {
        int position_value = spawnArrayList.IndexOf(false);
        if(position_value != -1)
        {
            spawnArrayList[position_value] = true;
            obj.transform.position = spawnList[position_value];
        }
        Hero_Holder holder = obj.GetComponent<Hero_Holder>();
        holder.index = position_value;
    }

    #endregion

    #region 몬스터 소환

    IEnumerator Spawn_Monster_Coroutine()
    {
        yield return new WaitForSeconds(1.0f);

        Net_Utils.HostAndClientMethod(
            () => ServerMonsterSpawnServerRpc(Net_Utils.LocalID()),
            () => MonsterSpawn(Net_Utils.LocalID()));

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
        if (Net_Utils.TryGetSpawnedObject(networkObjectId, out NetworkObject monsterNetworkObject))
        {
            var moveList = clientId == Net_Utils.LocalID() ? Player_move_List : Other_move_List;
            monsterNetworkObject.transform.position = moveList[0];
            monsterNetworkObject.GetComponent<Monster>().Init(moveList);
        }
    }
    #endregion
}

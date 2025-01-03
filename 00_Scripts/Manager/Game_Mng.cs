using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;

public delegate void OnMoneyUpEventHandler();
public delegate void OnTimerUpEventHandler();
public class Game_Mng : NetworkBehaviour
{
    public static Game_Mng instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        B_Data = Resources.Load<Boss_Scriptable>("Boss/Boss_Scriptable");
    }

    public float Timer = 60.0f;
    public int Wave = 1;
    public int Money = 50;
    public int SummonCount = 20;
    public int HeroCount;
    public int HeroMaximumCount = 25;
    public int[] Upgrade = new int[4];

    public event OnMoneyUpEventHandler OnMoneyUp;
    public event OnTimerUpEventHandler OnTimerUp;

    public List<Monster> monsters = new List<Monster>();
    public List<Monster> Boss_Monsters = new List<Monster>();
    public Boss_Scriptable B_Data;
    public int MonsterCount;

    public bool GetBoss = false;

    private void Update()
    {
        if(IsServer)
        {
            bool GetWaveUp = false;
            if(Timer > 0)
            {
                Timer -= Time.deltaTime;
                Timer = Mathf.Max(Timer, 0); // 음수 방지
            }
            else
            {
                if(GetBoss)
                {
                    Debug.Log("게임 실패");
                    return;
                }
                Wave++;
                GetWaveUp = true;
                Timer = 60.0f;
            }
            NotifyTimerClientRpc(Timer, Wave, GetWaveUp);
        }
    }

    public void GetMoney(int value, HostType type = HostType.All)
    {
        if(type == HostType.All)
        {
            NotifyGetMoneyClientRpc(value);
        }
    }

    public void AddMonster(Monster monster, bool Boss = false)
    {
        if(Boss)
            Boss_Monsters.Add(monster);
        else
            monsters.Add(monster);
        MonsterCount++;
        UpdateMonsterCountOnClients();
    }
    public void RemoveMonster(Monster monster, bool Boss =false)
    {
        if (Boss)
        {
            Boss_Monsters.Remove(monster);
            if(Boss_Monsters.Count == 0)
            {
                GetBoss = false;
                Timer = 0.0f;
            }
        }
        else
            monsters.Remove(monster);
        MonsterCount--;
        UpdateMonsterCountOnClients();
    }


    private void UpdateMonsterCountOnClients()
    {
        NotifyClientMonsterCountClientRpc(MonsterCount);
    }

    [ClientRpc]
    private void NotifyTimerClientRpc(float timer, int wave, bool GetWaveUp)
    {
        Timer = timer;
        Wave = wave;

        if(GetWaveUp)
        {
            GetBoss = false;
            if (Wave % 10 == 0)
            {
                GetBoss = true;
                Spawner.instance.BossSpawn();
            }
            else
                Spawner.instance.ReMonsterSpawn();
            UI_Main.instance.GetWavePopUp(GetBoss);
        }

        OnTimerUp?.Invoke();
    }

    [ClientRpc]
    private void NotifyGetMoneyClientRpc(int value)
    {
        Money += value;
        OnMoneyUp?.Invoke();
    }

    [ClientRpc]
    private void NotifyClientMonsterCountClientRpc(int count)
    {
        MonsterCount = count;
    }
}

using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;

public delegate void OnMoneyUpEventHandler();
public delegate void OnTimerUpEventHandler();
public delegate void OnGameOverEventHandler();
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
    public int MonsterLimitCount = 100;
    public int[] Upgrade = new int[4];
    public double Damage;

    public double HostDPS, ClientDPS;

    public float DistanceMagnitude;

    public event OnMoneyUpEventHandler OnMoneyUp;
    public event OnTimerUpEventHandler OnTimerUp;
    public event OnGameOverEventHandler OnGameOver;

    public List<Monster> monsters = new List<Monster>();
    public List<Monster> Boss_Monsters = new List<Monster>();
    public List<Hero> heros = new List<Hero>();
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
    public void OnGameOverEvent()
    {
        Time.timeScale = 0.0f;

        if(IsServer)
        {
            HostDPS = Damage;
            SendDpsToClientRpc(Damage);
        }
        else
        {
            ClientDPS = Damage;
            SendDpsToServerRpc(Damage);
        }

        OnGameOver?.Invoke();
    }

    public void DamageCount(double damage)
    {
        Damage += damage;
    }

    public void AddHero(Hero hero)
    {
        heros.Add(hero);
    }

    public void RemoveHero(Hero hero)
    {
        heros.Remove(hero);
    }

    public void AddMonster(Monster monster, bool Boss = false)
    {
        if(Boss)
            Boss_Monsters.Add(monster);
        else
            monsters.Add(monster);
        MonsterCount++;
        if(MonsterCount >= MonsterLimitCount)
        {
            OnGameOverClientRpc();
        }
        UpdateMonsterCountOnClients();
    }

    [ClientRpc]
    public void OnGameOverClientRpc()
    {
        OnGameOverEvent();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendDpsToServerRpc(double clientDps)
    {
        ClientDPS = clientDps;
    }

    [ClientRpc]
    private void SendDpsToClientRpc(double hostDps)
    {
        if(!IsServer)
        {
            HostDPS = hostDps;
        }
    }

    public void CleanupNetworkObjects()
    {
        if(IsHost)
        {
            foreach (var obj in FindObjectsByType<NetworkObject>(FindObjectsSortMode.None))
            {
                if (obj != null && obj.IsSpawned)
                {
                    obj.Despawn(true);
                }
            }
        }
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

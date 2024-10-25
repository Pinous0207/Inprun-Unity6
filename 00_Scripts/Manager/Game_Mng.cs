using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public delegate void OnMoveyUpEventHandler();
public class Game_Mng : MonoBehaviour
{
    public static Game_Mng instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public int Money = 50;
    public int SummonCount = 20;

    public event OnMoveyUpEventHandler OnMoneyUp;

    public List<Monster> monsters = new List<Monster>();

    public void GetMoney(int value)
    {
        Money += value;
        OnMoneyUp?.Invoke();
    }

    public void AddMonster(Monster monster)
    {
        monsters.Add(monster);
    }
    public void RemoveMonster(Monster monster)
    {
        monsters.Remove(monster);
    }
}

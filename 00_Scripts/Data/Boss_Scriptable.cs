using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BossData
{
    public string BossName;
    public Monster BossPrefab;
}

[CreateAssetMenu(fileName = "Boss_Scriptable", menuName = "Scriptable Objects/Boss_Scriptable")]
public class Boss_Scriptable : ScriptableObject
{
    public List<BossData> bossData = new List<BossData>();
}

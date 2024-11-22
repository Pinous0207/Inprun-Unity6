using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct HeroData : INetworkSerializable
{
    public string heroName;
    public int heroATK;
    public float heroATK_Speed;
    public float heroRange;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref heroName);
        serializer.SerializeValue(ref heroATK);
        serializer.SerializeValue(ref heroATK_Speed);
        serializer.SerializeValue(ref heroRange);
    }
}

[CreateAssetMenu(fileName = "Hero_Scriptable", menuName = "Scriptable Objects/Hero_Scriptable")]
public class Hero_Scriptable : ScriptableObject
{
    public string Name;
    public int ATK;
    public float ATK_Speed;
    public float Range;
    public RuntimeAnimatorController m_Animator;
    public Rarity rare;
    public HeroData GetHeroData()
    {
        return new HeroData
        {
            heroName = Name,
            heroATK = ATK,
            heroATK_Speed = ATK_Speed,
            heroRange = Range,
        };
    }
}

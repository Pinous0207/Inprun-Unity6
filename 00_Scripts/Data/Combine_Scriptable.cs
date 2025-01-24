using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Combine_Scriptable", menuName = "Scriptable Objects/Combine_Scriptable")]
public class Combine_Scriptable : ScriptableObject
{
    public Hero_Scriptable MainData;
    public List<Hero_Scriptable> SubDatas = new List<Hero_Scriptable>();
}

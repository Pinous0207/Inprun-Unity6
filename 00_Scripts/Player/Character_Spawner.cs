using Unity.VisualScripting;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Collections;
using UnityEditor.Build.Content;
public class Character_Spawner : MonoBehaviour
{
    [SerializeField] private GameObject _spawn_Prefab;
    [SerializeField] private Monster _spawn_Monster_Prefab;

    public static List<Vector2> move_list = new List<Vector2>();
    List<Vector2> spawn_list = new List<Vector2>();
    List<bool> spawn_list_Array = new List<bool>();
    private void Start()
    {
        Grid_Start();

        for(int i = 0; i< transform.childCount; i++)
        {
            move_list.Add(transform.GetChild(i).position);
        }

        StartCoroutine(Spawn_Monster_Coroutine());
    }

    #region Make Grid
    private void Grid_Start()
    {
        SpriteRenderer parentSprite = GetComponent<SpriteRenderer>();

        float parentwidth = parentSprite.bounds.size.x;
        float parentheight = parentSprite.bounds.size.y;

        float xCount = transform.localScale.x / 6;
        float yCount = transform.localScale.y / 3;
        for (int row = 0; row < 3; row++) // 상하 = 3개
        {
            for (int col = 0; col < 6; col++) // 좌우 = 6개
            {
                float xPos = (-parentwidth / 2) + (col * xCount) + (xCount / 2);
                float yPos = (parentheight / 2) - (row * yCount) + (yCount / 2);

                spawn_list.Add(new Vector2(xPos, yPos + transform.localPosition.y - yCount));
                spawn_list_Array.Add(false);
            }
        }
    }
    #endregion

    #region 캐릭터 소환
    public void Summon()
    {
        if(Game_Mng.instance.Money < Game_Mng.instance.SummonCount)
        {
            return;
        }

        Game_Mng.instance.Money -= Game_Mng.instance.SummonCount;
        Game_Mng.instance.SummonCount += 2;

        int position_value = -1; 
        var go = Instantiate(_spawn_Prefab);
        for(int i = 0; i< spawn_list_Array.Count; i++)
        {
            if (spawn_list_Array[i] == false)
            {
                position_value = i;
                spawn_list_Array[i] = true;
                break;
            }
        }
        go.transform.position = spawn_list[position_value];
    }
    #endregion

    #region 몬스터 소환

    IEnumerator Spawn_Monster_Coroutine()
    {
        var go = Instantiate(_spawn_Monster_Prefab, move_list[0], Quaternion.identity);
        
        Game_Mng.instance.AddMonster(go);

        yield return new WaitForSeconds(1.0f);

        StartCoroutine(Spawn_Monster_Coroutine());
    }

    #endregion
}

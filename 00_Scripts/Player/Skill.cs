using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class Skill : MonoBehaviour
{
    Hero hero;
    Hero_Scriptable m_Data;
    public SKILL m_State;
    public bool isReady = false;
    public Image Fill;


    private void Start()
    {
        hero = GetComponent<Hero>();
        Initalize();
    }

    private List<Monster> monsters()
    {
        return Game_Mng.instance.monsters;
    }
    private List<Hero> heros()
    {
        return Game_Mng.instance.heros;
    }

    private double SkillDamage()
    {
        return hero.ATK * (m_Data.skillData.SkillDamage / 100);
    }

    private float DistanceStep(int value)
    {
        return (Game_Mng.instance.DistanceMagnitude * value) + 0.1f;
    }

    private bool Distance(Vector2 startPos, Vector2 endPos, float CheckDistance)
    {
        if(Vector2.Distance(startPos, endPos) <= CheckDistance)
        {
            return true;
        }
        return false;
    }

    private void Initalize()
    {
        if (hero.m_Data.skillData.skill != SKILL.None)
        {
            m_Data = hero.m_Data;
            m_State = m_Data.skillData.skill;
            Fill.transform.parent.gameObject.SetActive(true);
            StartCoroutine(SkillDelay());
        }
        else Destroy(this);
    }

    private void Update()
    {
        if(hero.target != null && isReady)
        {
            isReady = false;
            StartCoroutine(SkillDelay());
            GetSkill();
        }
    }

    private void GetSkill()
    {
        switch(m_State)
        {
            case SKILL.Gun: Gun(); break;
            case SKILL.Sword: Sword(); break;
        }
    }

    private void Sword()
    {
        Vector2 pos = transform.position;
        for(int i = 0; i < heros().Count; i++)
        {
            var hero =  heros()[i];
            if(Distance(pos, hero.transform.position, DistanceStep(2)))
            {
                Instantiate(m_Data.skillData.Particle, hero.transform.position, Quaternion.identity);
                hero.SetATKSpeed(m_Data.skillData.SkillDamage / 100, 3.0f);
            }
        }
    }
    private void Gun()
    {
        Vector2 pos = hero.target.transform.position;
        Instantiate(m_Data.skillData.Particle, pos, Quaternion.identity);

        for(int i = 0;i < monsters().Count; i++)
        {
            if(Distance(pos, monsters()[i].transform.position, 0.5f))
            {
                var monster = monsters()[i];
                monster.GetDamage(SkillDamage());

                float[] values = { 2.0f };
                monster.ApplyDebuffServerRpc(1, values);
            }
        }
    }

    IEnumerator SkillDelay()
    {
        float t = 0.0f;
        float cooltime = hero.m_Data.skillData.Cooltime;
        while(t < cooltime)
        {
            t += Time.deltaTime;
            Fill.fillAmount = t / cooltime;
            yield return null;
        }
        isReady = true;
    }
}

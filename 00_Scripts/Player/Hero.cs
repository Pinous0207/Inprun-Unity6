using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;

public class Hero : Character
{
    Hero_Holder parent_holder;
    private double baseATK;
    public double ATK
    {
        get
        {
            float upgradeBonus = 
                Game_Mng.instance.Upgrade[UpgradeCount()] != 0 ? 
                Game_Mng.instance.Upgrade[UpgradeCount()] * 0.1f : 0;
            return baseATK * (1 + upgradeBonus);
        }
    }
    public float attackRange = 1.0f;
    public float attackSpeed = 1.0f;
    public NetworkObject target;
    public LayerMask enemyLayer;
    public Hero_Scriptable m_Data;
    bool isMove = false;

    public string HeroName;
    public Rarity HeroRarity;
    public Color[] colors;
    public SpriteRenderer circleRenderer;
    [SerializeField] private GameObject SpawnParticle;


    private int UpgradeCount()
    {
        switch(m_Data.rare)
        {
            case Rarity.Common:
            case Rarity.UnCommon:
            case Rarity.Rare:
                return 0;
            case Rarity.Hero:
                return 1;
            case Rarity.Legendary:
                return 2;
        }
        return -1;
    }

    public void Initalize(HeroData obj, Hero_Holder holder, string rarity)
    {
        m_Data = Resources.Load<Hero_Scriptable>("Character_Scriptable/" + rarity + "/" + obj.heroName);
        parent_holder = holder;
        baseATK = obj.heroATK;
        attackRange = obj.heroRange;
        attackSpeed = obj.heroATK_Speed;

        HeroName = obj.heroName;
        HeroRarity = (Rarity)Enum.Parse(typeof(Rarity), rarity);

        circleRenderer.color = colors[(int)HeroRarity];

        GetInitCharacter(obj.heroName, rarity);

        Instantiate(SpawnParticle, parent_holder.transform.position, Quaternion.identity);
    }

    public void Position_Change(Hero_Holder holder, List<Vector2> poss, int myIndex)
    {
        isMove = true;
        AnimatorChange("MOVE", false);

        parent_holder = holder;
        
        if(IsServer)
            transform.parent = holder.transform;

        int sign = (int)Mathf.Sign(poss[myIndex].x - transform.position.x);
        switch(sign)
        {
            case -1: renderer.flipX = true; break;
            case 1: renderer.flipX = false; break;
        }
        StartCoroutine(Move_Coroutine(poss[myIndex]));
    }

    private IEnumerator Move_Coroutine(Vector2 endPos)
    {
        float current = 0.0f;
        float percent = 0.0f;
        Vector2 start = transform.position;
        Vector2 end = endPos;
        while(percent < 1.0f)
        {
            current += Time.deltaTime;
            percent = current / 0.5f;
            Vector2 LerpPos = Vector2.Lerp(start, end, percent);
            transform.position = LerpPos;
            yield return null;
        }
        isMove = false;
        AnimatorChange("IDLE", false);
        renderer.flipX = true;
    }

    private void Update()
    {
        if (isMove) return;
        CheckForEnemies();
    }

    void CheckForEnemies()
    {
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(parent_holder.transform.position, attackRange, enemyLayer);
        attackSpeed += Time.deltaTime;
        if(enemiesInRange.Length > 0)
        {
            target = enemiesInRange[0].GetComponent<NetworkObject>();
            if(attackSpeed >= 1.0f)
            {
                attackSpeed = 0.0f;
                AnimatorChange("ATTACK", true);
                GetBullet();
                //AttackMonsterServerRpc(target.NetworkObjectId);
            }
        }
        else
        {
            target = null;
        }
    }

    public void GetBullet()
    {
        var go = Instantiate(m_Data.HitParticle, transform.position + new Vector3(0.0f, 0.1f), Quaternion.identity);
        go.Init(target.transform, this);
    }


    public void SetDamage()
    {
        if (target != null)
        {
            AttackMonsterServerRpc(target.NetworkObjectId);
            if (m_Data.effectType != null)
            {
                for (int i = 0; i < m_Data.effectType.Length; i++)
                {
                    List<float> values = new List<float>(m_Data.effectType[i].parameters);

                    switch (m_Data.effectType[i].debuffType)
                    {
                        case Debuff.Slow:
                            if (UnityEngine.Random.value <= values[0])
                            {
                                values.RemoveAt(0);
                                target.GetComponent<Monster>().ApplyDebuffServerRpc((int)Debuff.Slow, values.ToArray());
                            }
                            break;
                        case Debuff.Stun:
                            if (UnityEngine.Random.value <= values[0])
                            {
                                values.RemoveAt(0);
                                target.GetComponent<Monster>().ApplyDebuffServerRpc((int)Debuff.Stun, values.ToArray());
                            }
                            break;
                    }
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AttackMonsterServerRpc(ulong monsterId)
    {
        if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(monsterId, out var spawnedObject))
        {
            Monster monster = spawnedObject.GetComponent<Monster>();
            if(monster != null)
            {
                monster.GetDamage(ATK);
            }
        }
    }
}

using NUnit.Framework;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Netcode;
using System;

public class Monster : Character
{
    public bool Boss;

    [SerializeField] private float m_Speed;
    private float originalSpeed;
    private float currentSlowAmount;
    private float currentSlowDuration;
    [SerializeField] private HitText hitText;
    [SerializeField] private Image m_Fill, m_Fill_Deco;
    [SerializeField] private Color slowColor;
    [SerializeField] private GameObject StunParticle; 
    Coroutine slowCoroutine;
    Coroutine stunCoroutine;

    int target_Value = 0;
    public double HP = 0, MaxHP = 0;
    bool isDead = false;
    bool isStun = false;
    List<Vector2> move_list = new List<Vector2>();
    public override void Awake()
    {
        HP = CalculateMonsterHP(Game_Mng.instance.Wave);
        MaxHP = HP;

        originalSpeed = m_Speed;

        base.Awake();
    }

    // 지수적 증가 공식
    double CalculateMonsterHP(int waveLevel)
    {
        double baseHP = 5000.0f;

        double powerMultiplier = Mathf.Pow(1.1f, waveLevel);

        if(waveLevel % 10 == 0)
        {
            powerMultiplier += 0.05f * (waveLevel / 10);
        }

        return baseHP * powerMultiplier * (Boss ? 10 : 1);
    }

    public void Init(List<Vector2> vectorList)
    {
        move_list = vectorList;
    }

    private void Update()
    {
        m_Fill_Deco.fillAmount = Mathf.Lerp(m_Fill_Deco.fillAmount, m_Fill.fillAmount, Time.deltaTime * 2.0f);

        if (isDead) return;
        if (isStun) return;
        transform.position = Vector2.MoveTowards(transform.position, move_list[target_Value], Time.deltaTime * m_Speed);
        if (Vector2.Distance(transform.position, move_list[target_Value]) <= 0.1f)
        {
            target_Value++;
            renderer.flipX = target_Value >= 3 ? true : false;
            if (target_Value >= 4)
            {
                target_Value = 0;
            }
        }
    }

    public void GetDamage(double dmg)
    {
        if (!IsServer) return;
        if (isDead) return;

        GetDamageMonster(dmg);
        NotifyClientUpdateClientRpc(HP -= dmg , dmg);
    }

    private void GetDamageMonster(double dmg)
    {
        HP -= dmg;
        m_Fill.fillAmount = (float)HP / (float)MaxHP;

        Instantiate(hitText, transform.position, Quaternion.identity).Initalize(dmg);

        if (HP <= 0)
        {
            isDead = true;
            gameObject.layer = LayerMask.NameToLayer("Default");
            Game_Mng.instance.GetMoney(1);
            StartCoroutine(Dead_Coroutine());
            AnimatorChange("DEAD", true);
        }
    }

    [ClientRpc]
    public void NotifyClientUpdateClientRpc(double hp, double dmg)
    {
        HP = hp;
        m_Fill.fillAmount = (float)HP / (float)MaxHP;

        Instantiate(hitText, transform.position, Quaternion.identity).Initalize(dmg);

        if (HP <= 0)
        {
            isDead = true;
            gameObject.layer = LayerMask.NameToLayer("Default");
            StartCoroutine(Dead_Coroutine());
            AnimatorChange("DEAD", true);
        }
    }

    IEnumerator Dead_Coroutine()
    {
        float Alpha = 1.0f;

        while (renderer.color.a > 0.0f)
        {
            Alpha -= Time.deltaTime;
            renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b,
                Alpha);

            yield return null;
        }

        if (IsServer)
        {
            Game_Mng.instance.RemoveMonster(this, Boss);
            this.gameObject.SetActive(false);
            Destroy(this); // NetworkBehaviour
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void ApplyDebuffServerRpc(int debuffType, float[] values)
    {
        Debuff debuff = (Debuff)debuffType;
        switch(debuff)
        {
            case Debuff.Slow:
                if (values[0] > currentSlowAmount || (values[0] == currentSlowAmount && values[1] > currentSlowDuration))
                {
                    currentSlowAmount = values[0];
                    currentSlowDuration = values[1];

                    ApplySlowClientRpc(values[0], values[1]);
                }
                break;
            case Debuff.Stun:
                ApplyStunClientRpc(values[0]);
                break;
        }
    }

    [ClientRpc]
    private void ApplyStunClientRpc(float stunDuration)
    {
        CoroutineStop(stunCoroutine);
        stunCoroutine = StartCoroutine(EffectCoroutine(stunDuration, () =>
        {
            isStun = true;
            StunParticle.SetActive(true);
        }, () =>
        {
            isStun = false;
            StunParticle.SetActive(false);
        }));
    }

    [ClientRpc]
    private void ApplySlowClientRpc(float slowAmount, float duration)
    {
        CoroutineStop(slowCoroutine);
        slowCoroutine = StartCoroutine(EffectCoroutine(duration, () =>
        {
            float newSpeed = originalSpeed - (originalSpeed * slowAmount);
            newSpeed = Mathf.Max(newSpeed, 0.1f);
            m_Speed = newSpeed;
            renderer.color = slowColor;
        }, () =>
        {
            m_Speed = originalSpeed;
            renderer.color = Color.white;
        }));
    }

    private void CoroutineStop(Coroutine coroutine)
    {
        if(coroutine != null)
        {
            StopCoroutine(coroutine);
        }
    }

    private IEnumerator EffectCoroutine(float duration, Action FirstAction, Action SecondAction)
    {
        FirstAction?.Invoke();
        yield return new WaitForSeconds(duration);
        SecondAction?.Invoke();
    }
}

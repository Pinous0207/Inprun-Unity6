using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.UI;

public class Monster : Character
{
    [SerializeField] private float m_Speed;
    [SerializeField] private HitText hitText;
    [SerializeField] private Image m_Fill, m_Fill_Deco;

    int target_Value = 0;
    public int HP = 0, MaxHP = 0;
    bool isDead = false;
    public override void Start()
    {
        HP = MaxHP;
        base.Start();
    }

    private void Update()
    {
        m_Fill_Deco.fillAmount = Mathf.Lerp(m_Fill_Deco.fillAmount, m_Fill.fillAmount, Time.deltaTime * 2.0f);

        if (isDead) return;

        transform.position = Vector2.MoveTowards(transform.position, Character_Spawner.move_list[target_Value], Time.deltaTime * m_Speed);
        if(Vector2.Distance(transform.position, Character_Spawner.move_list[target_Value]) <= 0.0f)
        {
            target_Value++;
            renderer.flipX = target_Value >= 3 ? true : false;
            if(target_Value >= 4)
            {
                target_Value = 0;
            }
        }
    }

    public void GetDamage(int dmg)
    {
        if (isDead) return;

        HP -= dmg;
        m_Fill.fillAmount = (float)HP / (float)MaxHP;
        Instantiate(hitText, transform.position, Quaternion.identity).Initalize(dmg);

        if (HP <= 0)
        {
            isDead = true;
            Game_Mng.instance.GetMoney(1);
            Game_Mng.instance.RemoveMonster(this);
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

        Destroy(this.gameObject);
    }
}

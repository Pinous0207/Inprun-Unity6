using TMPro;
using UnityEditor.Build.Content;
using UnityEngine;

public class UI_Main : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI MonsterCount_T;
    [SerializeField] private TextMeshProUGUI Money_T;
    [SerializeField] private TextMeshProUGUI Summon_T;

    [SerializeField] private Animator MoneyAnimation;

    private void Start()
    {
        Game_Mng.instance.OnMoneyUp += Money_Anim;
    }

    private void Update()
    {
        MonsterCount_T.text = Game_Mng.instance.monsters.Count.ToString() + " / 100";
        Money_T.text = Game_Mng.instance.Money.ToString();
        Summon_T.text = Game_Mng.instance.SummonCount.ToString();

        Summon_T.color = Game_Mng.instance.Money >= Game_Mng.instance.SummonCount ? Color.white : Color.red;
    }

    private void Money_Anim()
    {
        MoneyAnimation.SetTrigger("GET");
    }
}

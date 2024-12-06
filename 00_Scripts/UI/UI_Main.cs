using NUnit.Framework;
using TMPro;
using UnityEditor.Build.Content;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UI_Main : MonoBehaviour
{
    public static UI_Main instance = null;
    private void Awake()
    {
        if (instance == null) instance = this;
    }

    [SerializeField] private TextMeshProUGUI MonsterCount_T;
    [SerializeField] private TextMeshProUGUI Money_T;
    [SerializeField] private TextMeshProUGUI Summon_T;
    [SerializeField] private TextMeshProUGUI Timer_T;
    [SerializeField] private TextMeshProUGUI Wave_T;
    [SerializeField] private TextMeshProUGUI HeroCount_T;
    [SerializeField] private TextMeshProUGUI Navigation_T;

    [SerializeField] private Transform Navigation_Content;

    [SerializeField] private Image MonsterCount_Image;

    [SerializeField] private Animator MoneyAnimation;

    List<GameObject> NavigationTextList = new List<GameObject>();

    [SerializeField] private Button SummonButton;

    private void Start()
    {
        Game_Mng.instance.OnMoneyUp += Money_Anim;
        Game_Mng.instance.OnTimerUp += WavePoint;
        SummonButton.onClick.AddListener(() => Spawner.instance.Summon("Common", false));
    }

    private void Update()
    {
        MonsterCount_T.text = Game_Mng.instance.MonsterCount.ToString() + " / 100";
        MonsterCount_Image.fillAmount = (float)Game_Mng.instance.MonsterCount / 100.0f;
        HeroCount_T.text = UpdateHeroCountText();

        Money_T.text = Game_Mng.instance.Money.ToString();
        Summon_T.text = Game_Mng.instance.SummonCount.ToString();

        Summon_T.color = Game_Mng.instance.Money >= Game_Mng.instance.SummonCount ? Color.white : Color.red;
    }

    public void GetNavigation(string temp)
    {
        if(NavigationTextList.Count > 7)
        {
            Destroy(NavigationTextList[0]);
            NavigationTextList.RemoveAt(0);
        }
        var go = Instantiate(Navigation_T, Navigation_Content);
        NavigationTextList.Add(go.gameObject);
        go.gameObject.SetActive(true);

        go.transform.SetAsFirstSibling();
        Destroy(go.gameObject, 2.5f);

        go.text = temp;
    }

    public void WavePoint()
    {
        Timer_T.text = UpdateTimerText();
        Wave_T.text = "WAVE " + Game_Mng.instance.Wave.ToString();
    }

    string UpdateTimerText()
    {
        int minutes = Mathf.FloorToInt(Game_Mng.instance.Timer / 60);
        int seconds = Mathf.FloorToInt(Game_Mng.instance.Timer % 60);

        return $"{minutes:00}:{seconds:00}";
    }

    string UpdateHeroCountText()
    {
        int myCount = Game_Mng.instance.HeroCount;
        string temp = "";
        if(myCount < 10)
        {
            temp = "0" + myCount.ToString();
        }
        else
        {
            temp = myCount.ToString();
        }
        return string.Format("{0} / {1}", temp, Game_Mng.instance.HeroMaximumCount);
    }

    private void Money_Anim()
    {
        MoneyAnimation.SetTrigger("GET");
    }
}

using NUnit.Framework;
using TMPro;
using UnityEditor.Build.Content;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Android.Gradle.Manifest;
using UnityEditor.PackageManager;
public class UI_Main : MonoBehaviour
{
    public static UI_Main instance = null;
    private void Awake()
    {
        if (instance == null) instance = this;
    }
    [Header("##Texts")]
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

    [Header("##Trail Effect##")]
    [SerializeField] private GameObject TrailPrefab;
    [UnityEngine.Range(0.0f, 30.0f)]
    [SerializeField] private float trailSpeed;
    [SerializeField] private float yPosMin, yPosMax;
    [SerializeField] private float xPos;

    private void Start()
    {
        Game_Mng.instance.OnMoneyUp += Money_Anim;
        Game_Mng.instance.OnTimerUp += WavePoint;
        SummonButton.onClick.AddListener(() => ClickSummon());
    }

    private void ClickSummon()
    {
        if (Game_Mng.instance.Money < Game_Mng.instance.SummonCount) return;
        if (Game_Mng.instance.HeroCount >= Game_Mng.instance.HeroMaximumCount) return;

        Game_Mng.instance.Money -= Game_Mng.instance.SummonCount;
        Game_Mng.instance.SummonCount += 2;
        Game_Mng.instance.HeroCount++;
        StartCoroutine(SummonCoroutine());
    }

    private Vector3 GenerateRandomControlPoint(Vector3 start, Vector3 end)
    {
        // 시작점과 끝점의 중간 위치
        Vector3 midPoint = (start + end) / 2f;

        // Y축 방향으로 랜덤한 높이를 추가하여 곡선을 만듦
        float randomHeight = Random.Range(yPosMin, yPosMax);
        midPoint += Vector3.up * randomHeight;

        // X 방향으로도 약간의 랜덤 변화를 추가
        midPoint += new Vector3(Random.Range(-xPos, xPos), 0.0f);

        return midPoint;
    }

    IEnumerator SummonCoroutine()
    {
        var data = Spawner.instance.Data("Common");

        Vector3 buttonWorldPosition = Camera.main.ScreenToWorldPoint(SummonButton.transform.position);
        GameObject trailInstance = Instantiate(TrailPrefab);

        trailInstance.transform.position = buttonWorldPosition;

        Vector3 endPos = Spawner.instance.HolderPosition(data);

        Vector3 startPoint = buttonWorldPosition;
        Vector3 endPoint = endPos;

        Vector3 controlPoint = GenerateRandomControlPoint(startPoint, endPoint);

        float elapsedTime = 0.0f;
        
        while(elapsedTime < trailSpeed)
        {
            float t = elapsedTime / trailSpeed;

            Vector3 curvePosition = CalculateBezierPoint(t, startPoint, controlPoint, endPoint);

            trailInstance.transform.position = new Vector3(curvePosition.x, curvePosition.y, 0.0f);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(trailInstance);
        Spawner.instance.Summon("Common", data);
    }

    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        // 베지어 곡선 공식 : (1-t)^2 & p0 + 2 * (1-t) * t * p1 + t^2 * p2
        return Mathf.Pow(1 - t, 2) * p0 + 2
            * (1 - t) * t * p1 +
            Mathf.Pow(t, 2) * p2;
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

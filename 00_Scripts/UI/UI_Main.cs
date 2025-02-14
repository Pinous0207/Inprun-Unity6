using NUnit.Framework;
using TMPro;
using UnityEditor.Build.Content;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Android.Gradle.Manifest;
using UnityEditor.PackageManager;
using Unity.Netcode;
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
    [SerializeField] private TextMeshProUGUI NickName_T;
    [SerializeField] private TextMeshProUGUI OtherPlayerNickName_T;

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

    [Header("##Upgrade##")]
    [SerializeField] private TextMeshProUGUI u_Money_T;
    [SerializeField] private TextMeshProUGUI[] u_Upgrade_T;
    [SerializeField] private TextMeshProUGUI[] u_Upgrade_Asset_T;

    [Header("##BOSS##")]
    [SerializeField] private GameObject WavePopUp_Object;
    [SerializeField] private GameObject BossWaveCount;
    [SerializeField] private TextMeshProUGUI WaveText_Object;
    [SerializeField] private TextMeshProUGUI WaveBossName;
    [SerializeField] private TextMeshProUGUI BossTimer_T;

    [Header("##GameOver##")]
    [SerializeField] private GameObject GameOverUI;
    [SerializeField] private TextMeshProUGUI PlayerNick_T;
    [SerializeField] private TextMeshProUGUI OtherPlayerNick_T;
    [SerializeField] private TextMeshProUGUI HostDPs;
    [SerializeField] private TextMeshProUGUI ClientDps;
    [SerializeField] private TextMeshProUGUI GameOverWave;

    private void Start()
    {
        Game_Mng.instance.OnMoneyUp += Money_Anim;
        Game_Mng.instance.OnTimerUp += WavePoint;
        Game_Mng.instance.OnGameOver += Gameover;

        SummonButton.onClick.AddListener(() => ClickSummon());

        Game_Mng.instance.SetNickNameOtherPlayer();
        Invoke("SetNickNameUI", 0.5f);
    }

    private void SetNickNameUI()
    {
        NickName_T.text = Cloud_Mng.instance.m_Data.playerName;
        OtherPlayerNickName_T.text = Cloud_Mng.instance.Other_Data.playerName;
    }

    private void Gameover()
    {
        GameOverUI.SetActive(true);
        GameOverWave.text = string.Format("WAVE {0}", Game_Mng.instance.Wave);

        StartCoroutine(SceneLoadDelay());
    }

    IEnumerator SceneLoadDelay()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        HostDPs.text = string.Format("{0:0.0}", Game_Mng.instance.HostDPS);
        PlayerNick_T.text = Cloud_Mng.instance.m_Data.playerName;
        OtherPlayerNick_T.text = Cloud_Mng.instance.Other_Data.playerName;
        ClientDps.text = string.Format("{0:0.0}", Game_Mng.instance.ClientDPS);
        yield return new WaitForSecondsRealtime(3.0f);
        Game_Mng.instance.CleanupNetworkObjects();
        Time.timeScale = 1.0f;
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("MainScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    public void GetWavePopUp(bool GetBoss)
    {
        WavePopUp_Object.SetActive(true);
        WaveText_Object.text = string.Format("WAVE {0}", Game_Mng.instance.Wave);

        if(GetBoss)
        {
            Animator animator = WavePopUp_Object.GetComponent<Animator>();
            animator.SetTrigger("Boss");
            WaveBossName.text = Game_Mng.instance.B_Data.bossData[(int)(Game_Mng.instance.Wave / 10) -1].BossName;
        }
        BossWaveCount.SetActive(GetBoss);
    }

    public void UpgradeButton(int value)
    {
        if (Game_Mng.instance.Money < 30 + Game_Mng.instance.Upgrade[value])
            return;

        Game_Mng.instance.Money -= (30 + Game_Mng.instance.Upgrade[value]);
        Game_Mng.instance.Upgrade[value]++;
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
        MonsterCount_T.text = Game_Mng.instance.MonsterCount.ToString() + " / " + Game_Mng.instance.MonsterLimitCount.ToString();
        MonsterCount_Image.fillAmount = (float)Game_Mng.instance.MonsterCount / Game_Mng.instance.MonsterLimitCount;
        HeroCount_T.text = UpdateHeroCountText();

        Money_T.text = Game_Mng.instance.Money.ToString();
        Summon_T.text = Game_Mng.instance.SummonCount.ToString();
        u_Money_T.text = Game_Mng.instance.Money.ToString();

        for(int i = 0; i < u_Upgrade_T.Length; i++)
        {
            u_Upgrade_T[i].text = "Lv." + (Game_Mng.instance.Upgrade[i]+1).ToString();
            u_Upgrade_Asset_T[i].text = (30 + Game_Mng.instance.Upgrade[i]).ToString();
        }

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
        Timer_T.text = Game_Mng.instance.GetBoss == false ? UpdateTimerText() : "In BOSS!";
        Wave_T.text = "WAVE " + Game_Mng.instance.Wave.ToString();
        BossTimer_T.text = UpdateTimerText();
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

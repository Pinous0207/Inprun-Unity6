using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class Combine_UI : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI NameText;
    [SerializeField] private TextMeshProUGUI DescriptionText;
    public Combine_Scriptable[] CombineDataArray;

    public Image MainCharacterImage;

    public GameObject SubObject;
    public GameObject PlusObject;
    public GameObject ActiveMarkObject;
    public Transform HorizontalContent;

    public Color[] colors;

    List<GameObject> Gorvage = new List<GameObject>();
    List<Hero_Scriptable> HeroPartList = new List<Hero_Scriptable>();
    List<Hero_Holder> holderList = new List<Hero_Holder>();
    Hero_Scriptable mainHero;
    int characterValue;
    
    private void Start()
    {
        CombineDataArray = Resources.LoadAll<Combine_Scriptable>("Combine");
        
        Initalize();
    }

    private void OnEnable()
    {
        Initalize();
    }

    public void Combine()
    {
        for(int i = 0; i < holderList.Count; i++)
        {
            holderList[i].Sell(false);
        }
        Spawner.instance.Summon("Rare", mainHero);
        this.gameObject.SetActive(false);
    }

    private void Initalize()
    {
        mainHero = null;
        CombinePartCheck();
    }
    private void CombinePartCheck()
    {
        if (Gorvage.Count > 0)
        {
            for (int i = 0; i < Gorvage.Count; i++) Destroy(Gorvage[i]);
            Gorvage.Clear();
        }
        HeroPartList.Clear();
        var combinedata = CombineDataArray[characterValue];
        MainCharacterImage.sprite = Utils.GetAtlas(combinedata.MainData.Name);
        mainHero = combinedata.MainData;
        NameText.text = Utils.Localization_Text(Localize.Hero, combinedata.MainData.Name);
        DescriptionText.text = Utils.Localization_Text(Localize.Hero, combinedata.MainData.Name + "_DES");

        for (int i = 0; i < combinedata.SubDatas.Count; i++)
        {
            var go = Instantiate(SubObject, HorizontalContent);
            go.transform.Find("SubCharacter").GetComponent<Image>().sprite =
                Utils.GetAtlas(combinedata.SubDatas[i].Name);

            go.transform.Find("Circle").GetComponent<Image>().color = colors[(int)combinedata.SubDatas[i].rare];

            go.SetActive(true);

            if (i != combinedata.SubDatas.Count - 1)
            {
                var plus = Instantiate(PlusObject, HorizontalContent);
                plus.SetActive(true);
                Gorvage.Add(plus);
            }
            Gorvage.Add(go);
            HeroPartList.Add(combinedata.SubDatas[i]);
        }

        ActiveMarkObject.SetActive(!CanCombine());
    }

    private bool CanCombine(bool isSell = false)
    {
        holderList.Clear();
        List<Hero_Scriptable> heroList = new List<Hero_Scriptable>(HeroPartList);
        foreach(var holder in Spawner.instance.Hero_Holders)
        {
            if (holder.Value.m_Heroes.Count > 0)
            {
                for(int i = 0; i < holder.Value.m_Heroes.Count; i++)
                {
                    for(int j = 0; j < heroList.Count; j++)
                    {
                        if (heroList[j].Name == holder.Value.m_Heroes[i].HeroName)
                        {
                            heroList.Remove(heroList[j]);
                            holderList.Add(holder.Value);
                        }
                    }
                }
            }
        }
        if(heroList.Count > 0)
        {
            holderList.Clear();
        }
        return heroList.Count <= 0 ? true : false;
    }

    public void Arrow(int value)
    {
        characterValue += value;
        if (characterValue < 0)
        {
            characterValue = CombineDataArray.Length - 1;
        }
        else if (characterValue > CombineDataArray.Length - 1)
        {
            characterValue = 0;
        }

        Initalize();
    }
}

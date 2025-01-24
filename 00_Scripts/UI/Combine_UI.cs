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
    public Transform HorizontalContent;

    public Color[] colors;

    List<GameObject> Gorvage = new List<GameObject>();

    int characterValue;
    
    private void Start()
    {
        CombineDataArray = Resources.LoadAll<Combine_Scriptable>("Combine");
        
        Initalize();
    }

    private void Initalize()
    {
        if(Gorvage.Count > 0)
        {
            for (int i = 0; i < Gorvage.Count; i++) Destroy(Gorvage[i]);
            Gorvage.Clear();
        }

        var combinedata = CombineDataArray[characterValue];
        MainCharacterImage.sprite = Utils.GetAtlas(combinedata.MainData.Name);

        NameText.text = Utils.Localization_Text(Localize.Hero, combinedata.MainData.Name);
        DescriptionText.text = Utils.Localization_Text(Localize.Hero, combinedata.MainData.Name + "_DES");

        for (int i = 0; i < combinedata.SubDatas.Count; i++)
        {
            var go = Instantiate(SubObject, HorizontalContent);

            go.transform.Find("SubCharacter").GetComponent<Image>().sprite =
                Utils.GetAtlas(combinedata.SubDatas[i].Name);


            go.transform.Find("Circle").GetComponent<Image>().color = colors[(int)combinedata.SubDatas[i].rare];


            go.SetActive(true);

            if(i != combinedata.SubDatas.Count-1)
            {
                var plus = Instantiate(PlusObject, HorizontalContent);
                plus.SetActive(true);
                Gorvage.Add(plus);
            }
            Gorvage.Add(go);
        }
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

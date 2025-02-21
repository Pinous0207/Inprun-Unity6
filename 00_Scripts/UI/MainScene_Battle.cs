using TMPro;
using UnityEngine;

public class MainScene_Battle : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Level_T;
    [SerializeField] private TextMeshProUGUI Wave_T;

    public void Initalize()
    {
        Level_T.text = "Lv." + Cloud_Mng.instance.m_Data.level.ToString();
        if (Cloud_Mng.instance.m_Data.Wave == 0)
        {
            Wave_T.transform.parent.gameObject.SetActive(false);
        }
        Wave_T.text = Cloud_Mng.instance.m_Data.Wave.ToString();
    }
}

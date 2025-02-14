using TMPro;
using UnityEngine;

public class MainScene_Canvas : MonoBehaviour
{
    public static MainScene_Canvas instance = null;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    [SerializeField] private TextMeshProUGUI Level_T;

    public void Initalize()
    {
        Level_T.text = "Lv." + Cloud_Mng.instance.m_Data.level.ToString();
    }
}

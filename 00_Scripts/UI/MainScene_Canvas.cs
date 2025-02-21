using TMPro;
using UnityEngine;

public class MainScene_Canvas : MonoBehaviour
{
    public static MainScene_Canvas instance = null;
    public static MainScene_Battle Battle;
    public static Bottom_UIs Bottom;


    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }

        Battle = GetComponentInChildren<MainScene_Battle>(true);
        Bottom = GetComponentInChildren<Bottom_UIs>();
    }

    public void Initalize()
    {
        Battle.Initalize();
        Bottom.GetPanel(2);
    }

}

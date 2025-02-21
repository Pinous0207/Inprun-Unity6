using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
[System.Serializable]
public class Panel
{
    public GameObject MainPanel;
    public Image PanelImage;
    public Animator animator;
}
public class Bottom_UIs : MonoBehaviour
{
    private List<Panel> Panels = new List<Panel>();
    [SerializeField] private GameObject[] mainPanels;
    [SerializeField] private Button[] buttons;
    [SerializeField] private Color ActiveColor, NoneActiveColor;

    private void Start()
    {
        for(int i = 0; i < buttons.Length; i++)
        {
            Panel panel = new Panel();
            panel.MainPanel = mainPanels[i];
            panel.PanelImage = buttons[i].GetComponent<Image>();
            panel.animator = buttons[i].GetComponent<Animator>();

            Panels.Add(panel);

            int index = i;
            buttons[i].onClick.AddListener(() => GetPanel(index));
        }
    }

    public void GetPanel(int value)
    {
        for (int i = 0; i < Panels.Count; i++)
        {
            bool isActive = value == i;
            Panels[i].MainPanel.SetActive(isActive);

            AnimatorStateInfo stateInfo = Panels[i].animator.GetCurrentAnimatorStateInfo(0);
            bool isCurrentlyOn = stateInfo.IsName("Bottom_Panel_On");

            if(isActive)
            {
                Panels[i].animator.Play("Bottom_Panel_On");
            }
            else if(isCurrentlyOn)
            {
                Panels[i].animator.Play("Bottom_Panel_Down");
            }

            Panels[i].PanelImage.color = isActive == true ? ActiveColor : NoneActiveColor;
        }
    }

}

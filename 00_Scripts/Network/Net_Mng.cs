using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public partial class Net_Mng : MonoBehaviour
{
    // Lobby -> �÷��̾ ���ϴ� ������ ã�ų�, �� ������ ����� ����� �� �ִ�
    // Relay -> ��Ī�� �÷��̾���� Relay�� Join Code�� ����Ǿ�, ȣ��Ʈ-Ŭ���̾�Ʈ ������� �ǽð� ��Ƽ�÷��� ȯ���� ����
    private Lobby currentLobby;

    private const int maxPlayers = 2;
    private string gameplaySceneName = "GamePlayScene";
    public Button StartMatchButton;
    public GameObject Matching_Object;
    public Button CancelButton;

    public GameObject SetNicknameUI;
    public TMP_InputField NickNameInputField;

    private async void Start() // �񵿱� -> ���ÿ� �Ͼ�� �ʴ´�.
    {
        await UnityServices.InitializeAsync();
        if(!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            var loadData = await Cloud_Mng.instance.LoadPlayerData();
            Cloud_Mng.instance.m_Data = loadData;

            if(string.IsNullOrEmpty(Cloud_Mng.instance.m_Data.playerName))
            {
                SetNicknameUI.SetActive(true);
            }

            MainScene_Canvas.instance.Initalize();
        }
        else
        {
            MainScene_Canvas.instance.Initalize();
        }

        StartMatchButton.onClick.AddListener(() => StartMatchmaking());
        //JoinMatchButton.onClick.AddListener(() => JoinGameWithCode(fieldText.text));
    }

    public void SetNickName()
    {
        string nickname = NickNameInputField.text;
        if(string.IsNullOrEmpty(nickname))
        {
            Debug.LogError("�г����� �Է����ּ���.");
            return;
        }
        if(nickname.Length < 3 || nickname.Length > 10)
        {
            Debug.LogError("�г����� 3���� �̻� 10���� ���Ϸ� ������ּž��մϴ�.");
            return;
        }
        
        Cloud_Mng.instance.m_Data.playerName = NickNameInputField.text;
        SetNicknameUI.SetActive(false);

        Cloud_Mng.instance.Save();
    }
}

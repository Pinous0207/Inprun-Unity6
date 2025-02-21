using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public partial class Net_Mng : MonoBehaviour
{
    // Lobby -> 플레이어가 원하는 게임을 찾거나, 새 게임을 만들고 대기할 수 있는
    // Relay -> 매칭된 플레이어들의 Relay의 Join Code로 연결되어, 호스트-클라이언트 방식으로 실시간 멀티플레이 환경을 유지
    private Lobby currentLobby;

    private const int maxPlayers = 2;
    private string gameplaySceneName = "GamePlayScene";
    public Button StartMatchButton;
    public GameObject Matching_Object;
    public Button CancelButton;

    public GameObject SetNicknameUI;
    public TMP_InputField NickNameInputField;

    private async void Start() // 비동기 -> 동시에 일어나지 않는다.
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
            Debug.LogError("닉네임을 입력해주세요.");
            return;
        }
        if(nickname.Length < 3 || nickname.Length > 10)
        {
            Debug.LogError("닉네임은 3글자 이상 10글자 이하로 만들어주셔야합니다.");
            return;
        }
        
        Cloud_Mng.instance.m_Data.playerName = NickNameInputField.text;
        SetNicknameUI.SetActive(false);

        Cloud_Mng.instance.Save();
    }
}

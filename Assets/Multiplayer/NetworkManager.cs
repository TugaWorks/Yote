using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public Button playButton;
    public Button hostButton;
    public Button joinButton;
    public Button ComputerButton;
    public Button cancelButton;
    public TMP_InputField roomCodeInputField;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI textJoinGameHint;

    private string roomCode;
    private bool isHosting = false;
    private bool isConnecting = false; // Flag para saber se está tentando conectar
    private void Start()
    {
        playButton.onClick.AddListener(OnPlayButtonClicked);
        hostButton.onClick.AddListener(OnHostButtonClicked);
        joinButton.onClick.AddListener(OnJoinButtonClicked);
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
        statusText.text = "";
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void OnPlayButtonClicked()
    {
        PlaySoundSelect();
        playButton.interactable = false;
        hostButton.interactable = false;
        joinButton.interactable = false;
        ComputerButton.interactable = false;
        statusText.text = "Connecting to the server...";
        isHosting = false;
        isConnecting = true; // Agora está conectando

        cancelButton.gameObject.SetActive(true); // Esconde o botão de cancelar
        PhotonNetwork.ConnectUsingSettings();
    }

    private void OnHostButtonClicked()
    {
        PlaySoundSelect();
        playButton.interactable = false;
        hostButton.interactable = false;
        joinButton.interactable = false;
        ComputerButton.interactable = false;
        statusText.text = "Connecting to the server...";
        isHosting = true;
        isConnecting = true; // Agora está conectando

        cancelButton.gameObject.SetActive(true); // Esconde o botão de cancelar
        PhotonNetwork.ConnectUsingSettings();
    }

    private void OnJoinButtonClicked()
    {
        PlaySoundSelect();
        string enteredRoomCode = roomCodeInputField.text;
        if (!string.IsNullOrEmpty(enteredRoomCode))
        {

            playButton.interactable = false;
            hostButton.interactable = false;
            joinButton.interactable = false;
            ComputerButton.interactable = false;
            statusText.text = "Connecting to the server...";
            roomCode = enteredRoomCode;
            isConnecting = true; // Agora está conectando
            cancelButton.gameObject.SetActive(true); // Esconde o botão de cancelar
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            statusText.text = "Please, insert a valid code";
        }
    }

    public override void OnConnectedToMaster()
    {
        if (isHosting)
        {
            statusText.text = "Connected! Creating room...";
            roomCode = GenerateRoomCode();
            PhotonNetwork.CreateRoom(roomCode, new RoomOptions { MaxPlayers = 2 });
        }
        else if (!string.IsNullOrEmpty(roomCode))
        {
            statusText.text = "Connected! Entering room...";
            PhotonNetwork.JoinRoom(roomCode);
        }
        else
        {
            statusText.text = "Conectado! Entrando no Lobby...";
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnJoinedLobby()
    {
        if (!isHosting && string.IsNullOrEmpty(roomCode))
        {
            statusText.text = "In Lobby. Waiting for other players...";
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        statusText.text = "No room avaible. Creating new room...";
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 });
    }

    public override void OnJoinedRoom()
    {
        statusText.text = "Entered in room: " + PhotonNetwork.CurrentRoom.Name + ". Waiting other players...";
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            StartGame();
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            StartGame();
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        statusText.text = "Failed entering room: " + message;
        joinButton.interactable = true;
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        statusText.text = "Failed entering room: " + message;
        hostButton.interactable = true;
        joinButton.interactable = true;
    }

    private void StartGame()
    {
        statusText.text = "Player found! Game will begin";
        PhotonNetwork.LoadLevel("MultiplayerScene"); // Certifique-se de ter uma cena chamada "MultiplayerScene"
    }
    public void StartGameWithPC()
    {
        SceneManager.LoadScene("VsComputerScene", LoadSceneMode.Single);
    }
    private string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        char[] stringChars = new char[6];
        System.Random random = new System.Random();

        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        return new string(stringChars);
    }

    public bool IsRoomFull()
    {
        return PhotonNetwork.CurrentRoom.PlayerCount == 2;
    }

    public void PlaySoundSelect()
    {
        SoundManager.instance.PlaySoundSelect();
    }

    public void ShowHintJoinGame()
    {
        textJoinGameHint.gameObject.SetActive(true);
    }

    public void HideHintJoinGame()
    {
        textJoinGameHint.gameObject.SetActive(false);
    }

    // Método para cancelar a conexão
    private void OnCancelButtonClicked()
    {
        if (isConnecting)
        {
            // Se está conectando, cancelar a conexão e reativar os botões
            statusText.text = "Connection canceled";
            PhotonNetwork.Disconnect(); // Desconectar do Photon

            // Resetar variáveis de estado
            roomCode = string.Empty; // Limpar o código da sala para evitar erros
            isHosting = false; // Não está mais em modo de host
            isConnecting = false; // Não está mais conectando

            // Reativar botões
            playButton.interactable = true;
            hostButton.interactable = true;
            joinButton.interactable = true;
            ComputerButton.interactable = true;
            cancelButton.gameObject.SetActive(false); // Esconder o botão de cancelar
        }
    }


    public void ExitGame()
    {
        PlaySoundSelect();
        Application.Quit();
    }
}

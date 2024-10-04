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
        statusText.text = "Conectando ao servidor...";
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
        statusText.text = "Conectando ao servidor...";
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
            statusText.text = "Conectando ao servidor...";
            roomCode = enteredRoomCode;
            isConnecting = true; // Agora está conectando
            cancelButton.gameObject.SetActive(true); // Esconde o botão de cancelar
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            statusText.text = "Por favor, insira um código de sala válido.";
        }
    }

    public override void OnConnectedToMaster()
    {
        if (isHosting)
        {
            statusText.text = "Conectado! Criando sala...";
            roomCode = GenerateRoomCode();
            PhotonNetwork.CreateRoom(roomCode, new RoomOptions { MaxPlayers = 2 });
        }
        else if (!string.IsNullOrEmpty(roomCode))
        {
            statusText.text = "Conectado! Entrando na sala...";
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
            statusText.text = "No Lobby. Aguardando outros jogadores...";
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        statusText.text = "Nenhuma sala disponível. Criando uma nova sala...";
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 });
    }

    public override void OnJoinedRoom()
    {
        statusText.text = "Entrou na sala: " + PhotonNetwork.CurrentRoom.Name + ". Aguardando outros jogadores...";
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
        statusText.text = "Falha ao entrar na sala: " + message;
        joinButton.interactable = true;
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        statusText.text = "Falha ao criar a sala: " + message;
        hostButton.interactable = true;
        joinButton.interactable = true;
    }

    private void StartGame()
    {
        statusText.text = "Jogador encontrado! Iniciando o jogo...";
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
            statusText.text = "Conexão cancelada.";
            PhotonNetwork.Disconnect(); // Desconectar do Photon

            // Reativar botões
            playButton.interactable = true;
            hostButton.interactable = true;
            joinButton.interactable = true;
            ComputerButton.interactable = true;
            cancelButton.gameObject.SetActive(false); // Esconde o botão de cancelar
            isConnecting = false; // Não está mais conectando
        }
    }
}

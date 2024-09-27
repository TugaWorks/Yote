using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviourPunCallbacks
{
    public Photon.Realtime.Player photonPlayer;
    public List<PieceScript> PlayerPieces = new List<PieceScript>();

    public List<PieceScript> PlayerPiecesInside = new List<PieceScript>();
    public List<PieceScript> PlayerPiecesOutside = new List<PieceScript>();

    public bool isportugal = false;
    public Material playerMaterial;
    private PhotonView photonView;
    void Awake()
    {
        DontDestroyOnLoad(this);
        photonView = GetComponent<PhotonView>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void Initialize(Photon.Realtime.Player player)
    {
        photonPlayer = player;
    }
    // Update is called once per frame
    void Update()
    {
       
    }

    // Método que é chamado quando o local player sai da sala
    public override void OnLeftRoom()
    {
        Debug.Log("Você saiu da partida.");

        // Notificar o outro jogador que o oponente saiu
        photonView.RPC("OpponentLeftRoom", RpcTarget.Others);

        // Voltar ao menu principal para o jogador que saiu
        GoToMainMenu();
    }

    // Função para voltar ao menu principal
    private void GoToMainMenu()
    {
        SceneManager.LoadScene("LobbyScene");
    }

    // RPC para ser chamado no jogador restante
    [PunRPC]
    public void OpponentLeftRoom()
    {
        Debug.Log("Oponente saiu da partida. Voltando para o menu...");

        // Voltar ao menu principal para o jogador que ainda está na sala
        //GoToMainMenu();
    }
}

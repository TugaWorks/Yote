using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;

    [SerializeField] TextMeshProUGUI currentPlayerText;
    [SerializeField] TextMeshProUGUI TextWinLost;
    [SerializeField] TextMeshProUGUI Player1TextCountPieces;
    [SerializeField] TextMeshProUGUI Player2TextCountPieces;
    [SerializeField] Button endTurnButton;
    [SerializeField] GameObject BoardManagerPrefab;  // Referência ao prefab do BoardManager
    [SerializeField] Transform cameraPositionPlayer1; // Posição da câmera para o Jogador 1
    [SerializeField] Transform cameraPositionPlayer2;
    public List<Player> playerList = new List<Player>();
    public Player player1Prefab;
    public Player player2Prefab;
    public Transform player1SpawnPoint;
    public Transform player2SpawnPoint;

    public int currentPlayerIndex = 0;
    public bool isOnlineMode = false;
    private PhotonView photonView;
    private Player localPlayer;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        photonView = GetComponent<PhotonView>();
       
    }

    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            isOnlineMode = true;

           

            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Instantiate(BoardManagerPrefab.name, Vector3.zero, Quaternion.identity);
            }

            // Defina a posição da câmera baseada no Master Client
            if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
            {
                Camera.main.transform.position = cameraPositionPlayer1.position;
                Camera.main.transform.rotation = cameraPositionPlayer1.rotation;
            }
            else if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
            {
                Camera.main.transform.position = cameraPositionPlayer2.position;
                Camera.main.transform.rotation = cameraPositionPlayer2.rotation;
            }

            
           
        }
        else
        {
            isOnlineMode = false;
            playerList.Add(player1Prefab);
            playerList.Add(player2Prefab);

            
        }

        //endTurnButton.onClick.AddListener(EndTurn);
        UpdateCurrentPlayerText();
        UpdateButtonInteractable();
    }
    public Player GetPlayer(int actorNumber)
    {
        return playerList.Find(player => player.photonPlayer.ActorNumber == actorNumber);
    }
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        playerList.Add(new Player { photonPlayer = newPlayer });

        if (playerList.Count == 2)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                //BoardManager.instance.PlacePlayerPieces();
            }
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        playerList.RemoveAll(player => player.photonPlayer == otherPlayer);
    }

    private void UpdateCurrentPlayerText()
    {
        currentPlayerText.text = $"Current Player: {(currentPlayerIndex == 0 ? "Player 1" : "Player 2")}";
    }

    private void UpdateButtonInteractable()
    {
        if (isOnlineMode)
        {
            endTurnButton.interactable = (PhotonNetwork.LocalPlayer.ActorNumber - 1) == currentPlayerIndex;
        }
        else
        {
            endTurnButton.interactable = true;
        }
    }

    public void EndTurn()
    {
        if (isOnlineMode && (PhotonNetwork.LocalPlayer.ActorNumber - 1) != currentPlayerIndex)
        {
            return;
        }

        currentPlayerIndex = (currentPlayerIndex + 1) % 2;
        UpdateCurrentPlayerText();
        UpdateButtonInteractable();

        if (isOnlineMode)
        {
            photonView.RPC("NotifyEndTurn", RpcTarget.Others, currentPlayerIndex);
        }
    }

    [PunRPC]
    private void NotifyEndTurn(int newCurrentPlayerIndex)
    {
        currentPlayerIndex = newCurrentPlayerIndex;
        UpdateCurrentPlayerText();
        UpdateButtonInteractable();
    }

    public void HighlightAllUnoccupiedCells()
    {
        if (BoardManager.instance != null)
        {
            foreach (CellScripts cell in BoardManager.instance.GetAllCells())
            {
                if (!cell.isOccupied)
                {
                    cell.GetComponent<Renderer>().material = cell.MaterialHighlight;
                    cell.isHighlighted = true;
                }
            }
        }
    }

    public void HighlightAdjacentCells(CellScripts cell, bool isPlayerOnePiece)
    {
        ClearHighlightedCells();
        if (cell != null)
        {
            HighlightCell(cell.cellAbove);
            HighlightCell(cell.cellBelow);
            HighlightCell(cell.cellLeft);
            HighlightCell(cell.cellRight);

            CheckAndHighlightJump(cell, cell.cellAbove, cell.cellAbove?.cellAbove, isPlayerOnePiece);
            CheckAndHighlightJump(cell, cell.cellBelow, cell.cellBelow?.cellBelow, isPlayerOnePiece);
            CheckAndHighlightJump(cell, cell.cellLeft, cell.cellLeft?.cellLeft, isPlayerOnePiece);
            CheckAndHighlightJump(cell, cell.cellRight, cell.cellRight?.cellRight, isPlayerOnePiece);
        }
    }

    public void CheckAndHighlightJump(CellScripts currentCell, CellScripts adjacentCell, CellScripts jumpCell, bool isPlayerOnePiece)
    {
        if (adjacentCell != null && jumpCell != null && adjacentCell.isOccupied)
        {
            PieceScript piece = adjacentCell.GetComponentInChildren<PieceScript>();
            if (piece != null && piece.isPlayerOnePiece != isPlayerOnePiece && !jumpCell.isOccupied)
            {
                HighlightCell(jumpCell);
            }
        }
    }

    public void ClearHighlightedCells()
    {
        BoardManager boardManager = FindObjectOfType<BoardManager>();
        if (boardManager != null)
        {
            foreach (CellScripts cell in boardManager.GetAllCells())
            {
                if (cell.isHighlighted)
                {
                    cell.GetComponent<Renderer>().material = cell.OriginalMaterial;
                    cell.isHighlighted = false;
                    StartCoroutine(cell.ResetCellPositionCoroutine());
                }
            }
        }
    }

    public void HighlightCell(CellScripts cell)
    {
        if (cell != null && !cell.isOccupied)
        {
            cell.GetComponent<Renderer>().material = cell.MaterialHighlight;
            cell.isHighlighted = true;
            StartCoroutine(cell.ElevateCellCoroutine());
        }
    }

    public void RemovePiece(PieceScript piece)
    {
        // Identifique o jogador que perdeu a peça e remova a peça da sua lista
        Player currentPlayer = playerList[piece.isPlayerOnePiece ? 0 : 1];
        currentPlayer.PlayerPieces.Remove(piece.gameObject);

        // Identifique o outro jogador
        Player otherPlayer = playerList[piece.isPlayerOnePiece ? 1 : 0];

        // Destrói a peça
        Destroy(piece.gameObject);

        // Verifique se o jogador atual (que perdeu a peça) tem menos de uma peça
        if (currentPlayer.PlayerPieces.Count < 12)
        {
            Debug.Log($"{otherPlayer.photonPlayer.NickName} ganhou o jogo!"); // Mensagem de debug indicando que o outro jogador ganhou

            // Obtenha o ID do jogador atual
            int localPlayerId = PhotonNetwork.LocalPlayer.ActorNumber;

            // Determine a mensagem de acordo com quem perdeu todas as peças
            string localPlayerMessage = currentPlayer.photonPlayer.ActorNumber == localPlayerId ? "You lost" : "You win";
            string otherPlayerMessage = currentPlayer.photonPlayer.ActorNumber == localPlayerId ? "You win" : "You lost";

            // Determine a mensagem de acordo com quem perdeu todas as peças
            bool isLocalPlayer = currentPlayer.photonPlayer.ActorNumber == localPlayerId;
            string message = isLocalPlayer ? "You lost" : "You win";
            TextWinLost.gameObject.SetActive(true);
            TextWinLost.text = message;
            // Envie a mensagem correta para cada jogador
            photonView.RPC("ShowEndGameMessage", RpcTarget.Others, localPlayerId, localPlayerMessage, otherPlayerMessage);
            
        }
    }

    [PunRPC]
    private void ShowEndGameMessage(int localPlayerId, string localPlayerMessage, string otherPlayerMessage)
    {

        // Obtenha o ID do jogador local
        int localId = PhotonNetwork.LocalPlayer.ActorNumber;

        // Determine a mensagem correta para o jogador local
        string message = localId == localPlayerId ? localPlayerMessage : otherPlayerMessage;

        // Aqui você pode adicionar a lógica para exibir a mensagem na UI
        Debug.Log(message); // Apenas para debug, você pode substituir isso pela lógica da UI
        TextWinLost.gameObject.SetActive(true);
        TextWinLost.text = message; // Certifique-se de ter uma referência ao componente de texto UI
        Interstitial.instance.ShowAd();
    }
    public int GetRow(CellScripts cell)
    {
        string[] parts = cell.name.Split('_');
        return int.Parse(parts[1]);
    }

    public int GetCol(CellScripts cell)
    {
        string[] parts = cell.name.Split('_');
        return int.Parse(parts[2]);
    }
    public CellScripts GetMiddleCell(CellScripts startCell, CellScripts endCell)
    {
        int startRow = GameManager.instance.GetRow(startCell);
        int startCol = GameManager.instance.GetCol(startCell);
        int endRow = GameManager.instance.GetRow(endCell);
        int endCol = GameManager.instance.GetCol(endCell);

        int middleRow = (startRow + endRow) / 2;
        int middleCol = (startCol + endCol) / 2;

        return BoardManager.instance.GetCell(middleRow, middleCol);
    }

    public bool IsPlayerTurn(bool isPlayerOnePiece)
    {
        return (isPlayerOnePiece && currentPlayerIndex == 0) || (!isPlayerOnePiece && currentPlayerIndex == 1);
    }

    public string GetCaptureDirection(CellScripts startCell, CellScripts endCell)
    {
        int startRow = GetRow(startCell);
        int startCol = GetCol(startCell);
        int endRow = GetRow(endCell);
        int endCol = GetCol(endCell);

        int rowDiff = endRow - startRow;
        int colDiff = endCol - startCol;

        if (Mathf.Abs(rowDiff) > Mathf.Abs(colDiff))
        {
            return rowDiff > 0 ? "Down" : "Up";
        }
        else
        {
            return colDiff > 0 ? "Right" : "Left";
        }
    }
    public List<PieceScript> ListOfpieceThatHasAnotherPieceToCapture = new List<PieceScript>();
    public bool VerifyIfHasNoPieceToCapture(bool isPlayerOne , int playerId)
    {
        ListOfpieceThatHasAnotherPieceToCapture.Clear();
        
        Player playerPlaying = PhotonView.Find(playerId).GetComponent<Player>();
        playerPlaying.PlayerPiecesInside.Add(this.gameObject);
        if(playerPlaying.PlayerPiecesInside.Count > 0) { 
            foreach (GameObject piece in playerPlaying.PlayerPiecesInside)
            {
                CellScripts currentCell = piece.GetComponent<PieceScript>().currentCell;

                if (currentCell != null)
                {
                    // Verificar direções para capturar
                    CheckAndAddPieceToCaptureList(currentCell, currentCell.cellAbove, currentCell.cellAbove?.cellAbove, isPlayerOne, piece.GetComponent<PieceScript>());
                    CheckAndAddPieceToCaptureList(currentCell, currentCell.cellBelow, currentCell.cellBelow?.cellBelow, isPlayerOne, piece.GetComponent<PieceScript>());
                    CheckAndAddPieceToCaptureList(currentCell, currentCell.cellLeft, currentCell.cellLeft?.cellLeft, isPlayerOne, piece.GetComponent<PieceScript>());
                    CheckAndAddPieceToCaptureList(currentCell, currentCell.cellRight, currentCell.cellRight?.cellRight, isPlayerOne, piece.GetComponent<PieceScript>());
                }
            }
        }

        return ListOfpieceThatHasAnotherPieceToCapture.Count > 0;
    }

    private void CheckAndAddPieceToCaptureList(CellScripts currentCell, CellScripts adjacentCell, CellScripts cellBeyond, bool isPlayerOne, PieceScript piece)
    {
        if (adjacentCell != null && adjacentCell.currentPiece != null && cellBeyond != null && cellBeyond.currentPiece == null)
        {
            if (adjacentCell.currentPiece.isPlayerOnePiece != isPlayerOne)
            {
                if (!ListOfpieceThatHasAnotherPieceToCapture.Contains(piece))
                {
                    ListOfpieceThatHasAnotherPieceToCapture.Add(piece);

                }
            }
        }
    }
}



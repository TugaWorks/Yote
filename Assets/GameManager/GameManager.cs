using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;

    [SerializeField] TextMeshProUGUI currentPlayerText;
    [SerializeField] TextMeshProUGUI TextWinLost;
    [SerializeField] TextMeshProUGUI Player1TextCountPieces;
    [SerializeField] TextMeshProUGUI Player2TextCountPieces;
    [SerializeField] Button endTurnButton;
    [SerializeField] GameObject BoardManagerPrefab;  // Refer�ncia ao prefab do BoardManager
    [SerializeField] Transform cameraPositionPlayer1; // Posi��o da c�mera para o Jogador 1
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

            // Defina a posi��o da c�mera baseada no Master Client
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
    public bool CheckIfPieceIsStuck(PieceScript piece)
    {
        CellScripts currentCell = piece.currentCell;

        // Verificar se a c�lula atual da pe�a existe
        if (currentCell != null)
        {
            // Verificar c�lula � esquerda
            if (currentCell.cellLeft != null)
            {
                if (!currentCell.cellLeft.isOccupied)
                {
                    return false; // H� um espa�o dispon�vel � esquerda
                }
                if (currentCell.cellLeft.currentPiece)
                {
                    if (currentCell.cellLeft.isOccupied && currentCell.cellLeft.currentPiece.isPlayerOnePiece != piece.isPlayerOnePiece)
                    {
                        // A c�lula � esquerda est� ocupada por uma pe�a inimiga, verificar se pode capturar
                        if (currentCell.cellLeft.cellLeft != null && !currentCell.cellLeft.cellLeft.isOccupied)
                        {
                            return false; // Pode capturar a pe�a inimiga � esquerda
                        }
                    }
                }
            }

            // Verificar c�lula � direita
            if (currentCell.cellRight != null)
            {
                if (!currentCell.cellRight.isOccupied)
                {
                    return false; // H� um espa�o dispon�vel � direita
                }
                if (currentCell.cellRight.currentPiece)
                {
                    if (currentCell.cellRight.isOccupied && currentCell.cellRight.currentPiece.isPlayerOnePiece != piece.isPlayerOnePiece)
                    {
                        // A c�lula � direita est� ocupada por uma pe�a inimiga, verificar se pode capturar
                        if (currentCell.cellRight.cellRight != null && !currentCell.cellRight.cellRight.isOccupied)
                        {
                            return false; // Pode capturar a pe�a inimiga � direita
                        }
                    }
                }
            }

            // Verificar c�lula acima
            if (currentCell.cellAbove != null)
            {
                if (!currentCell.cellAbove.isOccupied)
                {
                    return false; // H� um espa�o dispon�vel acima
                }
                if (currentCell.cellAbove.currentPiece)
                {
                    if (currentCell.cellAbove.isOccupied && currentCell.cellAbove.currentPiece.isPlayerOnePiece != piece.isPlayerOnePiece)
                    {
                        // A c�lula acima est� ocupada por uma pe�a inimiga, verificar se pode capturar
                        if (currentCell.cellAbove.cellAbove != null && !currentCell.cellAbove.cellAbove.isOccupied)
                        {
                            return false; // Pode capturar a pe�a inimiga acima
                        }
                    }
                }
            }

            // Verificar c�lula abaixo
            if (currentCell.cellBelow != null)
            {
                if (!currentCell.cellBelow.isOccupied)
                {
                    return false; // H� um espa�o dispon�vel abaixo
                }
                if (currentCell.cellBelow.currentPiece)
                {
                    if (currentCell.cellBelow.isOccupied && currentCell.cellBelow.currentPiece.isPlayerOnePiece != piece.isPlayerOnePiece)
                    {
                        // A c�lula abaixo est� ocupada por uma pe�a inimiga, verificar se pode capturar
                        if (currentCell.cellBelow.cellBelow != null && !currentCell.cellBelow.cellBelow.isOccupied)
                        {
                            return false; // Pode capturar a pe�a inimiga abaixo
                        }
                    }
                }
            }
        }

        // Se todas as verifica��es falharem, a pe�a est� presa
        return true;
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

        int HowManyPiecesPlayerHasStucked = 0;
        if(playerList[currentPlayerIndex].PlayerPiecesInside.Count > 0)
        {
            foreach (PieceScript piece in playerList[currentPlayerIndex].PlayerPiecesInside)
            {

                if (CheckIfPieceIsStuck(piece))
                {
                    HowManyPiecesPlayerHasStucked++;
                }
            }
        }
        

        if (HowManyPiecesPlayerHasStucked == playerList[currentPlayerIndex].PlayerPieces.Count)
        {
            Debug.Log("Player ganhou!");

            Player currentPlayer = playerList[currentPlayerIndex];
            // Obtenha o ID do jogador atual
            int localPlayerId = PhotonNetwork.LocalPlayer.ActorNumber;

            // Determine a mensagem de acordo com quem perdeu todas as pe�as
            string localPlayerMessage = currentPlayer.photonPlayer.ActorNumber == localPlayerId ? "You lost" : "You win";
            string otherPlayerMessage = currentPlayer.photonPlayer.ActorNumber == localPlayerId ? "You win" : "You lost";

            // Determine a mensagem de acordo com quem perdeu todas as pe�as
            bool isLocalPlayer = currentPlayer.photonPlayer.ActorNumber == localPlayerId;
            string message = isLocalPlayer ? "You lost" : "You won";
            TextWinLost.gameObject.SetActive(true);
            TextWinLost.text = message;
            StartCoroutine(ReturnToMainMenuAfterDelay());
            // Envie a mensagem correta para cada jogador
            photonView.RPC("ShowEndGameMessage", RpcTarget.Others, localPlayerId, localPlayerMessage, otherPlayerMessage);
        }
        
        

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

    public void HighlightAdjacentCells(CellScripts cell, bool isPlayerOnePiece, PieceScript currentPiece)
    {
        ClearHighlightedCells();
        if (cell != null)
        {
            //HighlightCell(cell.cellAbove);
            //HighlightCell(cell.cellBelow);
            //HighlightCell(cell.cellLeft);
            //HighlightCell(cell.cellRight);

            //CheckAndHighlightJump(cell, cell.cellAbove, cell.cellAbove?.cellAbove, isPlayerOnePiece);
            //CheckAndHighlightJump(cell, cell.cellBelow, cell.cellBelow?.cellBelow, isPlayerOnePiece);
            //CheckAndHighlightJump(cell, cell.cellLeft, cell.cellLeft?.cellLeft, isPlayerOnePiece);
            //CheckAndHighlightJump(cell, cell.cellRight, cell.cellRight?.cellRight, isPlayerOnePiece);

            if (GameManager.instance.ListOfpieceThatHasAnotherPieceToCapture.Contains(currentPiece))
            {
                // Apenas destacar as c�lulas al�m de uma pe�a inimiga
                CheckAndHighlightJump(cell, cell.cellAbove, cell.cellAbove?.cellAbove, isPlayerOnePiece);
                CheckAndHighlightJump(cell, cell.cellBelow, cell.cellBelow?.cellBelow, isPlayerOnePiece);
                CheckAndHighlightJump(cell, cell.cellLeft, cell.cellLeft?.cellLeft, isPlayerOnePiece);
                CheckAndHighlightJump(cell, cell.cellRight, cell.cellRight?.cellRight, isPlayerOnePiece);
            }
            else
            {
                // Caso n�o esteja na lista, destaca as c�lulas adjacentes normais
                if (cell != null)
                {
                    if (cell.cellAbove != null && !cell.cellAbove.isOccupied) cell.cellAbove.Highlight();
                    if (cell.cellBelow != null && !cell.cellBelow.isOccupied) cell.cellBelow.Highlight();
                    if (cell.cellLeft != null && !cell.cellLeft.isOccupied) cell.cellLeft.Highlight();
                    if (cell.cellRight != null && !cell.cellRight.isOccupied) cell.cellRight.Highlight();
                }
            }
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
        // Identifique o jogador que perdeu a pe�a e remova a pe�a da sua lista
        Player currentPlayer = playerList[piece.isPlayerOnePiece ? 0 : 1];
        currentPlayer.PlayerPieces.Remove(piece);

        // Identifique o outro jogador
        Player otherPlayer = playerList[piece.isPlayerOnePiece ? 1 : 0];

        // Envie o ViewID da pe�a e o ActorNumber do jogador que a possui
        photonView.RPC("RemovePieceOnline", RpcTarget.Others, piece.photonView.ViewID, currentPlayer.photonPlayer.ActorNumber, piece.currentCell.photonView.ViewID);

        // Destroi a pe�a localmente
        Destroy(piece.gameObject);
        // Verifique se o jogador atual (que perdeu a pe�a) tem menos de 12 pe�as
        if (currentPlayer.PlayerPieces.Count < 1)
        {
            Debug.Log($"{otherPlayer.photonPlayer.NickName} ganhou o jogo!"); // Mensagem de debug indicando que o outro jogador ganhou

            // Obtenha o ID do jogador atual
            int localPlayerId = PhotonNetwork.LocalPlayer.ActorNumber;

            // Determine a mensagem de acordo com quem perdeu todas as pe�as
            string localPlayerMessage = currentPlayer.photonPlayer.ActorNumber == localPlayerId ? "You lost" : "You win";
            string otherPlayerMessage = currentPlayer.photonPlayer.ActorNumber == localPlayerId ? "You win" : "You lost";

            // Determine a mensagem de acordo com quem perdeu todas as pe�as
            bool isLocalPlayer = currentPlayer.photonPlayer.ActorNumber == localPlayerId;
            string message = isLocalPlayer ? "You lost" : "You won";
            TextWinLost.gameObject.SetActive(true);
            TextWinLost.text = message;
            StartCoroutine(ReturnToMainMenuAfterDelay());
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

        // Aqui voc� pode adicionar a l�gica para exibir a mensagem na UI
        Debug.Log(message); // Apenas para debug, voc� pode substituir isso pela l�gica da UI
        TextWinLost.gameObject.SetActive(true);
        TextWinLost.text = message; // Certifique-se de ter uma refer�ncia ao componente de texto UI
                                    // Sai da sala atual
        StartCoroutine(ReturnToMainMenuAfterDelay());

    }
    [PunRPC]
    private void RemovePieceOnline(int pieceId, int playerActorNumber, int cellPieceId)
    {
        // Encontrar o jogador pelo ActorNumber
        Player playerOfPiece = FindPlayerByActorNumber(playerActorNumber);

        // Encontrar a pe�a pelo ViewID
        PieceScript pieceToRemove = FindPieceById(pieceId, playerOfPiece);
        CellScripts cell = FindCellById(cellPieceId);
        // Remover a pe�a se encontrada
        if (pieceToRemove != null)
        {
            // Primeiro, remova a pe�a da lista de pe�as
            playerOfPiece.PlayerPiecesInside.Remove(pieceToRemove);
            cell.currentPiece = null;
            cell.isOccupied = false;
            // Destrua a pe�a depois de remov�-la da lista
            Destroy(pieceToRemove.gameObject);

            // Opcional: Limpar refer�ncias nulas restantes na lista
            playerOfPiece.PlayerPiecesInside.RemoveAll(item => item == null);
        }

        
    }

    // Fun��o para encontrar a pe�a pelo ViewID
    public PieceScript FindPieceById(int pieceId, Player player)
    {
        foreach (PieceScript piece in player.PlayerPiecesInside)
        {
            if (piece.photonView.ViewID == pieceId)
            {
                return piece;
            }
        }
        return null;
    }

    // Fun��o para encontrar o jogador pelo ActorNumber
    public Player FindPlayerByActorNumber(int actorNumber)
    {
        foreach (Player player in GameManager.instance.playerList)
        {
            if (player.photonPlayer.ActorNumber == actorNumber)
            {
                return player;
            }
        }
        return null;
    }
    // Fun��o para encontrar o jogador pelo ActorNumber
    public CellScripts FindCellById(int piceId)
    {
        foreach (CellScripts cell in BoardManager.instance.allCells)
        {
            if (cell.photonView.ViewID == piceId)
            {
                return cell;
            }
        }
        return null;
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


        if(playerPlaying.PlayerPiecesInside.Count > 0) { 
            foreach (PieceScript piece in playerPlaying.PlayerPiecesInside)
            {
                if (piece) { 
                    if (!piece.isOutSide)
                    {
                        CellScripts currentCell = piece.GetComponent<PieceScript>().currentCell;

                        if (currentCell != null)
                        {
                            // Verificar dire��es para capturar
                            CheckAndAddPieceToCaptureList(currentCell, currentCell.cellAbove, currentCell.cellAbove?.cellAbove, isPlayerOne, piece.GetComponent<PieceScript>());
                            CheckAndAddPieceToCaptureList(currentCell, currentCell.cellBelow, currentCell.cellBelow?.cellBelow, isPlayerOne, piece.GetComponent<PieceScript>());
                            CheckAndAddPieceToCaptureList(currentCell, currentCell.cellLeft, currentCell.cellLeft?.cellLeft, isPlayerOne, piece.GetComponent<PieceScript>());
                            CheckAndAddPieceToCaptureList(currentCell, currentCell.cellRight, currentCell.cellRight?.cellRight, isPlayerOne, piece.GetComponent<PieceScript>());
                        }
                    }
                }

            }
        }

        // Chamar a fun��o que modifica a lista antes do retorno
        ModifyPieceListBasedOnCaptures(isPlayerOne);

        return ListOfpieceThatHasAnotherPieceToCapture.Count > 0;
    }

    // Fun��o recursiva para contar capturas a partir de uma c�lula espec�fica
    public int CountCaptures(CellScripts currentCell, bool isPlayerOne)
    {
        int captureCount = 0;

        // Verificar dire��es para capturar
        captureCount += CheckAndCapture(currentCell, currentCell.cellAbove, currentCell.cellAbove?.cellAbove, isPlayerOne);
        captureCount += CheckAndCapture(currentCell, currentCell.cellBelow, currentCell.cellBelow?.cellBelow, isPlayerOne);
        captureCount += CheckAndCapture(currentCell, currentCell.cellLeft, currentCell.cellLeft?.cellLeft, isPlayerOne);
        captureCount += CheckAndCapture(currentCell, currentCell.cellRight, currentCell.cellRight?.cellRight, isPlayerOne);

        return captureCount;
    }

    // Fun��o que verifica capturas e retorna o n�mero de capturas subsequentes
    private int CheckAndCapture(CellScripts currentCell, CellScripts nextCell, CellScripts afterNextCell, bool isPlayerOne)
    {
        if(nextCell != null) { 
            if(nextCell.currentPiece != null) { 
                if (nextCell != null && afterNextCell != null && nextCell.currentPiece.isPlayerOnePiece && !afterNextCell.isOccupied)
                {
                    // Simular a captura removendo a pe�a capturada e contar capturas subsequentes
                    PieceScript capturedPiece = nextCell.currentPiece;
                    nextCell.currentPiece = null; // Remover a pe�a capturada

                    // Contar capturas subsequentes
                    int furtherCaptures = 1 + CountCaptures(afterNextCell, isPlayerOne);

                    // Restaurar a pe�a capturada (para n�o modificar o estado do jogo)
                    nextCell.currentPiece = capturedPiece;

                    return furtherCaptures;
                }
            }
        }
        return 0;
    }

    // Fun��o que altera a lista de pe�as com base nas capturas recursivas
    public void ModifyPieceListBasedOnCaptures(bool isPlayerOne)
    {
        List<PieceScript> piecesWithMostCaptures = new List<PieceScript>();
        Dictionary<PieceScript, int> pieceWithMostCapturesAndTheValue = new Dictionary<PieceScript, int>();
        int maxCaptures = 0; // Vari�vel para armazenar o n�mero m�ximo de capturas

        // Itera pela lista de pe�as que podem capturar
        foreach (PieceScript piece in ListOfpieceThatHasAnotherPieceToCapture)
        {
            CellScripts currentCell = piece.GetComponent<PieceScript>().currentCell;

            if (currentCell != null)
            {
                // Conta capturas subsequentes
                int captureCount = CountCaptures(currentCell, isPlayerOne);

                // Se encontrarmos uma pe�a com mais capturas, atualizamos a lista
                if (captureCount >= maxCaptures)
                {
                    maxCaptures = captureCount;
                    piecesWithMostCaptures.Clear(); // Remove pe�as anteriores
                    piecesWithMostCaptures.Add(piece); // Adiciona a pe�a com o novo m�ximo
                    pieceWithMostCapturesAndTheValue.Add(piece, captureCount);

                    if (captureCount >= maxCaptures)
                    {
                        maxCaptures = captureCount;
                    }
                }
                // Se a pe�a tem o mesmo n�mero de capturas m�ximas, adiciona � lista
                else if (captureCount == maxCaptures)
                {
                    piecesWithMostCaptures.Add(piece);
                }
            }
        }

        // Atualiza a lista original com as pe�as que t�m o maior n�mero de capturas
        ListOfpieceThatHasAnotherPieceToCapture.Clear();

        // Adiciona todas as pe�as que t�m o n�mero m�ximo de capturas
        foreach (var entry in pieceWithMostCapturesAndTheValue)
        {
            if (entry.Value == maxCaptures)
            {
                ListOfpieceThatHasAnotherPieceToCapture.Add(entry.Key);
            }
        }
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
    // M�todo chamado automaticamente quando um jogador sai da sala
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log($"O outro jogador saiu da sala.");
        base.OnPlayerLeftRoom(otherPlayer);

        playerList.RemoveAll(player => player.photonPlayer == otherPlayer);
        // Aqui voc� pode notificar o outro jogador
        NotifyPlayerLeft(otherPlayer.NickName);
    }

    // M�todo para notificar o jogador restante
    void NotifyPlayerLeft(string playerName)
    {
        // Aqui pode ser implementada a l�gica para notifica��o visual ou sonora
        // Exemplo: Mostrar uma mensagem na tela
        Debug.Log($"O outro jogador saiu da sala.");

        
        GoToMainMenu();
        // Se voc� tem uma UI para notifica��o:
        // UIManager.Instance.ShowNotification($"{playerName} deixou o jogo.");
    }
    private void GoToMainMenu()
    {
        StartCoroutine(ReturnToMainMenuAfterDelay());
    }

    private IEnumerator ReturnToMainMenuAfterDelay()
    {
        yield return new WaitForSeconds(5); // Espera 5 segundos
        // Sai da sala atual
        PhotonNetwork.LeaveRoom();

        // Ap�s sair da sala, desconecta do servidor
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("LobbyScene"); // Carrega a cena do menu inicial
    }
}



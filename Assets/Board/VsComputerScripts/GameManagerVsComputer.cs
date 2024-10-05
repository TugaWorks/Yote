using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerVsComputer : MonoBehaviour
{
    public static GameManagerVsComputer instance;

    public List<PlayerVsComputer> playerList = new List<PlayerVsComputer>();
    public ComputerPlayer computerPlayer;

    public bool isPlayerOneTurn = true; // True se for a vez do jogador 1, False se for a vez do computador

    [SerializeField] TextMeshProUGUI textPlayerWin;
    [SerializeField] TextMeshProUGUI textPlayerLost;
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
    }

    void Start()
    {
        // Inicialização se necessário
    }

    void Update()
    {
        // Atualizações se necessário
    }

    public void EndTurn()
    {
        isPlayerOneTurn = !isPlayerOneTurn;
        if (!isPlayerOneTurn)
        {
            int HowManyPiecesComputerHasStucked = 0;
            foreach (PieceScriptVsComputer piece in ComputerPlayer.instance.computerPiecesInBoard)
            {

                if (CheckIfPieceIsStuck(piece))
                {
                    HowManyPiecesComputerHasStucked++;
                }
            }

            if (HowManyPiecesComputerHasStucked == ComputerPlayer.instance.computerPieces.Count)
            {
                Debug.Log("Player ganhou!");
            }
            else
            {
                HowManyPiecesComputerHasStucked = 0;
                computerPlayer.TakeTurn();
            }
            // Se for a vez do computador, chama o método TakeTurn do ComputerPlayer
            
        }
        else
        {
            int HowManyPiecesPlayerHasStucked = 0;
            foreach (PieceScriptVsComputer piece in playerList[0].PlayerPiecesInside)
            {
                
                if (CheckIfPieceIsStuck(piece))
                {
                    HowManyPiecesPlayerHasStucked++;
                }
            }

            if(HowManyPiecesPlayerHasStucked == playerList[0].PlayerPieces.Count)
            {
                Debug.Log("Computador ganhou!");
            }
            else
            {
                HowManyPiecesPlayerHasStucked = 0;
            }
        }
    }

    public bool IsPlayerTurn(bool isPlayerOnePiece)
    {
        // Verifica se é a vez do jogador 1
        return isPlayerOneTurn == isPlayerOnePiece;
    }

    public List<CellScriptsVSComputer> GetAllCells()
    {
       return BoardManagerVsComputer.instance.GetAllCells();
    }

    public void HighlightAllUnoccupiedCells()
    {
        foreach (var cell in BoardManagerVsComputer.instance.GetAllCells())
        {
            if (!cell.isOccupied)
            {
                cell.Highlight();
            }
        }
    }
    public bool CheckIfPieceIsStuck(PieceScriptVsComputer piece)
    {
        CellScriptsVSComputer currentCell = piece.currentCell;

        // Verificar se a célula atual da peça existe
        if (currentCell != null)
        {
            // Verificar célula à esquerda
            if (currentCell.cellLeft != null)
            {
                if (!currentCell.cellLeft.isOccupied)
                {
                    return false; // Há um espaço disponível à esquerda
                }
                if (currentCell.cellLeft.currentPiece) { 
                    if (currentCell.cellLeft.isOccupied && currentCell.cellLeft.currentPiece.isPlayerOnePiece != piece.isPlayerOnePiece)
                    {
                        // A célula à esquerda está ocupada por uma peça inimiga, verificar se pode capturar
                        if (currentCell.cellLeft.cellLeft != null && !currentCell.cellLeft.cellLeft.isOccupied)
                        {
                            return false; // Pode capturar a peça inimiga à esquerda
                        }
                    }
                }
            }

            // Verificar célula à direita
            if (currentCell.cellRight != null)
            {
                if (!currentCell.cellRight.isOccupied)
                {
                    return false; // Há um espaço disponível à direita
                }
                if (currentCell.cellRight.currentPiece)
                {
                 if (currentCell.cellRight.isOccupied && currentCell.cellRight.currentPiece.isPlayerOnePiece != piece.isPlayerOnePiece)
                    {
                        // A célula à direita está ocupada por uma peça inimiga, verificar se pode capturar
                        if (currentCell.cellRight.cellRight != null && !currentCell.cellRight.cellRight.isOccupied)
                        {
                            return false; // Pode capturar a peça inimiga à direita
                        }
                    }
                }
            }

            // Verificar célula acima
            if (currentCell.cellAbove != null)
            {
                if (!currentCell.cellAbove.isOccupied)
                {
                    return false; // Há um espaço disponível acima
                }
                if (currentCell.cellAbove.currentPiece)
                {
                 if (currentCell.cellAbove.isOccupied && currentCell.cellAbove.currentPiece.isPlayerOnePiece != piece.isPlayerOnePiece)
                {
                    // A célula acima está ocupada por uma peça inimiga, verificar se pode capturar
                    if (currentCell.cellAbove.cellAbove != null && !currentCell.cellAbove.cellAbove.isOccupied)
                    {
                        return false; // Pode capturar a peça inimiga acima
                    }
                }
                }
            }

            // Verificar célula abaixo
            if (currentCell.cellBelow != null)
            {
                if (!currentCell.cellBelow.isOccupied)
                {
                    return false; // Há um espaço disponível abaixo
                }
                if (currentCell.cellBelow.currentPiece)
                {
                 if (currentCell.cellBelow.isOccupied && currentCell.cellBelow.currentPiece.isPlayerOnePiece != piece.isPlayerOnePiece)
                {
                    // A célula abaixo está ocupada por uma peça inimiga, verificar se pode capturar
                    if (currentCell.cellBelow.cellBelow != null && !currentCell.cellBelow.cellBelow.isOccupied)
                    {
                        return false; // Pode capturar a peça inimiga abaixo
                    }
                }
                }
            }
        }

        // Se todas as verificações falharem, a peça está presa
        return true;
    }
    public void HighlightAdjacentCells(CellScriptsVSComputer cell, bool isPlayerOnePiece, PieceScriptVsComputer currentPiece)
    {
        ClearHighlightedCells();
        if (cell)
        {
            // Verifica se a peça está na lista de peças que podem capturar outra peça
            if (GameManagerVsComputer.instance.ListOfpieceThatHasAnotherPieceToCapture.Contains(currentPiece))
            {
                // Apenas destacar as células além de uma peça inimiga
                CheckAndHighlightJump(cell, cell.cellAbove, cell.cellAbove?.cellAbove, isPlayerOnePiece);
                CheckAndHighlightJump(cell, cell.cellBelow, cell.cellBelow?.cellBelow, isPlayerOnePiece);
                CheckAndHighlightJump(cell, cell.cellLeft, cell.cellLeft?.cellLeft, isPlayerOnePiece);
                CheckAndHighlightJump(cell, cell.cellRight, cell.cellRight?.cellRight, isPlayerOnePiece);
            }
            else
            {
                // Caso não esteja na lista, destaca as células adjacentes normais
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

    public void CheckAndHighlightJump(CellScriptsVSComputer currentCell, CellScriptsVSComputer adjacentCell, CellScriptsVSComputer jumpCell, bool isPlayerOnePiece)
    {
        if (adjacentCell != null && jumpCell != null && adjacentCell.isOccupied)
        {
            PieceScriptVsComputer piece = adjacentCell.currentPiece;
            if (piece != null && piece.isPlayerOnePiece != isPlayerOnePiece && !jumpCell.isOccupied)
            {
                HighlightCell(jumpCell);
            }
        }
    }
    public void ClearHighlightedCells()
    {
        foreach (var cell in BoardManagerVsComputer.instance.GetAllCells())
        {
            cell.ClearHighlight();
        }
    }

    public CellScriptsVSComputer GetMiddleCell(CellScriptsVSComputer startCell, CellScriptsVSComputer endCell)
    {
        int rowDiff = endCell.row - startCell.row;
        int colDiff = endCell.column - startCell.column;

        if (Mathf.Abs(rowDiff) == 2 && colDiff == 0)
        {
            return BoardManagerVsComputer.instance.GetCell(startCell.row + rowDiff / 2, startCell.column);
        }
        if (rowDiff == 0 && Mathf.Abs(colDiff) == 2)
        {
            return BoardManagerVsComputer.instance.GetCell(startCell.row, startCell.column + colDiff / 2);
        }
        return null;
    }

    public void RemovePiece(PieceScriptVsComputer piece)
    {
        piece.GetComponent<PieceScriptVsComputer>().currentCell.isOccupied = false;
        piece.GetComponent<PieceScriptVsComputer>().currentCell.currentPiece = null;
        piece.GetComponent<PieceScriptVsComputer>().currentCell = null;
        Destroy(piece.gameObject);
        ComputerPlayer.instance.computerPieces.Remove(piece);

        ComputerPlayer.instance.textCountComputerPieces.text = ComputerPlayer.instance.computerPieces.Count.ToString();
        GameManagerVsComputer.instance.CheckIfComputerLost();
    }

    public string GetCaptureDirection(CellScriptsVSComputer startCell, CellScriptsVSComputer endCell)
    {
        int rowDiff = endCell.row - startCell.row;
        int colDiff = endCell.column - startCell.column;

        if (rowDiff == 2 && colDiff == 0) return "Above";
        if (rowDiff == -2 && colDiff == 0) return "Below";
        if (rowDiff == 0 && colDiff == 2) return "Right";
        if (rowDiff == 0 && colDiff == -2) return "Left";
        return "";
    }

    public void HighlightCell(CellScriptsVSComputer cell)
    {
        cell.Highlight();
    }

    public CellScriptsVSComputer FindAvailableCell()
    {
        List<CellScriptsVSComputer> availableCells = new List<CellScriptsVSComputer>();
        foreach (var cell in BoardManagerVsComputer.instance.GetAllCells())
        {
            if (!cell.isOccupied)
            {
                availableCells.Add(cell);
            }
        }

        if (availableCells.Count > 0)
        {
            int randomIndex = Random.Range(0, availableCells.Count);
            return availableCells[randomIndex];
        }
        return null;
    }
    public List<PieceScriptVsComputer> ListOfpieceThatHasAnotherPieceToCapture = new List<PieceScriptVsComputer>();
    public bool VerifyIfHasNoPieceToCapture(bool isPlayerOne)
    {
        // Limpa a lista antes de verificar
        ListOfpieceThatHasAnotherPieceToCapture.Clear();

        // Verifica todas as peças do jogador
        foreach (PieceScriptVsComputer piece in playerList[0].PlayerPieces)
        {
            if (piece)
            {
                if (!piece.isOutSide)
                {
                    CellScriptsVSComputer currentCell = piece.GetComponent<PieceScriptVsComputer>().currentCell;

                    if (currentCell != null)
                    {
                        // Verificar direções para capturar
                        CheckAndAddPieceToCaptureList(currentCell, currentCell.cellAbove, currentCell.cellAbove?.cellAbove, isPlayerOne, piece.GetComponent<PieceScriptVsComputer>());
                        CheckAndAddPieceToCaptureList(currentCell, currentCell.cellBelow, currentCell.cellBelow?.cellBelow, isPlayerOne, piece.GetComponent<PieceScriptVsComputer>());
                        CheckAndAddPieceToCaptureList(currentCell, currentCell.cellLeft, currentCell.cellLeft?.cellLeft, isPlayerOne, piece.GetComponent<PieceScriptVsComputer>());
                        CheckAndAddPieceToCaptureList(currentCell, currentCell.cellRight, currentCell.cellRight?.cellRight, isPlayerOne, piece.GetComponent<PieceScriptVsComputer>());
                    }
                }
            }

        }
        // Chamar a função que modifica a lista antes do retorno
        ModifyPieceListBasedOnCaptures(isPlayerOne);

        return ListOfpieceThatHasAnotherPieceToCapture.Count > 0;
    }


    // Função recursiva para contar capturas a partir de uma célula específica
    public int CountCaptures(CellScriptsVSComputer currentCell, bool isPlayerOne)
    {
        int captureCount = 0;

        // Verificar direções para capturar
        captureCount += CheckAndCapture(currentCell, currentCell.cellAbove, currentCell.cellAbove?.cellAbove, isPlayerOne);
        captureCount += CheckAndCapture(currentCell, currentCell.cellBelow, currentCell.cellBelow?.cellBelow, isPlayerOne);
        captureCount += CheckAndCapture(currentCell, currentCell.cellLeft, currentCell.cellLeft?.cellLeft, isPlayerOne);
        captureCount += CheckAndCapture(currentCell, currentCell.cellRight, currentCell.cellRight?.cellRight, isPlayerOne);

        return captureCount;
    }

    // Função que verifica capturas e retorna o número de capturas subsequentes
    private int CheckAndCapture(CellScriptsVSComputer currentCell, CellScriptsVSComputer nextCell, CellScriptsVSComputer afterNextCell, bool isPlayerOne)
    {
        if (nextCell != null)
        {
            if (nextCell.currentPiece != null)
            {
                if (nextCell != null && afterNextCell != null && nextCell.currentPiece.isPlayerOnePiece && !afterNextCell.isOccupied)
                {
                    // Simular a captura removendo a peça capturada e contar capturas subsequentes
                    PieceScriptVsComputer capturedPiece = nextCell.currentPiece;
                    nextCell.currentPiece = null; // Remover a peça capturada

                    // Contar capturas subsequentes
                    int furtherCaptures = 1 + CountCaptures(afterNextCell, isPlayerOne);

                    // Restaurar a peça capturada (para não modificar o estado do jogo)
                    nextCell.currentPiece = capturedPiece;

                    return furtherCaptures;
                }
            }
        }
        return 0;
    }

    // Função que altera a lista de peças com base nas capturas recursivas
    public void ModifyPieceListBasedOnCaptures(bool isPlayerOne)
    {
        List<PieceScriptVsComputer> piecesWithMostCaptures = new List<PieceScriptVsComputer>();
        Dictionary<PieceScriptVsComputer, int> pieceWithMostCapturesAndTheValue = new Dictionary<PieceScriptVsComputer, int>();
        int maxCaptures = 0; // Variável para armazenar o número máximo de capturas

        // Itera pela lista de peças que podem capturar
        foreach (PieceScriptVsComputer piece in ListOfpieceThatHasAnotherPieceToCapture)
        {
            CellScriptsVSComputer currentCell = piece.GetComponent<PieceScriptVsComputer>().currentCell;

            if (currentCell != null)
            {
                // Conta capturas subsequentes
                int captureCount = CountCaptures(currentCell, isPlayerOne);

                // Se encontrarmos uma peça com mais capturas, atualizamos a lista
                if (captureCount >= maxCaptures)
                {
                    maxCaptures = captureCount;
                    piecesWithMostCaptures.Clear(); // Remove peças anteriores
                    piecesWithMostCaptures.Add(piece); // Adiciona a peça com o novo máximo
                    pieceWithMostCapturesAndTheValue.Add(piece, captureCount);

                    if (captureCount >= maxCaptures)
                    {
                        maxCaptures = captureCount;
                    }
                }
                // Se a peça tem o mesmo número de capturas máximas, adiciona à lista
                else if (captureCount == maxCaptures)
                {
                    piecesWithMostCaptures.Add(piece);
                }
            }
        }

        // Atualiza a lista original com as peças que têm o maior número de capturas
        ListOfpieceThatHasAnotherPieceToCapture.Clear();

        // Adiciona todas as peças que têm o número máximo de capturas
        foreach (var entry in pieceWithMostCapturesAndTheValue)
        {
            if (entry.Value == maxCaptures)
            {
                ListOfpieceThatHasAnotherPieceToCapture.Add(entry.Key);
            }
        }
    }
    private void CheckAndAddPieceToCaptureList(CellScriptsVSComputer currentCell, CellScriptsVSComputer adjacentCell, CellScriptsVSComputer cellBeyond, bool isPlayerOne, PieceScriptVsComputer piece)
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

    public void CheckIfPlayerLost()
    {
        if (GameManagerVsComputer.instance.playerList[0].PlayerPieces.Count <= 0)
        {
            textPlayerLost.gameObject.SetActive(true);
            Debug.Log("Player lost");
            StartCoroutine(ReturnToMainMenuAfterDelay());
        }
    }

    public void CheckIfComputerLost()
    {
        if (ComputerPlayer.instance.computerPieces.Count <= 0)
        {
            
            textPlayerWin.gameObject.SetActive(true);
            Debug.Log("Computer lost");
            StartCoroutine(ReturnToMainMenuAfterDelay());
        }
    }

    private IEnumerator ReturnToMainMenuAfterDelay()
    {
        yield return new WaitForSeconds(5); // Espera 5 segundos
        SceneManager.LoadScene("LobbyScene"); // Carrega a cena do menu inicial
    }
}

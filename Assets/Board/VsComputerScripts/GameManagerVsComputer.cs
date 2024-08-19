using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class GameManagerVsComputer : MonoBehaviour
{
    public static GameManagerVsComputer instance;

    public List<PlayerVsComputer> playerList = new List<PlayerVsComputer>();
    public ComputerPlayer computerPlayer;

    public bool isPlayerOneTurn = true; // True se for a vez do jogador 1, False se for a vez do computador

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
            // Se for a vez do computador, chama o método TakeTurn do ComputerPlayer
            computerPlayer.TakeTurn();
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

    public void HighlightAdjacentCells(CellScriptsVSComputer cell, bool isPlayerOnePiece)
    {
        ClearHighlightedCells();
        if (cell != null)
        {
            if (cell.cellAbove != null && !cell.cellAbove.isOccupied) cell.cellAbove.Highlight();
            if (cell.cellBelow != null && !cell.cellBelow.isOccupied) cell.cellBelow.Highlight();
            if (cell.cellLeft != null && !cell.cellLeft.isOccupied) cell.cellLeft.Highlight();
            if (cell.cellRight != null && !cell.cellRight.isOccupied) cell.cellRight.Highlight();

            CheckAndHighlightJump(cell, cell.cellAbove, cell.cellAbove?.cellAbove, isPlayerOnePiece);
            CheckAndHighlightJump(cell, cell.cellBelow, cell.cellBelow?.cellBelow, isPlayerOnePiece);
            CheckAndHighlightJump(cell, cell.cellLeft, cell.cellLeft?.cellLeft, isPlayerOnePiece);
            CheckAndHighlightJump(cell, cell.cellRight, cell.cellRight?.cellRight, isPlayerOnePiece);
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
        ListOfpieceThatHasAnotherPieceToCapture.Clear();

        foreach (PieceScriptVsComputer piece in playerList[0].PlayerPiecesInside)
        {
            CellScriptsVSComputer currentCell = piece.currentCell;

            if (currentCell != null)
            {
                // Verificar direções para capturar
                CheckAndAddPieceToCaptureList(currentCell, currentCell.cellAbove, currentCell.cellAbove?.cellAbove, isPlayerOne, piece);
                CheckAndAddPieceToCaptureList(currentCell, currentCell.cellBelow, currentCell.cellBelow?.cellBelow, isPlayerOne, piece);
                CheckAndAddPieceToCaptureList(currentCell, currentCell.cellLeft, currentCell.cellLeft?.cellLeft, isPlayerOne, piece);
                CheckAndAddPieceToCaptureList(currentCell, currentCell.cellRight, currentCell.cellRight?.cellRight, isPlayerOne, piece);
            }
        }

        return ListOfpieceThatHasAnotherPieceToCapture.Count > 0;
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


}

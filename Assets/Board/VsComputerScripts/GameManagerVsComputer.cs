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
        // Inicializa��o se necess�rio
    }

    void Update()
    {
        // Atualiza��es se necess�rio
    }

    public void EndTurn()
    {
        isPlayerOneTurn = !isPlayerOneTurn;
        if (!isPlayerOneTurn)
        {
            // Se for a vez do computador, chama o m�todo TakeTurn do ComputerPlayer
            computerPlayer.TakeTurn();
        }
    }

    public bool IsPlayerTurn(bool isPlayerOnePiece)
    {
        // Verifica se � a vez do jogador 1
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
                if (currentCell.cellLeft.currentPiece) { 
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
    public void HighlightAdjacentCells(CellScriptsVSComputer cell, bool isPlayerOnePiece, PieceScriptVsComputer currentPiece)
    {
        ClearHighlightedCells();
        if (cell)
        {
            // Verifica se a pe�a est� na lista de pe�as que podem capturar outra pe�a
            if (GameManagerVsComputer.instance.ListOfpieceThatHasAnotherPieceToCapture.Contains(currentPiece))
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
        for(int i = 0; i < ListOfpieceThatHasAnotherPieceToCapture.Count; i++)
        {
            ListOfpieceThatHasAnotherPieceToCapture.Remove(ListOfpieceThatHasAnotherPieceToCapture[i]);
        }

        foreach (PieceScriptVsComputer piece in playerList[0].PlayerPiecesInside)
        {
            CellScriptsVSComputer currentCell = piece.currentCell;
            if (currentCell)
            {

           
                if(currentCell.cellAbove != null && currentCell.cellAbove.currentPiece != null && currentCell.cellAbove.currentPiece.isPlayerOnePiece != piece.isPlayerOnePiece && currentCell.cellAbove.isOccupied && currentCell.cellAbove.cellAbove != null && !currentCell.cellAbove.cellAbove.isOccupied &&  !ListOfpieceThatHasAnotherPieceToCapture.Contains(piece))
                {
                    ListOfpieceThatHasAnotherPieceToCapture.Add(piece);
                }
                if (currentCell.cellBelow != null && currentCell.cellBelow.currentPiece != null && currentCell.cellBelow.currentPiece.isPlayerOnePiece != piece.isPlayerOnePiece && currentCell.cellBelow.isOccupied && currentCell.cellBelow.cellBelow != null && !currentCell.cellBelow.cellBelow.isOccupied && !ListOfpieceThatHasAnotherPieceToCapture.Contains(piece))
                {
                    ListOfpieceThatHasAnotherPieceToCapture.Add(piece);
                }
                if (currentCell.cellLeft != null && currentCell.cellLeft.currentPiece != null && currentCell.cellLeft.currentPiece.isPlayerOnePiece != piece.isPlayerOnePiece && currentCell.cellLeft.isOccupied &&  currentCell.cellLeft.cellLeft != null && !currentCell.cellLeft.cellLeft.isOccupied && !ListOfpieceThatHasAnotherPieceToCapture.Contains(piece))
                {
                    ListOfpieceThatHasAnotherPieceToCapture.Add(piece);
                }
                if (currentCell.cellRight != null && currentCell.cellRight.currentPiece != null && currentCell.cellRight.currentPiece.isPlayerOnePiece != piece.isPlayerOnePiece && currentCell.cellRight.isOccupied && currentCell.cellRight.cellRight != null && !currentCell.cellRight.cellRight.isOccupied && !ListOfpieceThatHasAnotherPieceToCapture.Contains(piece) )
                {
                    ListOfpieceThatHasAnotherPieceToCapture.Add(piece);
                }
            }
            //if (currentCell != null)
            //{
            //    switch (piece.lastCaptureDirection)
            //    {
            //        // Verificar dire��es para capturar
            //        case "Above": CheckAndAddPieceToCaptureList(currentCell, currentCell.cellAbove, currentCell.cellAbove?.cellAbove, isPlayerOne, piece); break;
            //        case "Below": CheckAndAddPieceToCaptureList(currentCell, currentCell.cellBelow, currentCell.cellBelow?.cellBelow, isPlayerOne, piece); break;
            //        case "Left": CheckAndAddPieceToCaptureList(currentCell, currentCell.cellLeft, currentCell.cellLeft?.cellLeft, isPlayerOne, piece); break;
            //        case "Right": CheckAndAddPieceToCaptureList(currentCell, currentCell.cellRight, currentCell.cellRight?.cellRight, isPlayerOne, piece); break;
            //    }
            //}
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

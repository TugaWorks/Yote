using UnityEngine;
using System.Collections;
using System.Collections.Generic;

enum DirectionOfMovement
{
    Right,
    Left,
    Above,
    Below,
    none
}

public class ComputerPlayer : MonoBehaviour
{
    public List<PieceScriptVsComputer> computerPieces = new List<PieceScriptVsComputer>();
    public List<PieceScriptVsComputer> computerPiecesOutBoard = new List<PieceScriptVsComputer>();
    public List<PieceScriptVsComputer> computerPiecesInBoard = new List<PieceScriptVsComputer>();
    public float delayBetweenMoves = 2f;
    private string lastCaptureDirection = "";

    private DirectionOfMovement dirMovement;
    void Start()
    {
        // Inicialização se necessário
        computerPiecesOutBoard = new List<PieceScriptVsComputer>(computerPieces);
    }

    public void TakeTurn()
    {
        StartCoroutine(ExecuteTurn());
    }

    private IEnumerator ExecuteTurn()
    {
        // Esperar um momento para simular o pensamento do computador
        yield return new WaitForSeconds(delayBetweenMoves);

        if (computerPiecesOutBoard.Count > 0)
        {
            if (computerPiecesInBoard.Count > 0)
            {
                // Decidir se vai jogar uma nova peça ou mover uma peça existente
                bool playNewPiece = DecideIfPlaceNewPiece();

                if (playNewPiece && computerPiecesOutBoard.Count > 0)
                {
                    yield return StartCoroutine(PlaceNewPiece());
                }
                else
                {
                    // Esperar um momento para simular o pensamento do computador
                    yield return new WaitForSeconds(delayBetweenMoves);
                    MoveExistingPiece();
                }
            }
            else
            {
                yield return StartCoroutine(PlaceNewPiece());
            }
        }
        else
        {
            MoveExistingPiece();
        }

        // Esperar um momento após a jogada do computador
        yield return new WaitForSeconds(delayBetweenMoves);

        // Finalizar turno do computador
        GameManagerVsComputer.instance.EndTurn();
    }

    private bool DecideIfPlaceNewPiece()
    {
        // Verificar se há alguma peça em board que pode capturar uma peça inimiga adjacente
        foreach (var piece in computerPiecesInBoard)
        {
            if (CanCapture(piece))
            {
                // Se houver uma peça que pode capturar, não colocar uma nova peça
                return false;
            }
        }

        // Se nenhuma peça puder capturar, decidir aleatoriamente
        return Random.Range(0, 2) == 0;
    }

    private CellScriptsVSComputer CanCapture(PieceScriptVsComputer piece)
    {
        CellScriptsVSComputer currentCell = piece.currentCell;

        if (currentCell.cellLeft != null && currentCell.cellLeft.isOccupied && currentCell.cellLeft.currentPiece.isPlayerOnePiece != piece.isPlayerOnePiece && currentCell.cellLeft.cellLeft != null && !currentCell.cellLeft.cellLeft.isOccupied)
        {
            dirMovement = DirectionOfMovement.Left;
            return currentCell.cellLeft;
        }
        if (currentCell.cellRight != null && currentCell.cellRight.isOccupied && currentCell.cellRight.currentPiece.isPlayerOnePiece != piece.isPlayerOnePiece && currentCell.cellRight.cellRight != null && !currentCell.cellRight.cellRight.isOccupied)
        {
            dirMovement = DirectionOfMovement.Right;
            return currentCell.cellRight;
        }
        if (currentCell.cellAbove != null && currentCell.cellAbove.isOccupied && currentCell.cellAbove.currentPiece.isPlayerOnePiece != piece.isPlayerOnePiece && currentCell.cellAbove.cellAbove != null && !currentCell.cellAbove.cellAbove.isOccupied)
        {
            dirMovement = DirectionOfMovement.Above;
            return currentCell.cellAbove;
        }
        if (currentCell.cellBelow != null && currentCell.cellBelow.isOccupied && currentCell.cellBelow.currentPiece.isPlayerOnePiece != piece.isPlayerOnePiece && currentCell.cellBelow.cellBelow != null && !currentCell.cellBelow.cellBelow.isOccupied)
        {
            dirMovement = DirectionOfMovement.Below;
            return currentCell.cellBelow;
        }

        return null;
    }

    private IEnumerator PlaceNewPiece()
    {
        // Escolher uma peça disponível aleatoriamente
        PieceScriptVsComputer pieceToPlace = computerPiecesOutBoard[Random.Range(0, computerPiecesOutBoard.Count)];

        // Colocar a peça em uma célula disponível
        CellScriptsVSComputer targetCell = GameManagerVsComputer.instance.FindAvailableCell();
        if (targetCell != null)
        {
            if (pieceToPlace.currentCell)
            {
                pieceToPlace.currentCell.isOccupied = false;
                pieceToPlace.currentCell.currentPiece = null;
            }
            yield return StartCoroutine(MovePieceToCell(pieceToPlace, targetCell));
        }
    }
    private IEnumerator WaitFirstCapture(PieceScriptVsComputer piece)
    {
       
            yield return new WaitForSeconds(1f);
        CheckForChainCapture(piece);

    }
    private IEnumerator MovePieceToCell(PieceScriptVsComputer piece, CellScriptsVSComputer targetCell)
    {
        float duration = 1.0f; // duração da animação
        Vector3 startPosition = piece.transform.position;
        Vector3 endPosition = targetCell.transform.position;
        float peakHeight = 1.0f; // altura do ponto mais alto do movimento

        float elapsed = 0;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            // Lerp entre start e end position
            Vector3 horizontalPosition = Vector3.Lerp(startPosition, endPosition, t);
            // Adicionar a curva para o movimento vertical
            float height = Mathf.Sin(Mathf.PI * t) * peakHeight;
            piece.transform.position = new Vector3(horizontalPosition.x, horizontalPosition.y + height, horizontalPosition.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        piece.transform.position = endPosition;

        piece.currentCell = targetCell;
        piece.isOutSide = false;
        targetCell.isOccupied = true;
        targetCell.currentPiece = piece;

        computerPiecesOutBoard.Remove(piece);
        computerPiecesInBoard.Add(piece);

        // Verificar se há captura em cadeia
    }

    private void MoveExistingPiece()
    {
        // Escolher uma peça que já está no tabuleiro
        PieceScriptVsComputer pieceToMove = null;
        foreach (var piece in computerPiecesInBoard)
        {
            if (!piece.isOutSide)
            {
                pieceToMove = piece;

                if (pieceToMove.currentCell) pieceToMove.lastCell = pieceToMove.currentCell;
                if (pieceToMove.lastCell) pieceToMove.lastCell.isOccupied = false;
                break;
            }
        }

        // Verificar se temos uma peça para mover
        if (pieceToMove != null)
        {
            CellScriptsVSComputer cellWithPieceToCapture = FindDirectionOfCapture(pieceToMove.currentCell);

            // Verificar se a célula de captura está ocupada
            if (cellWithPieceToCapture != null && cellWithPieceToCapture.isOccupied && cellWithPieceToCapture.currentPiece.isPlayerOnePiece != pieceToMove.isPlayerOnePiece)
            {
                dirMovement = GetDirection(pieceToMove, cellWithPieceToCapture);
                if (dirMovement == DirectionOfMovement.Left)
                {
                    pieceToMove.currentCell.isOccupied = false;
                    cellWithPieceToCapture.isOccupied = false;
                    StartCoroutine(MovePieceToCell(pieceToMove, cellWithPieceToCapture.cellLeft));

                    Destroy(cellWithPieceToCapture.currentPiece.gameObject);
                }
                if (dirMovement == DirectionOfMovement.Right)
                {
                    pieceToMove.currentCell.isOccupied = false;
                    cellWithPieceToCapture.isOccupied = false;
                    StartCoroutine(MovePieceToCell(pieceToMove, cellWithPieceToCapture.cellRight));

                    Destroy(cellWithPieceToCapture.currentPiece.gameObject);
                }
                if (dirMovement == DirectionOfMovement.Above)
                {
                    pieceToMove.currentCell.isOccupied = false;
                    cellWithPieceToCapture.isOccupied = false;
                    StartCoroutine(MovePieceToCell(pieceToMove, cellWithPieceToCapture.cellAbove));

                    Destroy(cellWithPieceToCapture.currentPiece.gameObject);
                }
                if (dirMovement == DirectionOfMovement.Below)
                {
                    pieceToMove.currentCell.isOccupied = false;
                    cellWithPieceToCapture.isOccupied = false;
                    StartCoroutine(MovePieceToCell(pieceToMove, cellWithPieceToCapture.cellBelow));

                    Destroy(cellWithPieceToCapture.currentPiece.gameObject);
                }

                StartCoroutine(WaitFirstCapture(pieceToMove));
                
            }
            else
            {
                // Apenas mover para uma célula adjacente disponível
                CellScriptsVSComputer targetCell = FindAvailableAdjacentCell(pieceToMove.currentCell);
                if (targetCell != null)
                {
                    StartCoroutine(MovePieceToCell(pieceToMove, targetCell));
                }
            }
        }
    }

    private DirectionOfMovement GetDirection(PieceScriptVsComputer currentPieceToMove, CellScriptsVSComputer cellWithPieceToCapture)
    {
        if (cellWithPieceToCapture == currentPieceToMove.currentCell.cellAbove) return DirectionOfMovement.Above;
        if (cellWithPieceToCapture == currentPieceToMove.currentCell.cellLeft) return DirectionOfMovement.Left;
        if (cellWithPieceToCapture == currentPieceToMove.currentCell.cellRight) return DirectionOfMovement.Right;
        if (cellWithPieceToCapture == currentPieceToMove.currentCell.cellBelow) return DirectionOfMovement.Below;

        return DirectionOfMovement.none;
    }

    private CellScriptsVSComputer FindDirectionOfCapture(CellScriptsVSComputer currentCellOfPieceChoosed)
    {
        List<CellScriptsVSComputer> adjacentCellsToTheCurrentCell = new List<CellScriptsVSComputer>();

        if (currentCellOfPieceChoosed.cellLeft && currentCellOfPieceChoosed.cellLeft.isOccupied != false)
        {
            adjacentCellsToTheCurrentCell.Add(currentCellOfPieceChoosed.cellLeft);
        }
        if (currentCellOfPieceChoosed.cellBelow && currentCellOfPieceChoosed.cellBelow.isOccupied != false)
        {
            adjacentCellsToTheCurrentCell.Add(currentCellOfPieceChoosed.cellBelow);
        }
        if (currentCellOfPieceChoosed.cellAbove && currentCellOfPieceChoosed.cellAbove.isOccupied != false)
        {
            adjacentCellsToTheCurrentCell.Add(currentCellOfPieceChoosed.cellAbove);
        }
        if (currentCellOfPieceChoosed.cellRight && currentCellOfPieceChoosed.cellRight.isOccupied != false)
        {
            adjacentCellsToTheCurrentCell.Add(currentCellOfPieceChoosed.cellRight);
        }

        if (adjacentCellsToTheCurrentCell.Count > 0)
        {
            int chooseCapture = Random.Range(0, adjacentCellsToTheCurrentCell.Count);
            return adjacentCellsToTheCurrentCell[chooseCapture];
        }

        return null;
    }

    private CellScriptsVSComputer FindAvailableAdjacentCell(CellScriptsVSComputer currentCell)
    {
        // Encontrar uma célula adjacente não ocupada para mover a peça
        if (currentCell.cellAbove != null && !currentCell.cellAbove.isOccupied) return currentCell.cellAbove;
        if (currentCell.cellBelow != null && !currentCell.cellBelow.isOccupied) return currentCell.cellBelow;
        if (currentCell.cellLeft != null && !currentCell.cellLeft.isOccupied) return currentCell.cellLeft;
        if (currentCell.cellRight != null && !currentCell.cellRight.isOccupied) return currentCell.cellRight;
        return null;
    }

    private void CheckForChainCapture(PieceScriptVsComputer piece)
    {

        CellScriptsVSComputer cellWithPieceToCapture = CanCapture(piece); // aqui se retornar a cell ja deve ficar correto
        int countCaptures = 0;

        if (cellWithPieceToCapture && countCaptures < 1)
        {
            PieceScriptVsComputer adjacentPiece = cellWithPieceToCapture.currentPiece;
            if (adjacentPiece != null && adjacentPiece.isPlayerOnePiece != piece.isPlayerOnePiece)
            {
                
                if (cellWithPieceToCapture != null && cellWithPieceToCapture.isOccupied)
                {
                    piece.currentCell.isOccupied = false;
                    switch (dirMovement)
                    {
                        
                        case DirectionOfMovement.Right: StartCoroutine(MovePieceToCell(piece, cellWithPieceToCapture.cellRight));  break; 
                        case DirectionOfMovement.Left: StartCoroutine(MovePieceToCell(piece, cellWithPieceToCapture.cellLeft));  break; 
                        case DirectionOfMovement.Above: StartCoroutine(MovePieceToCell(piece, cellWithPieceToCapture.cellAbove));  break; 
                        case DirectionOfMovement.Below: StartCoroutine(MovePieceToCell(piece, cellWithPieceToCapture.cellBelow));  break;
                            
                    }

                    adjacentPiece.currentCell.isOccupied = false;
                    Destroy(adjacentPiece.gameObject);
                    countCaptures++;
                }
            }
        }

        countCaptures = 0;
    }    
    private CellScriptsVSComputer GetAdjacentCell(CellScriptsVSComputer cell, DirectionOfMovement direction)
    {
        switch (direction)
        {
            case DirectionOfMovement.Above:
                return cell.cellAbove;
            case DirectionOfMovement.Below:
                return cell.cellBelow;
            case DirectionOfMovement.Left:
                return cell.cellLeft;
            case DirectionOfMovement.Right:
                return cell.cellRight;
            default:
                return null;
        }
    }
    private bool CheckCapture(CellScriptsVSComputer cell, string direction, ref int countCaptures, PieceScriptVsComputer pieceMoving)
    {
        if (cell != null)
        {
            CellScriptsVSComputer nextCell = GetNextCell(cell, direction);
            if (nextCell != null)
            {
                PieceScriptVsComputer piece = cell.currentPiece;
                if (piece != null && countCaptures < 1)
                {
                    if (!nextCell.isOccupied && cell.isOccupied && piece.isPlayerOnePiece != pieceMoving.isPlayerOnePiece)
                    {
                        countCaptures++;
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private CellScriptsVSComputer GetNextCell(CellScriptsVSComputer cell, string direction)
    {
        switch (direction)
        {
            case "Right":
                return cell.cellRight;
            case "Above":
                return cell.cellAbove;
            case "Left":
                return cell.cellLeft;
            case "Below":
                return cell.cellBelow;
            default:
                return null;
        }
    }
}

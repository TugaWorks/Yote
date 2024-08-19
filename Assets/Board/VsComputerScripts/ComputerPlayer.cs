using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

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
    public PieceScriptVsComputer currentPieceToMove = null;
    public float delayBetweenMoves = 2f;
    private string lastCaptureDirection = "";

    private DirectionOfMovement dirMovement;
    public static ComputerPlayer instance;

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
                bool canCapturePiece = DecideIfCanCapturePiece();
                if (!canCapturePiece)
                {
                    bool playPiece = UnityEngine.Random.Range(0, 2) == 0;
                    if (playPiece)
                    {
                        if (!canCapturePiece && computerPiecesOutBoard.Count > 0)
                        {
                            yield return StartCoroutine(PlaceNewPiece());
                        }
                    }
                    else
                    {
                        // Esperar um momento para simular o pensamento do computador
                        yield return new WaitForSeconds(delayBetweenMoves);
                        MoveExistingPiece(currentPieceToMove);

                    }
                }
                else
                {
                    // Esperar um momento para simular o pensamento do computador
                    yield return new WaitForSeconds(delayBetweenMoves);
                    MoveExistingPiece(currentPieceToMove);
                }
                
               
            }
            else
            {
                yield return StartCoroutine(PlaceNewPiece());
            }
        }
        else
        {
            if (currentPieceToMove) { 
                MoveExistingPiece(currentPieceToMove);
            }
            else
            {
                MoveExistingPiece(ChoosePiecetoMove());
            }
        }

        // Esperar um momento após a jogada do computador
        yield return new WaitForSeconds(delayBetweenMoves);

        // Finalizar turno do computador
        GameManagerVsComputer.instance.EndTurn();
    }


    private PieceScriptVsComputer ChoosePiecetoMove()
    {
        // Verificar se há alguma peça em board que pode capturar uma peça inimiga adjacente
        foreach (var piece in computerPiecesInBoard)
        {
            if (piece != null)
            {
                if (!CanCapture(piece))
                {
                    return piece;
                    
                }
            }
        }

        return null;
    }
    private bool DecideIfCanCapturePiece()
    {
        // Verificar se há alguma peça em board que pode capturar uma peça inimiga adjacente
        foreach (var piece in computerPiecesInBoard)
        {
            if (piece != null)
            {
                if (CanCapture(piece))
                {
                    // Se houver uma peça que pode capturar, não colocar uma nova peça
                    currentPieceToMove = piece;
                    return true;
                }
            }
        }

        // Se nenhuma peça puder capturar, decidir aleatoriamente
        return false;
    }

    private CellScriptsVSComputer CanCapture(PieceScriptVsComputer piece)
    {
        try
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
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }
        return null;
    }

    private IEnumerator PlaceNewPiece()
    {
        // Escolher uma peça disponível aleatoriamente
        PieceScriptVsComputer pieceToPlace = computerPiecesOutBoard[UnityEngine.Random.Range(0, computerPiecesOutBoard.Count)];

        // Colocar a peça em uma célula disponível
        CellScriptsVSComputer targetCell = GameManagerVsComputer.instance.FindAvailableCell();
        if (targetCell != null)
        {
            if (pieceToPlace.currentCell)
            {
                pieceToPlace.currentCell.isOccupied = false;
                pieceToPlace.currentCell.currentPiece = null;
            }
            computerPiecesInBoard.Add(pieceToPlace);
            yield return StartCoroutine(MovePieceToCell(pieceToPlace, targetCell));
        }
    }
    private IEnumerator WaitFirstCapture(PieceScriptVsComputer piece)
    {

        yield return new WaitForSeconds(1f);
        StartCoroutine(CheckForChainCapture(piece, 1));

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


        if (ListHavePiece(computerPiecesOutBoard, piece))
        {
            computerPiecesOutBoard.Remove(piece);
        }
        
        

        // Verificar se há captura em cadeia
    }
    private bool ListHavePiece(List<PieceScriptVsComputer> listToSearch, PieceScriptVsComputer pieceToFind)
    {
        foreach(PieceScriptVsComputer piece in listToSearch)
        {
            if(piece == pieceToFind)
            {
                return true;
            }
        }

        return false;
    }
    private void MoveExistingPiece(PieceScriptVsComputer pieceToMove)
    {
        // Verificar se temos uma peça para mover
        if (pieceToMove != null)
        {
            CellScriptsVSComputer cellWithPieceToCapture = FindDirectionOfCapture(pieceToMove.currentCell);

            // Verificar se a célula de captura está ocupada
            if (cellWithPieceToCapture != null && cellWithPieceToCapture.isOccupied && cellWithPieceToCapture.currentPiece.isPlayerOnePiece != pieceToMove.isPlayerOnePiece)
            {
                bool CapturedAPiece = false;
                dirMovement = GetDirection(pieceToMove, cellWithPieceToCapture);
                if (dirMovement == DirectionOfMovement.Left && cellWithPieceToCapture.cellLeft != null)
                {
                    pieceToMove.currentCell.isOccupied = false;
                    cellWithPieceToCapture.isOccupied = false;
                    StartCoroutine(MovePieceToCell(pieceToMove, cellWithPieceToCapture.cellLeft));
                    GameManagerVsComputer.instance.playerList[0].PlayerPiecesInside.Remove(cellWithPieceToCapture.currentPiece);
                    Destroy(cellWithPieceToCapture.currentPiece.gameObject);
                    CapturedAPiece = true;
                }
                if (dirMovement == DirectionOfMovement.Right && cellWithPieceToCapture.cellRight != null)
                {
                    pieceToMove.currentCell.isOccupied = false;
                    cellWithPieceToCapture.isOccupied = false;
                    StartCoroutine(MovePieceToCell(pieceToMove, cellWithPieceToCapture.cellRight));
                    GameManagerVsComputer.instance.playerList[0].PlayerPiecesInside.Remove(cellWithPieceToCapture.currentPiece);
                    Destroy(cellWithPieceToCapture.currentPiece.gameObject);
                    CapturedAPiece = true;
                }
                if (dirMovement == DirectionOfMovement.Above && cellWithPieceToCapture.cellAbove != null)
                {
                    pieceToMove.currentCell.isOccupied = false;
                    cellWithPieceToCapture.isOccupied = false;
                    StartCoroutine(MovePieceToCell(pieceToMove, cellWithPieceToCapture.cellAbove));
                    GameManagerVsComputer.instance.playerList[0].PlayerPiecesInside.Remove(cellWithPieceToCapture.currentPiece);
                    Destroy(cellWithPieceToCapture.currentPiece.gameObject);
                    CapturedAPiece = true;
                }
                if (dirMovement == DirectionOfMovement.Below && cellWithPieceToCapture.cellBelow != null)
                {
                    pieceToMove.currentCell.isOccupied = false;
                    cellWithPieceToCapture.isOccupied = false;
                    StartCoroutine(MovePieceToCell(pieceToMove, cellWithPieceToCapture.cellBelow));
                    GameManagerVsComputer.instance.playerList[0].PlayerPiecesInside.Remove(cellWithPieceToCapture.currentPiece);
                    Destroy(cellWithPieceToCapture.currentPiece.gameObject);
                    CapturedAPiece = true;
                }
                if (CapturedAPiece)
                {
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
            else
            {
                // Apenas mover para uma célula adjacente disponível
                CellScriptsVSComputer targetCell = FindAvailableAdjacentCell(pieceToMove.currentCell);
                if (targetCell != null)
                {
                    StartCoroutine(MovePieceToCell(pieceToMove, targetCell));
                }
            }

            currentPieceToMove = null;
        }
        else
        {
            // Apenas mover para uma célula adjacente disponível
            PieceScriptVsComputer pieceToMoveOnly = moveRandomPieceInsideBoard();
            CellScriptsVSComputer targetCell = FindAvailableAdjacentCell(pieceToMoveOnly.currentCell);
            if (targetCell != null)
            {
                pieceToMoveOnly.currentCell.isOccupied = false;
                StartCoroutine(MovePieceToCell(pieceToMoveOnly, targetCell));
            }
        }
    }

    private PieceScriptVsComputer moveRandomPieceInsideBoard()
    {
        // Verificar se há peças no tabuleiro
        if (computerPiecesInBoard.Count == 0)
        {
            return null;
        }

        // Criar uma lista de índices de peças
        List<int> indices = new List<int>();
        for (int i = 0; i < computerPiecesInBoard.Count; i++)
        {
            indices.Add(i);
        }

        // Embaralhar a lista de índices para escolher uma peça aleatória
        Shuffle(indices);

        // Tentar encontrar uma peça que não pode capturar para mover
        foreach (int index in indices)
        {
            PieceScriptVsComputer piece = computerPiecesInBoard[index];
            if (!CanCapture(piece))
            {
                return piece;
            }
        }

        // Se todas as peças puderem capturar, retornar null
        return null;
    }

    // Método para embaralhar uma lista de índices
    private void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            int temp = list[i];
            list[i] = list[j];
            list[j] = temp;
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
            int chooseCapture = UnityEngine.Random.Range(0, adjacentCellsToTheCurrentCell.Count);
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

    private IEnumerator CheckForChainCapture(PieceScriptVsComputer piece, int countCaptures)
    {

        CellScriptsVSComputer cellWithPieceToCapture = CanCapture(piece); // aqui se retornar a cell ja deve ficar correto
     

        if (cellWithPieceToCapture)
        {
            PieceScriptVsComputer adjacentPiece = cellWithPieceToCapture.currentPiece;
            if (adjacentPiece != null && adjacentPiece.isPlayerOnePiece != piece.isPlayerOnePiece)
            {

                if (cellWithPieceToCapture != null && cellWithPieceToCapture.isOccupied)
                {
                    piece.currentCell.isOccupied = false;
                    switch (dirMovement)
                    {

                        case DirectionOfMovement.Right: StartCoroutine(MovePieceToCell(piece, cellWithPieceToCapture.cellRight)); break;
                        case DirectionOfMovement.Left: StartCoroutine(MovePieceToCell(piece, cellWithPieceToCapture.cellLeft)); break;
                        case DirectionOfMovement.Above: StartCoroutine(MovePieceToCell(piece, cellWithPieceToCapture.cellAbove)); break;
                        case DirectionOfMovement.Below: StartCoroutine(MovePieceToCell(piece, cellWithPieceToCapture.cellBelow)); break;

                    }

                    adjacentPiece.currentCell.isOccupied = false;
                    Destroy(adjacentPiece.gameObject);
                    countCaptures++;
                }
            }
        }
       
        yield return null;


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

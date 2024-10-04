using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using TMPro;

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
    public float delayBetweenMoves = 1f;
    private string lastCaptureDirection = "";
    [SerializeField]
    private DirectionOfMovement dirMovement;
    public static ComputerPlayer instance;

    public TextMeshProUGUI textCountComputerPieces;

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
        
        
        if (computerPieces.Count > 0) {
            if (computerPiecesOutBoard.Count > 0)
            {
                if (computerPiecesInBoard.Count > 0)
                {
                   

                    currentPieceToMove = ChoosePiecetoMove();
                      

                    // Decidir se vai jogar uma nova peça ou mover uma peça existente
                    bool canCapturePiece = DecideIfCanCapturePiece();
                    if (!canCapturePiece)
                    {
                        bool playPiece = UnityEngine.Random.Range(0, 2) == 0;
                        if(currentPieceToMove != null)
                        {
                            playPiece = UnityEngine.Random.Range(0, 2) == 0;
                        }

                        if (playPiece || currentPieceToMove == null)
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
                            DecideDirectionOfMovement(currentPieceToMove);
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
                if (computerPiecesInBoard.Count > 0)
                {
                    currentPieceToMove = ChoosePiecetoMove();
                    // Decidir se vai jogar uma nova peça ou mover uma peça existente
                    bool canCapturePiece = DecideIfCanCapturePiece();
                    if (!canCapturePiece)
                    {
                        bool playPiece = UnityEngine.Random.Range(0, 2) == 0;
                        if (playPiece && computerPiecesOutBoard.Count > 0)
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
                            DecideDirectionOfMovement(currentPieceToMove);
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
                
            }
        }
        // Esperar um momento após a jogada do computador
        yield return new WaitForSeconds(delayBetweenMoves);

        // Finalizar turno do computador
        GameManagerVsComputer.instance.EndTurn();
    }


    private CellScriptsVSComputer DecideDirectionOfMovement(PieceScriptVsComputer piece)
    {
        List<DirectionOfMovement> directionsPossible = new List<DirectionOfMovement>();
        try
        {
            if (piece != null)
            {
                CellScriptsVSComputer currentCell = piece.currentCell;

                if (currentCell.cellLeft != null && !currentCell.cellLeft.isOccupied)
                {
                    
                    directionsPossible.Add(DirectionOfMovement.Left);
                }
                if (currentCell.cellRight != null && !currentCell.cellRight.isOccupied)
                {
                    directionsPossible.Add(DirectionOfMovement.Right);
                }
                if (currentCell.cellAbove != null && !currentCell.cellAbove.isOccupied)
                {
                    directionsPossible.Add(DirectionOfMovement.Above);
                }
                if (currentCell.cellBelow != null && !currentCell.cellBelow.isOccupied)
                {
                    directionsPossible.Add(DirectionOfMovement.Below);
                }
            }

            if(directionsPossible.Count > 0)
            {
                int randomDirectionValue = UnityEngine.Random.Range(0, directionsPossible.Count);
                dirMovement = directionsPossible[randomDirectionValue];
            }

        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }
        return null;
    }
    private void IsComputerStucked()
    {
       
        foreach(var piece in computerPiecesInBoard)
        {
            bool IsPieceStuck = GameManagerVsComputer.instance.CheckIfPieceIsStuck(currentPieceToMove);
          
        }
       
    }
    private PieceScriptVsComputer ChoosePiecetoMove()
    {
        List<PieceScriptVsComputer> piecesToChooseFromRandomly = new List<PieceScriptVsComputer>();
        // Verificar se há alguma peça em board que pode capturar uma peça inimiga adjacente
        int HowManyPieceAreStucked = 0;
        foreach (var piece in computerPiecesInBoard)
        {
            bool IsPieceStuckOther = GameManagerVsComputer.instance.CheckIfPieceIsStuck(piece);
            if (IsPieceStuckOther)
            {
                HowManyPieceAreStucked++;
            }
            if (!IsPieceStuckOther)
            {
                piecesToChooseFromRandomly.Add(piece);

                
            }


        }
        if (HowManyPieceAreStucked == computerPieces.Count)
        {
            Debug.Log("O jogador ganhou");
        }
        if (piecesToChooseFromRandomly.Count > 0)
        {
            int randomIndexPiece = UnityEngine.Random.Range(0, piecesToChooseFromRandomly.Count);
            if (piecesToChooseFromRandomly[randomIndexPiece] != null)
            {
                if (!CanCapture(piecesToChooseFromRandomly[randomIndexPiece]))
                {
                    HowManyPieceAreStucked = 0;
                    return piecesToChooseFromRandomly[randomIndexPiece];

                }
            }
        }

        return null;
    }
    private bool CanPieceBeMoved(PieceScriptVsComputer piece)
    {
        bool IsPieceStuck = false;
        if (currentPieceToMove) { 
            if (!currentPieceToMove.isOutSide)
            {
                IsPieceStuck = GameManagerVsComputer.instance.CheckIfPieceIsStuck(piece);
            }
        }

        if (CanCapture(currentPieceToMove) && !IsPieceStuck)
        {
            return true;
        }
        return false;
    }
    private bool DecideIfCanCapturePiece()
    {
        bool IsPieceStuck = false;
        if (currentPieceToMove)
        {
            if (!currentPieceToMove.isOutSide)
            {
                IsPieceStuck = GameManagerVsComputer.instance.CheckIfPieceIsStuck(currentPieceToMove);
            }
        }
        if (CanCapture(currentPieceToMove) && !IsPieceStuck)
        {
            return true;
        }
        else
        {
            // Verificar se há alguma peça em board que pode capturar uma peça inimiga adjacente
            foreach (var piece in computerPiecesInBoard)
            {
                if (piece != null)
                {
                    bool IsPieceStuckOther = GameManagerVsComputer.instance.CheckIfPieceIsStuck(piece);
                    if (CanCapture(piece) && !IsPieceStuckOther)
                    {
                        // Se houver uma peça que pode capturar, não colocar uma nova peça
                        currentPieceToMove = piece;
                        return true;
                    }
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
            if(piece != null)
            {
                CellScriptsVSComputer currentCell = piece.currentCell;

                if (currentCell.cellLeft != null && currentCell.cellLeft.isOccupied && currentCell.cellLeft.currentPiece != null && currentCell.cellLeft.currentPiece.isPlayerOnePiece != piece.isPlayerOnePiece && currentCell.cellLeft.cellLeft != null && !currentCell.cellLeft.cellLeft.isOccupied)
                {
                    dirMovement = DirectionOfMovement.Left;
                    return currentCell.cellLeft;
                }
                if (currentCell.cellRight != null && currentCell.cellRight.isOccupied && currentCell.cellRight.currentPiece != null && currentCell.cellRight.currentPiece.isPlayerOnePiece != piece.isPlayerOnePiece && currentCell.cellRight.cellRight != null && !currentCell.cellRight.cellRight.isOccupied)
                {
                    dirMovement = DirectionOfMovement.Right;
                    return currentCell.cellRight;
                }
                if (currentCell.cellAbove != null && currentCell.cellAbove.isOccupied && currentCell.cellAbove.currentPiece != null && currentCell.cellAbove.currentPiece.isPlayerOnePiece != piece.isPlayerOnePiece && currentCell.cellAbove.cellAbove != null && !currentCell.cellAbove.cellAbove.isOccupied)
                {
                    dirMovement = DirectionOfMovement.Above;
                    return currentCell.cellAbove;
                }
                if (currentCell.cellBelow != null && currentCell.cellBelow.isOccupied && currentCell.cellBelow.currentPiece != null && currentCell.cellBelow.currentPiece.isPlayerOnePiece != piece.isPlayerOnePiece && currentCell.cellBelow.cellBelow != null && !currentCell.cellBelow.cellBelow.isOccupied)
                {
                    dirMovement = DirectionOfMovement.Below;
                    return currentCell.cellBelow;
                }
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
       
       
        piece.transform.position = endPosition + new Vector3(0,1,0);
        
        piece.currentCell = targetCell;
        piece.isOutSide = false;
        targetCell.isOccupied = true;
        targetCell.currentPiece = piece;
        SoundManager.instance.PlaySoundPiece();

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
            CellScriptsVSComputer cellWithPieceToCapture = null;
            switch (dirMovement)
            {
                case DirectionOfMovement.Above: cellWithPieceToCapture = pieceToMove.currentCell.cellAbove; break;
                case DirectionOfMovement.Below: cellWithPieceToCapture = pieceToMove.currentCell.cellBelow; break;
                case DirectionOfMovement.Left: cellWithPieceToCapture = pieceToMove.currentCell.cellLeft; break;
                case DirectionOfMovement.Right: cellWithPieceToCapture = pieceToMove.currentCell.cellRight; break;
            }

            // Verificar se a célula de captura está ocupada
            if (cellWithPieceToCapture != null && cellWithPieceToCapture.isOccupied && cellWithPieceToCapture.currentPiece.isPlayerOnePiece != pieceToMove.isPlayerOnePiece)
            {
                bool capturedAPiece = false;
                dirMovement = GetDirection(pieceToMove, cellWithPieceToCapture);

                // Mover e capturar com base na direção
                if (dirMovement == DirectionOfMovement.Left && cellWithPieceToCapture.cellLeft != null)
                {
                    StartCoroutine(MovePieceAndCapture(pieceToMove, cellWithPieceToCapture, cellWithPieceToCapture.cellLeft));
                    capturedAPiece = true;
                }
                else if (dirMovement == DirectionOfMovement.Right && cellWithPieceToCapture.cellRight != null)
                {
                    StartCoroutine(MovePieceAndCapture(pieceToMove, cellWithPieceToCapture, cellWithPieceToCapture.cellRight));
                    capturedAPiece = true;
                }
                else if (dirMovement == DirectionOfMovement.Above && cellWithPieceToCapture.cellAbove != null)
                {
                    StartCoroutine(MovePieceAndCapture(pieceToMove, cellWithPieceToCapture, cellWithPieceToCapture.cellAbove));
                    capturedAPiece = true;
                }
                else if (dirMovement == DirectionOfMovement.Below && cellWithPieceToCapture.cellBelow != null)
                {
                    StartCoroutine(MovePieceAndCapture(pieceToMove, cellWithPieceToCapture, cellWithPieceToCapture.cellBelow));
                    capturedAPiece = true;
                }

                if (capturedAPiece)
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
                    pieceToMove.currentCell.isOccupied = false;
                    pieceToMove.currentCell.currentPiece = null;
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
                pieceToMoveOnly.currentCell.currentPiece = null;
                StartCoroutine(MovePieceToCell(pieceToMoveOnly, targetCell));
            }
        }
    }


    IEnumerator MovePieceAndCapture(PieceScriptVsComputer pieceToMove, CellScriptsVSComputer cellWithPieceToCapture, CellScriptsVSComputer targetCell)
    {
        pieceToMove.currentCell.isOccupied = false;
        pieceToMove.currentCell.currentPiece = null;
        cellWithPieceToCapture.isOccupied = false;
        

        yield return StartCoroutine(MovePieceToCell(pieceToMove, targetCell));

        if (GameManagerVsComputer.instance.ListOfpieceThatHasAnotherPieceToCapture.Contains(cellWithPieceToCapture.currentPiece))
        {
            GameManagerVsComputer.instance.ListOfpieceThatHasAnotherPieceToCapture.Remove(cellWithPieceToCapture.currentPiece);
        }
        GameManagerVsComputer.instance.playerList[0].PlayerPieces.Remove(cellWithPieceToCapture.currentPiece);
        GameManagerVsComputer.instance.playerList[0].PlayerPiecesInside.Remove(cellWithPieceToCapture.currentPiece);
        GameManagerVsComputer.instance.playerList[0].textCountComputerPieces.text = GameManagerVsComputer.instance.playerList[0].PlayerPieces.Count.ToString();
        GameManagerVsComputer.instance.CheckIfPlayerLost();
        Destroy(cellWithPieceToCapture.currentPiece.gameObject);
        cellWithPieceToCapture.currentPiece = null;
        SoundManager.instance.PlaySoundPiece();
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
        List<CellScriptsVSComputer> cellsAvaible = new List<CellScriptsVSComputer>();
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
        List<CellScriptsVSComputer> availableAdjacentCells = new List<CellScriptsVSComputer>();
        // Encontrar uma célula adjacente não ocupada para mover a peça
        if (currentCell.cellAbove != null && !currentCell.cellAbove.isOccupied) availableAdjacentCells.Add(currentCell.cellAbove);
        if (currentCell.cellBelow != null && !currentCell.cellBelow.isOccupied) availableAdjacentCells.Add(currentCell.cellBelow);
        if (currentCell.cellLeft != null && !currentCell.cellLeft.isOccupied) availableAdjacentCells.Add(currentCell.cellLeft);
        if (currentCell.cellRight != null && !currentCell.cellRight.isOccupied) availableAdjacentCells.Add(currentCell.cellRight);

        if(availableAdjacentCells.Count > 0)
        {
            int randomAdjacenteCell = UnityEngine.Random.Range(0, availableAdjacentCells.Count);
            return availableAdjacentCells[randomAdjacenteCell];
        }
        else
        {
            return null;
        }
        
    }

    private IEnumerator CheckForChainCapture(PieceScriptVsComputer piece, int countCaptures)
    {
        bool canCapture = true; // Define uma flag para controlar o loop

        while (canCapture)
        {
            CellScriptsVSComputer cellWithPieceToCapture = CanCapture(piece); // Verifica se pode capturar

            if (cellWithPieceToCapture != null)
            {
                PieceScriptVsComputer adjacentPiece = cellWithPieceToCapture.currentPiece;

                if (adjacentPiece != null && adjacentPiece.isPlayerOnePiece != piece.isPlayerOnePiece)
                {
                    // Realiza a captura
                    piece.currentCell.isOccupied = false;
                    piece.currentCell.currentPiece = null;

                    switch (dirMovement)
                    {
                        case DirectionOfMovement.Right:
                            yield return StartCoroutine(MovePieceToCell(piece, cellWithPieceToCapture.cellRight));
                            break;
                        case DirectionOfMovement.Left:
                            yield return StartCoroutine(MovePieceToCell(piece, cellWithPieceToCapture.cellLeft));
                            break;
                        case DirectionOfMovement.Above:
                            yield return StartCoroutine(MovePieceToCell(piece, cellWithPieceToCapture.cellAbove));
                            break;
                        case DirectionOfMovement.Below:
                            yield return StartCoroutine(MovePieceToCell(piece, cellWithPieceToCapture.cellBelow));
                            break;
                    }

                    // Remove a peça capturada
                    adjacentPiece.currentCell.currentPiece = null;
                    adjacentPiece.currentCell.isOccupied = false;
                    GameManagerVsComputer.instance.playerList[0].PlayerPieces.Remove(adjacentPiece);
                    GameManagerVsComputer.instance.playerList[0].PlayerPiecesInside.Remove(adjacentPiece);
                    GameManagerVsComputer.instance.playerList[0].textCountComputerPieces.text = GameManagerVsComputer.instance.playerList[0].PlayerPieces.Count.ToString();
                    GameManagerVsComputer.instance.CheckIfPlayerLost();
                    Destroy(adjacentPiece.gameObject);

                    // Incrementa a contagem de capturas
                    countCaptures++;
                }
            }
            else
            {
                canCapture = false; // Não há mais capturas possíveis, então sai do loop
            }

            yield return null; // Espera um frame antes de continuar
        }
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

using Photon.Pun;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class PieceScriptVsComputer : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    private bool isDragging = false;
    private Vector3 initialPosition;

    public CellScriptsVSComputer currentCell;
    public CellScriptsVSComputer lastCell;

    public bool isPlayerOnePiece = false;
    public bool isOutSide = true;
    public string lastCaptureDirection = "";
    private Animator animPice;

    void Start()
    {
        initialPosition = transform.position;
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Piece Layer"), LayerMask.NameToLayer("Piece Layer"), true);
        animPice = GetComponent<Animator>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (GameManagerVsComputer.instance.isPlayerOneTurn) {
            if (!GameManagerVsComputer.instance.IsPlayerTurn(isPlayerOnePiece))
            {
                return;
            }
            if (GameManagerVsComputer.instance.CheckIfPieceIsStuck(this) && !isOutSide && !GameManagerVsComputer.instance.ListOfpieceThatHasAnotherPieceToCapture.Contains(this))
            {
                return;
            }
            GameManagerVsComputer.instance.VerifyIfHasNoPieceToCapture(GameManagerVsComputer.instance.isPlayerOneTurn);
            // Verifique se há peças que podem capturar antes de permitir arrastar
            if (GameManagerVsComputer.instance.ListOfpieceThatHasAnotherPieceToCapture.Count > 0 &&
                !GameManagerVsComputer.instance.ListOfpieceThatHasAnotherPieceToCapture.Contains(this))
            {
                for(int i = 0; i < GameManagerVsComputer.instance.ListOfpieceThatHasAnotherPieceToCapture.Count; i++)
                {
                    GameManagerVsComputer.instance.HighlightAdjacentCells(GameManagerVsComputer.instance.ListOfpieceThatHasAnotherPieceToCapture[i].lastCell, GameManagerVsComputer.instance.ListOfpieceThatHasAnotherPieceToCapture[i].isPlayerOnePiece, GameManagerVsComputer.instance.ListOfpieceThatHasAnotherPieceToCapture[i]);
                }
                // Existe outra peça que deve capturar, então não permita o movimento
                Debug.Log("Você deve usar uma peça que pode capturar!");
                return;
            }
           
            isDragging = true;

            if (lastCell == null)
            {
                GameManagerVsComputer.instance.HighlightAllUnoccupiedCells();
            }
            else
            {
                GameManagerVsComputer.instance.HighlightAdjacentCells(lastCell, isPlayerOnePiece, this);
            }

        }
        else
        {
            Debug.Log("Not your turn!");
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            Vector3 newPosition = GetMouseWorldPosition();
            transform.position = new Vector3(newPosition.x, transform.position.y, newPosition.z);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        isDragging = false;

        bool capturedAPiece = false;

        if (currentCell != null && currentCell.isHighlighted && GameManagerVsComputer.instance.IsPlayerTurn(isPlayerOnePiece))
        {
            if (!currentCell.isOccupied)
            {
                if (!isOutSide)
                {
                    capturedAPiece = HandlePieceCapture();
                }
                if (isOutSide)
                {
                    GameManagerVsComputer.instance.playerList[0].PlayerPiecesInside.Add(this);
                    GameManagerVsComputer.instance.EndTurn();
                }
                else
                {
                    if (capturedAPiece)
                    {
                        if (!CheckAndHandleAdjacentCaptures())
                        {
                            GameManagerVsComputer.instance.EndTurn();
                        }
                    }
                    else
                    {
                        GameManagerVsComputer.instance.EndTurn();
                    }
                }

                if (lastCell)
                {
                    lastCell.isOccupied = false;
                    lastCell.currentPiece = null;
                }

                lastCell = currentCell;
                initialPosition = currentCell.transform.position;
                isOutSide = false;
                SoundManager.instance.PlaySoundPiece();
                currentCell.isOccupied = true;
                currentCell.currentPiece = this;
                transform.position = currentCell.transform.position + new Vector3(0,1,0);
                
            }
            else
            {
                transform.position = initialPosition + new Vector3(0, 1, 0);
                if (!isOutSide)
                {
                    currentCell = lastCell;
                }
            }
        }
        else
        {
            transform.position = initialPosition + new Vector3(0, 1, 0);
            if (!isOutSide)
            {
                currentCell = lastCell;
            }
        }

        GameManagerVsComputer.instance.ClearHighlightedCells();
    }

    private bool HandlePieceCapture()
    {
        CellScriptsVSComputer middleCell = GameManagerVsComputer.instance.GetMiddleCell(lastCell, currentCell);
        if (middleCell != null && middleCell.isOccupied)
        {
            PieceScriptVsComputer capturedPiece = middleCell.currentPiece;
            if (capturedPiece != null && capturedPiece.GetComponent<PieceScriptVsComputer>().isPlayerOnePiece != isPlayerOnePiece)
            {
                ComputerPlayer.instance.computerPiecesInBoard.Remove(capturedPiece); 
                GameManagerVsComputer.instance.RemovePiece(capturedPiece);
                lastCell.currentPiece = null;
                lastCaptureDirection = GameManagerVsComputer.instance.GetCaptureDirection(lastCell, currentCell);
                
                return true;
            }
            return false;
        }

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Cell") && other.GetComponent<CellScriptsVSComputer>().currentPiece == null)
        {
            currentCell = other.GetComponent<CellScriptsVSComputer>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Cell"))
        {
            CellScriptsVSComputer cell = other.GetComponent<CellScriptsVSComputer>();
            if (cell == currentCell)
            {
                cell.currentPiece = null;
                currentCell = null;
            }
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    public bool CheckAndHandleAdjacentCaptures()
    {
        bool canCaptureAgain = false;
        int countCaptures = 0;

        canCaptureAgain = CheckCapture(currentCell.cellRight, "Right", ref countCaptures) || canCaptureAgain;
        canCaptureAgain = CheckCapture(currentCell.cellAbove, "Above", ref countCaptures) || canCaptureAgain;
        canCaptureAgain = CheckCapture(currentCell.cellLeft, "Left", ref countCaptures) || canCaptureAgain;
        canCaptureAgain = CheckCapture(currentCell.cellBelow, "Below", ref countCaptures) || canCaptureAgain;

        if (canCaptureAgain && countCaptures < 1)
        {
            CellScriptsVSComputer adjacentCell = GetAdjacentCell(currentCell, lastCaptureDirection);
            PieceScriptVsComputer adjacentPiece = adjacentCell.GetComponentInChildren<PieceScriptVsComputer>();
            if (adjacentPiece != null && adjacentPiece.isPlayerOnePiece != isPlayerOnePiece)
            {
                CellScriptsVSComputer jumpOverCell = GameManagerVsComputer.instance.GetMiddleCell(currentCell, adjacentCell);
                if (jumpOverCell != null && jumpOverCell.isOccupied)
                {
                    GameManagerVsComputer.instance.HighlightCell(jumpOverCell);
                    countCaptures++;
                }
            }
        }

        countCaptures = 0;
        return canCaptureAgain;
    }

    public bool CheckCapture(CellScriptsVSComputer cell, string direction, ref int countCaptures)
    {
        if (cell != null)
        {
            CellScriptsVSComputer nextCell = GetNextCell(cell, direction);
            if (nextCell != null)
            {
                PieceScriptVsComputer piece = cell.currentPiece;
                if (piece != null && countCaptures < 1)
                {
                    if (!nextCell.isOccupied && cell.isOccupied && piece.isPlayerOnePiece != isPlayerOnePiece)
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

    private CellScriptsVSComputer GetAdjacentCell(CellScriptsVSComputer cell, string direction)
    {
        switch (direction)
        {
            case "Up":
                return cell.cellAbove;
            case "Down":
                return cell.cellBelow;
            case "Left":
                return cell.cellLeft;
            case "Right":
                return cell.cellRight;
            default:
                return null;
        }
    }
}

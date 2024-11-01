using Photon.Pun;
using UnityEngine.EventSystems;
using UnityEngine;
using System.Linq;

public class PieceScript : MonoBehaviourPun, IPointerDownHandler, IDragHandler, IDropHandler
{
    private bool isDragging = false;
    private Vector3 initialPosition;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    public CellScripts currentCell;
    public CellScripts lastCell;

    public bool isPlayerOnePiece = false; // true = peças do jogador 1 (azul), false = peças do jogador 2 (vermelho)
    public bool isOutSide = true;
    private string lastCaptureDirection = "";
    private int idplayerCurrent = -1;

    void Start()
    {
        initialPosition = transform.position;
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Piece Layer"), LayerMask.NameToLayer("Piece Layer"), true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Verifica se é a vez do jogador e se a peça é do jogador
        if (!GameManager.instance.IsPlayerTurn(isPlayerOnePiece) || !IsPlayerPieceAllowed())
        {
            return; // Não é a vez do jogador atual ou peça não pertence ao jogador atual
        }
       
        int playerPlayingId = this.gameObject.tag == "PiecePlayer1" ? GameManager.instance.playerList[0].gameObject.GetComponent<PhotonView>().ViewID : GameManager.instance.playerList[1].gameObject.GetComponent<PhotonView>().ViewID;
        idplayerCurrent = playerPlayingId;



            GameManager.instance.VerifyIfHasNoPieceToCapture(isPlayerOnePiece, playerPlayingId);
        
        
        // Verifique se há peças que podem capturar antes de permitir arrastar
        if (GameManager.instance.ListOfpieceThatHasAnotherPieceToCapture.Count > 0 &&
            !GameManager.instance.ListOfpieceThatHasAnotherPieceToCapture.Contains(this))
        {
            for (int i = 0; i < GameManager.instance.ListOfpieceThatHasAnotherPieceToCapture.Count; i++)
            {
                GameManager.instance.HighlightAdjacentCells(GameManager.instance.ListOfpieceThatHasAnotherPieceToCapture[i].lastCell, GameManager.instance.ListOfpieceThatHasAnotherPieceToCapture[i].isPlayerOnePiece, GameManager.instance.ListOfpieceThatHasAnotherPieceToCapture[i]);
            }
            // Existe outra peça que deve capturar, então não permita o movimento
            Debug.Log("Você deve usar uma peça que pode capturar!");
            return;
        }
        if (!isOutSide)
        {
            if (GameManager.instance.CheckIfPieceIsStuck(this) && !GameManager.instance.ListOfpieceThatHasAnotherPieceToCapture.Contains(this))
            {
                return;
            }
        }
        
        isDragging = true;

        if (lastCell == null)
        {
            GameManager.instance.HighlightAllUnoccupiedCells();
        }
        else
        {
            GameManager.instance.HighlightAdjacentCells(lastCell, isPlayerOnePiece, this);
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

    public void OnDrop(PointerEventData eventData)
    {
        if (!isDragging) return;
        isDragging = false;

        bool capturedAPiece = false;

        
        // Verifica se a célula é válida e se é a vez do jogador
        if (currentCell != null && currentCell.isHighlighted && GameManager.instance.IsPlayerTurn(isPlayerOnePiece))
        {
            if (!currentCell.isOccupied)
            {
                if (!isOutSide)
                {
                    capturedAPiece = HandlePieceCapture();
                }
                if (isOutSide)
                {
                    int playerPlayingId = this.gameObject.tag == "PiecePlayer1" ? GameManager.instance.playerList[0].gameObject.GetComponent<PhotonView>().ViewID : GameManager.instance.playerList[1].gameObject.GetComponent<PhotonView>().ViewID;
                    Player playerPlaying = PhotonView.Find(playerPlayingId).GetComponent<Player>();
                    playerPlaying.PlayerPiecesInside.Add(this);
                    GameManager.instance.EndTurn();

                }
                else
                {
                    if (capturedAPiece)
                    {
                        // Verifica se há peças adjacentes capturáveis após a captura inicial
                        if (!CheckAndHandleAdjacentCaptures())
                        {
                            GameManager.instance.EndTurn();
                        }
                    }
                    else
                    {
                        photonView.RPC("RPC_UpdateLastCellState", RpcTarget.All, lastCell.gameObject.GetComponent<PhotonView>().ViewID);
                        // Não capturou peça, portanto termina logo o turno
                        GameManager.instance.EndTurn();
                    }
                }
                if (lastCell)
                {
                    lastCell.isOccupied = false; lastCell.currentPiece = null;
                }
               

                // Atualizar a posição para todos os jogadores
                photonView.RPC("RPC_UpdatePiecePosition", RpcTarget.Others, transform.position, currentCell.gameObject.GetComponent<PhotonView>().ViewID);
                lastCell = currentCell;
                initialPosition = currentCell.transform.position;
                isOutSide = false;
                SoundManager.instance.PlaySoundPiece();
                currentCell.isOccupied = true;
                transform.position = currentCell.transform.position;

                
            }
            else
            {
                transform.position = initialPosition;
                if (!isOutSide)
                {
                    currentCell = lastCell;
                }
                
            }
        }
        else
        {
            transform.position = initialPosition;
            if (!isOutSide)
            {
                currentCell = lastCell;
            }
        }

        GameManager.instance.ClearHighlightedCells();
    }

    [PunRPC]
    private void RPC_UpdatePiecePosition(Vector3 newPosition, int cellViewID)
    {
        // Atualiza a posição da peça para todos os jogadores
        transform.position = newPosition;
     
        CellScripts cell = PhotonView.Find(cellViewID).GetComponent<CellScripts>();
        if (cell != null)
        {
            transform.SetParent(cell.transform);
            cell.isOccupied = true;
            cell.currentPiece = this;
            this.currentCell = cell;
        }
        int pieceid = this.gameObject.GetComponent<PhotonView>().ViewID;
        PieceScript pieceThis = PhotonView.Find(pieceid).GetComponent<PieceScript>();
        int playerPlayingId = this.gameObject.tag == "PiecePlayer1" ? GameManager.instance.playerList[0].gameObject.GetComponent<PhotonView>().ViewID : GameManager.instance.playerList[1].gameObject.GetComponent<PhotonView>().ViewID;
        Player playerPlaying = PhotonView.Find(playerPlayingId).GetComponent<Player>();
        if (!playerPlaying.PlayerPiecesInside.Contains(this)) { 
            playerPlaying.PlayerPiecesInside.Add(this);
        }

        pieceThis.transform.SetParent(cell.transform);
        pieceThis.transform.position = cell.transform.position;
        if(pieceThis.lastCell)
        pieceThis.lastCell.isOccupied = false;

    }
    [PunRPC]
    private void RPC_UpdateLastCellState(int cellViewID)
    {
        

        CellScripts cell = PhotonView.Find(cellViewID).GetComponent<CellScripts>();
        if (cell != null)
        {
            cell.isOccupied = false;
            cell.currentPiece = null;
        }


    }
    private bool HandlePieceCapture()
    {
        CellScripts middleCell = GameManager.instance.GetMiddleCell(lastCell, currentCell);
        if (middleCell != null && middleCell.isOccupied)
        {
            PieceScript capturedPiece = middleCell.GetComponentInChildren<PieceScript>();
            if (capturedPiece != null && capturedPiece.isPlayerOnePiece != isPlayerOnePiece)
            {
               GameManager.instance.RemovePiece(capturedPiece);
               lastCaptureDirection = GameManager.instance.GetCaptureDirection(lastCell, currentCell);
                Debug.Log("Direction of capture: " + lastCaptureDirection);
                photonView.RPC("RPC_UpdateLastCellState", RpcTarget.All, lastCell.photonView.ViewID);
                // Atualizar a remoção da peça capturada para todos os jogadores
                photonView.RPC("RPC_RemoveCapturedPiece", RpcTarget.Others, capturedPiece.photonView.ViewID);

                capturedPiece.currentCell.isOccupied = false;
                if (capturedPiece.lastCell) capturedPiece.lastCell.isOccupied = false;
                if (capturedPiece.lastCell) capturedPiece.lastCell.isOccupied = false;

                int playerPlayingId = this.gameObject.tag == "Player1Piece" ? GameManager.instance.playerList[0].gameObject.GetComponent<PhotonView>().ViewID : GameManager.instance.playerList[1].gameObject.GetComponent<PhotonView>().ViewID;
                foreach (Player p in GameManager.instance.playerList)
                {
                    if (p.GetComponent<PhotonView>().ViewID == playerPlayingId)
                    {
                        p.PlayerPiecesInside.Remove(capturedPiece.GetComponent<PieceScript>());
                    }
                }
                //GameManager.instance.RemovePiece(capturedPiece);
                return true;
            }
            return false;
        }

        return false;
    }

    [PunRPC]
    private void RPC_RemoveCapturedPiece(int pieceViewID)
    {
        GameObject piece = PhotonView.Find(pieceViewID).gameObject;
        if (piece != null)
        {
            piece.GetComponent<PieceScript>().currentCell.isOccupied = false;
            if (piece.GetComponent<PieceScript>().lastCell) piece.GetComponent<PieceScript>().lastCell.isOccupied = false;
            if (this.lastCell) this.lastCell.isOccupied = false;

            PieceScript pieceThis = PhotonView.Find(pieceViewID).GetComponent<PieceScript>();
            
            Destroy(piece);
        }

        
    }
 
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Cell") && other.GetComponent<CellScripts>().currentPiece == null)
        {
            currentCell = other.GetComponent<CellScripts>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Cell"))
        {
            CellScripts cell = other.GetComponent<CellScripts>();
            if (cell == currentCell)
            {
                currentCell = null;
                //photonView.RPC("RPC_UpdateLastCellState", RpcTarget.All, currentCell.gameObject.GetComponent<PhotonView>().ViewID);

            }
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    private bool CheckAndHandleAdjacentCaptures()
    {
        bool canCaptureAgain = false;
        int countCaptures = 0;

        // Checa captura para a direção especificada
        canCaptureAgain = CheckCapture(currentCell.cellRight, "Right", ref countCaptures) || canCaptureAgain;
        canCaptureAgain = CheckCapture(currentCell.cellAbove, "Above", ref countCaptures) || canCaptureAgain;
        canCaptureAgain = CheckCapture(currentCell.cellLeft, "Left", ref countCaptures) || canCaptureAgain;
        canCaptureAgain = CheckCapture(currentCell.cellBelow, "Below", ref countCaptures) || canCaptureAgain;

        if (canCaptureAgain && countCaptures < 1)
        {
            CellScripts adjacentCell = GetAdjacentCell(currentCell, lastCaptureDirection);
            PieceScript adjacentPiece = adjacentCell.GetComponentInChildren<PieceScript>();
            if (adjacentPiece != null && adjacentPiece.isPlayerOnePiece != isPlayerOnePiece)
            {
                CellScripts jumpOverCell = GameManager.instance.GetMiddleCell(currentCell, adjacentCell);
                if (jumpOverCell != null && jumpOverCell.isOccupied)
                {
                    GameManager.instance.HighlightCell(jumpOverCell);
                    countCaptures++;
                }
            }
        }

        countCaptures = 0;
        return canCaptureAgain;
    }

    private bool CheckCapture(CellScripts cell, string direction, ref int countCaptures)
    {
        if (cell != null)
        {
            CellScripts nextCell = GetNextCell(cell, direction);
            if (nextCell != null)
            {
                PieceScript piece = cell.GetComponentInChildren<PieceScript>();
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

    private CellScripts GetNextCell(CellScripts cell, string direction)
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

    private CellScripts GetAdjacentCell(CellScripts cell, string direction)
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

    private bool IsPlayerPieceAllowed()
    {
        // Se for um jogador 1, verificar se a peça é do jogador 1
        // Se for um jogador 2, verificar se a peça é do jogador 2
        if (isPlayerOnePiece && PhotonNetwork.LocalPlayer.ActorNumber != 1)
        {
            return false;
        }
        else if (!isPlayerOnePiece && PhotonNetwork.LocalPlayer.ActorNumber != 2)
        {
            return false;
        }
        return true;
    }

    private void OnDestroy()
    {
        if (currentCell)
        {
            currentCell.isOccupied = false;
            currentCell.currentPiece = null;
        }
    }

}

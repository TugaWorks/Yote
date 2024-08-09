using Photon.Pun;
using System.Collections;
using UnityEngine;

public class CellScripts : MonoBehaviourPun, IPunObservable
{
    public bool isOccupied = false;
    public bool isHighlighted = false;
    public Material OriginalMaterial;
    public Material MaterialHighlight;

    private Renderer cellRenderer;

    public CellScripts cellAbove;
    public CellScripts cellBelow;
    public CellScripts cellLeft;
    public CellScripts cellRight;

    public CellScripts jumpOverCell;
    public PieceScript currentPiece;

    public Vector3 originalPosition;
    private Animator animator;

    public TextMesh textName;

    public int row, column;

    void Start()
    {
        cellRenderer = GetComponent<Renderer>();
        animator = GetComponent<Animator>();
        originalPosition = this.transform.position;
        textName.text = name;
      
    }

    void Update()
    {
        // Lógica de atualização (se necessária)
    }

    public void SetOccupied(bool occupied)
    {
        isOccupied = occupied;
        UpdateMaterial();
    }

    private void UpdateMaterial()
    {
        if (isOccupied)
        {
            cellRenderer.material = OriginalMaterial;
        }
        else
        {
            cellRenderer.material = isHighlighted ? MaterialHighlight : OriginalMaterial;
        }
    }

    public void ToggleOccupied()
    {
        isOccupied = !isOccupied;
        UpdateMaterial();
    }

    public void Highlight()
    {
        isHighlighted = true;
        UpdateMaterial();
    }

    public void ClearHighlight()
    {
        isHighlighted = false;
        UpdateMaterial();
    }

    public void AnimCellOnClick()
    {
        Highlight();
        //StartCoroutine(AnimateCell());
        //AnimateAdjacentCells(true); // Highlight adjacentes

        if (PhotonNetwork.IsConnected)
        {
            //photonView.RPC("RPC_AnimateCell", RpcTarget.AllBuffered);
        }
    }

    private IEnumerator AnimateCell()
    {
        Vector3 downPosition = originalPosition + new Vector3(0, -0.1f, 0);

        float elapsedTime = 0;
        float duration = 0.1f;
        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(originalPosition, downPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = downPosition;

        elapsedTime = 0;
        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(downPosition, originalPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPosition;

        // Clear highlight after animation
        ClearHighlight();
        ClearAdjacentHighlights();
    }

    private void AnimateAdjacentCells(bool highlight)
    {
        if (cellAbove != null)
        {
            if (highlight) cellAbove.Highlight();
            //StartCoroutine(cellAbove.AnimateCell());
        }
        if (cellBelow != null)
        {
            if (highlight) cellBelow.Highlight();
            //StartCoroutine(cellBelow.AnimateCell());
        }
        if (cellLeft != null)
        {
            if (highlight) cellLeft.Highlight();
            //StartCoroutine(cellLeft.AnimateCell());
        }
        if (cellRight != null)
        {
            if (highlight) cellRight.Highlight();
            //StartCoroutine(cellRight.AnimateCell());
        }
    }

    private void ClearAdjacentHighlights()
    {
        if (cellAbove != null) cellAbove.ClearHighlight();
        if (cellBelow != null) cellBelow.ClearHighlight();
        if (cellLeft != null) cellLeft.ClearHighlight();
        if (cellRight != null) cellRight.ClearHighlight();
    }

    [PunRPC]
    void RPC_AnimateCell()
    {
        Highlight();
        StartCoroutine(AnimateCell());
        AnimateAdjacentCells(true); // Highlight adjacentes para todos os jogadores
    }

    public IEnumerator ElevateCellCoroutine()
    {
        Vector3 elevatedPosition = originalPosition + new Vector3(0, 0.3f, 0);

        float elapsedTime = 0;
        float duration = 0.1f;
        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(originalPosition, elevatedPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = elevatedPosition;
    }

    public IEnumerator ResetCellPositionCoroutine()
    {
        Vector3 elevatedPosition = originalPosition + new Vector3(0, 0.3f, 0);

        float elapsedTime = 0;
        float duration = 0.1f;
        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(elevatedPosition, originalPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPosition;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Envia os dados para os outros jogadores
            stream.SendNext(transform.position);
        }
        else
        {
            // Recebe os dados dos outros jogadores
            transform.position = (Vector3)stream.ReceiveNext();
        }
    }

    public void RemovePiece()
    {
        isOccupied = false;
        Destroy(currentPiece);
    }
}

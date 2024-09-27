using System.Collections;
using TMPro;
using UnityEngine;

public class CellScriptsVSComputer : MonoBehaviour
{
    public bool isOccupied = false;
    public bool isHighlighted = false;
    public Material OriginalMaterial;
    public Material OccupiedMaterial;
    public Material MaterialHighlight;

    public Renderer cellRenderer;

    public CellScriptsVSComputer cellAbove;
    public CellScriptsVSComputer cellBelow;
    public CellScriptsVSComputer cellLeft;
    public CellScriptsVSComputer cellRight;

    public CellScriptsVSComputer jumpOverCell;
    public PieceScriptVsComputer currentPiece;

    public Vector3 originalPosition;
    private Animator animator;

    public TextMeshProUGUI textName;

    public int row, column;

    void Start()
    {
        cellRenderer = GetComponent<Renderer>();
        animator = GetComponent<Animator>();
        originalPosition = this.transform.position;
        OriginalMaterial = GetComponent<Renderer>().material;
        textName = transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        textName.text = this.gameObject.name;
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



    private void ClearAdjacentHighlights()
    {
        if (cellAbove != null) cellAbove.ClearHighlight();
        if (cellBelow != null) cellBelow.ClearHighlight();
        if (cellLeft != null) cellLeft.ClearHighlight();
        if (cellRight != null) cellRight.ClearHighlight();
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



    public void RemovePiece()
    {
        isOccupied = false;
        Destroy(currentPiece);
    }

  
}

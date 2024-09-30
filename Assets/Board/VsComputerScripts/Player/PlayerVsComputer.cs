using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerVsComputer : MonoBehaviour
{
    public List<PieceScriptVsComputer> PlayerPieces = new List<PieceScriptVsComputer>();
    public List<PieceScriptVsComputer> PlayerPiecesInside = new List<PieceScriptVsComputer>();
    public List<PieceScriptVsComputer> PlayerPiecesOutside = new List<PieceScriptVsComputer>();

    public bool isPlayerOne = false;
    public Material playerMaterial;
    public TextMeshProUGUI textCountComputerPieces;

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        // Inicialização se necessário
    }

    void Update()
    {
        
            textCountComputerPieces.text = PlayerPieces.Count.ToString();
        
    }
}

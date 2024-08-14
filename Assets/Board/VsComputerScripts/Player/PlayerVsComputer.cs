using System.Collections.Generic;
using UnityEngine;

public class PlayerVsComputer : MonoBehaviour
{
    public List<GameObject> PlayerPieces = new List<GameObject>();
    public List<GameObject> PlayerPiecesInside = new List<GameObject>();
    public List<GameObject> PlayerPiecesOutside = new List<GameObject>();

    public bool isPlayerOne = false;
    public Material playerMaterial;

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
        // Atualizações se necessário
    }
}

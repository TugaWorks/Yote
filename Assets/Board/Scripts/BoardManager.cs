using Photon.Pun;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;

public class BoardManager : MonoBehaviourPunCallbacks
{
    public GameObject cellPrefab;
    public GameObject piecePrefab;
    public int rows = 6;
    public int columns = 5;
    public float cellSize = 1.0f;
    public Material whiteMaterial;
    public Material brownMaterial;
    public Material blueMaterial;
    public Material redMaterial;
    public Material TestMaterial;
    public Material TestMaterial1;

    public Transform player1Transform;
    public Transform player2Transform;
    public float pieceSpacing = 1.1f;

    public List<CellScripts> allCells = new List<CellScripts>();
    public CellScripts[,] cellGrid;

    public static BoardManager instance;

    private PhotonView photonView;

    public List<Player> players = new List<Player>();

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

        photonView = GetComponent<PhotonView>();
        if (photonView == null)
        {
            Debug.LogError("PhotonView component is missing on BoardManager.");
        }
    }

    void Start()
    {
        cellGrid = new CellScripts[rows, columns];
        foreach (CellScripts cell in allCells)
        {
            int row = cell.row;  // Assuming CellScripts has a row property
            int column = cell.column;  // Assuming CellScripts has a column property

            if (row >= 0 && row < rows && column >= 0 && column < columns)
            {
                cellGrid[row, column] = cell;
            }
            else
            {
                Debug.LogWarning($"Cell at row {row} and column {column} is out of bounds.");
            }
        }
        GameManager.instance.playerList = players;

        
            Photon.Realtime.Player[] photonPlayers = PhotonNetwork.PlayerList;

            for (int i = 0; i < photonPlayers.Length; i++)
            {
                // Associa o Photon Player ao Player Script
                GameManager.instance.playerList[i].Initialize(photonPlayers[i]);
            }
        
        SetupAdjacentCells();
    }

    void SetupPlayerPieces()
    {

       int localPlayerId = PhotonNetwork.LocalPlayer.ActorNumber;

        for (int i = 0; i < GameManager.instance.playerList.Count; i++)
        {
            Player currentPlayer = GameManager.instance.playerList[i];
            if (currentPlayer.GetComponent<PhotonView>().OwnerActorNr == localPlayerId)
            {
                Material playerMaterial = currentPlayer.playerMaterial;
                foreach (GameObject piece in currentPlayer.PlayerPieces)
                {
                    piece.GetComponent<Renderer>().material = playerMaterial;
                }
                break; // Exit the loop once the current player is found and processed
            }
        }
        
        
    }

    public List<CellScripts> GetAllCells()
    {
        return allCells;
    }

    public CellScripts GetCell(int row, int col)
    {
        if (row >= 0 && row < rows && col >= 0 && col < columns)
        {
            return cellGrid[row, col];
        }
        return null;
    }

    void SetupAdjacentCells()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                CellScripts cell = cellGrid[row, column];
                if (cell != null)
                {
                    cell.cellAbove = (row < rows - 1) ? cellGrid[row + 1, column] : null;
                    cell.cellBelow = (row > 0) ? cellGrid[row - 1, column] : null;
                    cell.cellLeft = (column > 0) ? cellGrid[row, column - 1] : null;
                    cell.cellRight = (column < columns - 1) ? cellGrid[row, column + 1] : null;
                }
            }
        }
    }


 

}

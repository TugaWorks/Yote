using System.Collections.Generic;
using UnityEngine;

public class BoardManagerVsComputer : MonoBehaviour
{
    public int rows = 6;
    public int columns = 5;
    public float cellSize = 1.0f;

    public Transform player1Transform;
    public Transform player2Transform;
    public float pieceSpacing = 1.1f;

    public List<CellScriptsVSComputer> allCells = new List<CellScriptsVSComputer>();
    public CellScriptsVSComputer[,] cellGrid;

    public static BoardManagerVsComputer instance;


    public List<PlayerVsComputer> players = new List<PlayerVsComputer>();

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
        cellGrid = new CellScriptsVSComputer[rows, columns];
        foreach (CellScriptsVSComputer cell in allCells)
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
        GameManagerVsComputer.instance.playerList = players;

        
        SetupAdjacentCells();
    }

 

    public List<CellScriptsVSComputer> GetAllCells()
    {
        return allCells;
    }

    public CellScriptsVSComputer GetCell(int row, int col)
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
                CellScriptsVSComputer cell = cellGrid[row, column];
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

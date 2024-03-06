using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.TextCore.Text;
using UnityEngine;
using UnityEngine.UIElements;

public class BoardManager : MonoBehaviour
{
    [SerializeField] private GameObject casePrefab;
    [SerializeField] private int numberOfRows;
    [SerializeField] private int numberOfColumns;
    [SerializeField] private float horizontalSpacing;
    [SerializeField] private float verticalSpacing;
    [SerializeField] private List<GameObject> pionDictionary;

    private GameObject selectedPiece;
    private Vector2Int selectedPiecePosition;

    private BoardCase boardCase;
    private GameObject[,] boardArray; // Array that represent the board

    private void Awake()
    {
        boardCase = casePrefab.GetComponent<BoardCase>();
    }

    private void Start()
    {
        GenerateBoard();
        StartFillArray();
        UpdateTilesClickability(null);
        LogBoardArray();
    }

    private void StartFillArray()
    {
        selectedPiece = pionDictionary[0];
        PlacePiece(new Vector2Int(0, 0));
        selectedPiece = pionDictionary[0];
        PlacePiece(new Vector2Int(3, 0));

        selectedPiece = pionDictionary[1];
        PlacePiece(new Vector2Int(0, 1));
        selectedPiece = pionDictionary[1];
        PlacePiece(new Vector2Int(3, 1));

        selectedPiece = pionDictionary[2];
        PlacePiece(new Vector2Int(0, 2));
        selectedPiece = pionDictionary[2];
        PlacePiece(new Vector2Int(3, 2));

        selectedPiece = pionDictionary[3];
        PlacePiece(new Vector2Int(2, 1));
        selectedPiece = pionDictionary[3];
        PlacePiece(new Vector2Int(1, 1));
    }

    private void GenerateBoard()
    {
        boardArray = new GameObject[numberOfRows, numberOfColumns];

        float startX = -(numberOfColumns - 1) * (boardCase.GetSize().x + horizontalSpacing) / 2f;
        float startY = -(numberOfRows - 1) * (boardCase.GetSize().y + verticalSpacing) / 2f;

        for (int i = 0; i < numberOfRows; i++)
        {
            for (int j = 0; j < numberOfColumns; j++)
            {
                GameObject newCase = Instantiate(casePrefab, transform);

                float positionX = startX + j * (boardCase.GetSize().x + horizontalSpacing);
                float positionY = startY + i * (boardCase.GetSize().y + verticalSpacing);

                newCase.transform.position = new Vector2(positionX, positionY);

                // give to the case its position information
                newCase.GetComponent<BoardCase>().DefinePositionOnBoard(new Vector2Int(i, j));
                boardArray[i, j] = newCase;
            }
        }
    }

    // debug
    public void LogBoardArray()
    {
        if (boardArray == null) {
            Debug.LogWarning("The board array is null.");
            return;
        }

        int numRows = boardArray.GetLength(0);
        int numCols = boardArray.GetLength(1);

        for (int i = 0; i < numRows; i++)
        {
            string rowContent = "";
            for (int j = 0; j < numCols; j++)
            {
                if (boardArray[i, j] != null && boardArray[i, j].transform.childCount > 0)
                {
                    rowContent += "X"; // display "O" if the case is occupied
                }
                else
                {
                    rowContent += "O"; // display "X" if the case is empty
                }

                if (j < numCols - 1)
                {
                    rowContent += ", ";
                }
            }
            Debug.Log(i + ": " + rowContent);
        }
    }


    /// <summary>
    /// Places the selected GameObject piece at a specific position on the board.
    /// </summary>
    /// <param name="position">The position on the board where to place the piece (row, column). Indices start from 0.</param>
    /// <remarks> ex: "PlacePiece(piecePrefab, new Vector2Int(0, 2));"</remarks>
    public void PlacePiece(Vector2Int position)
    {
        int row = position.x;
        int col = position.y;

        // Instantiate a piece on a specified case
        boardArray[row, col].GetComponent<BoardCase>().PlacePiece(selectedPiece);
        selectedPiece = null;
        UpdateTilesClickability(null);
    }


    /// <summary>
    /// Removes a the piece at a specific position on the board.
    /// </summary>
    /// <param name="position">The position on the board where to remove the piece (row, column). Indices start from 0.</param>
    /// <remarks> ex: "RemovePiece(new Vector2Int(0, 2));"</remarks>
    public void RemovePiece(Vector2Int position)
    {
        int row = position.x;
        int col = position.y;

        // destroy a piece on a specified case
        boardArray[row, col].GetComponent<BoardCase>().RemovePiece();
    }

    private void OnEnable()
    {
        BoardCase.caseClicked += OnCaseClicked;
        MoveManager.possibilities += UpdateTilesClickability;
    }

    private void OnDisable()
    {
        BoardCase.caseClicked -= OnCaseClicked;
        MoveManager.possibilities -= UpdateTilesClickability;
    }


    public delegate void transferPionToMovementManager(SOPiece pion, Vector2Int position, GameObject[,] boardArray);
    public static event transferPionToMovementManager transferPion;

    public void OnCaseClicked(Vector2Int clickedPosition)
    {
        GameObject piece = GetPieceAtPosition(clickedPosition);
        // got a piece on it
        if (piece)
        {
            Debug.Log("piece : " + piece + "position : " + clickedPosition);
            selectedPiece = piece;
            selectedPiecePosition = clickedPosition;

            SOPiece soPiece = selectedPiece.GetComponent<Piece>().soPiece;
            transferPion(soPiece, clickedPosition, boardArray);
        }
        else
        {
            if (!selectedPiece)
            {
                Debug.LogError("Selected piece is empty");
                return;
            }
            RemovePiece(selectedPiecePosition);
            PlacePiece(clickedPosition);
        }
    }

    private GameObject GetPieceAtPosition(Vector2Int position)
    {
        int row = position.x;
        int col = position.y;

        if (row >= 0 && row < numberOfRows && col >= 0 && col < numberOfColumns)
        {
            if (boardArray[row, col] != null && boardArray[row, col].transform.childCount > 0)
            {
                // return the piece GameObject if it exists in the specified position
                return boardArray[row, col].transform.GetChild(0).gameObject;
            }
        }

        // no piece found
        return null;
    }

    public void UpdateTilesClickability(List<Vector2Int> possibleMoves)
    {
        if (possibleMoves != null) Debug.Log("nb of possible moves : " + possibleMoves.Count);

        // Go through the board
        for (int i = 0; i < numberOfRows; i++)
        {
            for (int j = 0; j < numberOfColumns; j++)
            {
                // Get the piece info
                Vector2Int position = new Vector2Int(i, j);
                GameObject currentCase = boardArray[position.x, position.y];
                bool isEmpty = GetPieceAtPosition(position) == null;

                // No selected piece - nothing done yet
                if (!selectedPiece)
                {
                    currentCase.GetComponent<BoardCase>().isClickable = !isEmpty;
                }
                // Already selected the piece to move
                else
                {
                    if (possibleMoves != null)
                    {
                        bool isClickable = possibleMoves.Contains(position);
                        currentCase.GetComponent<BoardCase>().isClickable = isClickable;
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }
    }


}

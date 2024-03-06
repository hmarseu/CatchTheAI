using catchTheAI;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.TextCore.Text;
using UnityEngine;
using UnityEngine.UIElements;

public class BoardManager : MonoBehaviour
{
    [SerializeField] private GameObject casePrefab;
    [SerializeField] private CemeteryManager cemeteryManager;

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
        if (boardArray == null)
        {
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

        // check if there is already a piece
        HandleEatingPiece(position);

        // place the selected piece on the new case
        boardArray[row, col].GetComponent<BoardCase>().InstantiatePiece(selectedPiece);
        selectedPiece = null;

        StartCoroutine(DelayedUpdateTilesClickability());
    }

    public void MovePiece(Vector2Int position)
    {
        int row = position.x;
        int col = position.y;

        // check if there is already a piece
        HandleEatingPiece(position);

        // place the selected piece on the new case
        boardArray[row, col].GetComponent<BoardCase>().MovePiece(selectedPiece);
        selectedPiece = null;
        // TODO remove btn of the UI if parachutage = true (?)

        StartCoroutine(DelayedUpdateTilesClickability());
    }

    // had to because its faster than to destroy the child
    private IEnumerator DelayedUpdateTilesClickability()
    {
        yield return new WaitForSeconds(0.1f);
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
        CemeteryButton.selectPiece += PrepareParachuting;
        MoveManager.possibilities += UpdateTilesClickability;
    }

    private void OnDisable()
    {
        BoardCase.caseClicked -= OnCaseClicked;
        CemeteryButton.selectPiece -= PrepareParachuting;
        MoveManager.possibilities -= UpdateTilesClickability;
    }

    public void PrepareParachuting(GameObject piece)
    {
        selectedPiece = piece;
        UpdateTilesClickability(null, true);
    }

    public delegate void transferPionToMovementManager(SOPiece pion, Vector2Int position, GameObject[,] boardArray);
    public static event transferPionToMovementManager transferPion;

    public void OnCaseClicked(Vector2Int clickedPosition)
    {
        if(selectedPiece)
        {
            RemovePiece(selectedPiecePosition);
            MovePiece(clickedPosition);
        }
        else
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
                // Debug.LogWarning("position : " + position + " nb of child : " + boardArray[row, col].transform.childCount);
                // return the piece GameObject if it exists in the specified position
                return boardArray[row, col].transform.GetChild(0).gameObject;

            }
        }

        // no piece found
        return null;
    }

    public void UpdateTilesClickability(List<Vector2Int> possibleMoves = null, bool parachuting = false)
    {
        // Go through the board
        for (int i = 0; i < numberOfRows; i++)
        {
            for (int j = 0; j < numberOfColumns; j++)
            {
                // Get the piece info
                Vector2Int position = new Vector2Int(i, j);
                GameObject currentCase = boardArray[position.x, position.y];
                bool isEmpty = GetPieceAtPosition(position) == null;

                // If there's no selected piece
                if (!selectedPiece)
                {
                    currentCase.GetComponent<BoardCase>().isClickable = !isEmpty;
                }
                // If we are in a parachuting scenario and the piece is empty
                else if (parachuting && isEmpty)
                {
                    currentCase.GetComponent<BoardCase>().isClickable = true;
                }
                // If there are possible moves and the case is in the list of possible moves
                else if (possibleMoves != null && possibleMoves.Contains(position))
                {
                    currentCase.GetComponent<BoardCase>().isClickable = true;
                }
                // Otherwise, the case is not clickable
                else
                {
                    currentCase.GetComponent<BoardCase>().isClickable = false;
                }
            }
        }
    }

    private void HandleEatingPiece(Vector2Int newPosition)
    {
        GameObject pieceToEat = GetPieceAtPosition(newPosition);
        if (pieceToEat != null)
        {
            // Ajouter la pièce dans le cimetière
            int playerId = pieceToEat.GetComponent<Piece>().idPlayer;
            cemeteryManager.AddToCemetery(pieceToEat, playerId);
        }
    }


}

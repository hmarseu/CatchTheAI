using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.TextCore.Text;
using UnityEngine;
using UnityEngine.UIElements;
using YokaiNoMori.Enumeration;
using YokaiNoMori.Interface;

public class BoardManager : MonoBehaviour, IGameManager
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

    public Player player1;
    public Player player2;

    public Player currentPlayerTurn;

    private void SetPlayerPiece()
    {
        //boardArray[0,0].transform.GetChild(0).GetComponent<Piece>().player = player1;
        //Piece[] pieces = GameObject.FindObjectsOfType<Piece>();
        //for (int i = 0; i < pieces.Length; i++)
        //{
        //    if (i < 4)
        //    {
        //        pieces[i].player = player1;
        //    }
        //    else
        //    {
        //        pieces[i].player = player2;
        //    }
        //}

    }
    private void Awake()
    {
        boardCase = casePrefab.GetComponent<BoardCase>();
   
    }

    private void Start()
    {
        CreationOfPlayer();
        GenerateBoard();
        StartFillArray();
        UpdateTilesClickability(null);
        LogBoardArray();
    }
    private void CreationOfPlayer()
    {
        ECampType pl1 = ECampType.PLAYER_ONE;
        ECampType pl2 = ECampType.PLAYER_TWO;

        player1 = GameObject.Find("Player1").GetComponent<Player>();
        player2 = GameObject.Find("Player2").GetComponent<Player>();
        player1.SetCamp(pl1);
        player1.SetName("joueur 1");
        player2.SetCamp(pl2);
        player2.SetName("joueur 2");

        currentPlayerTurn = player1;
        Debug.Log($"player 1 {player1.GetName()} player 2 {player2.GetName()} et current player {currentPlayerTurn.GetName()}");
        //player1.StartTurn();

        SetPlayerPiece();
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
            //Debug.Log(i + ": " + rowContent);
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

        StartCoroutine(DelayedUpdateTilesClickability());

    }

    // had to because its faster than the destroy child
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
        MoveManager.possibilities += UpdateTilesClickability;
    }

    private void OnDisable()
    {
        BoardCase.caseClicked -= OnCaseClicked;
        MoveManager.possibilities -= UpdateTilesClickability;
    }

    //move manager
    public delegate void transferPionToMovementManager(SOPiece pion, Vector2Int position, GameObject[,] boardArray,Player player);
    public static event transferPionToMovementManager transferPion;

    public void OnCaseClicked(Vector2Int clickedPosition)
    {
        
        GameObject piece = GetPieceAtPosition(clickedPosition);
        // got a piece on it
        if (piece)
        {
            //Debug.Log("piece : " + piece + "position : " + clickedPosition);
            selectedPiece = piece;
            selectedPiecePosition = clickedPosition;

            SOPiece soPiece = selectedPiece.GetComponent<Piece>().soPiece;
            transferPion(soPiece, clickedPosition, boardArray,currentPlayerTurn);
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
            ChangeTurn();
        }
    }
    private void ChangeTurn()
    {
        Debug.Log(currentPlayerTurn.GetName());
        if (currentPlayerTurn== player1)
        {
            currentPlayerTurn = player2;
        }
        else
        {
            currentPlayerTurn = player1;
        }
        UpdateTilesClickability();
    }
    private GameObject GetPieceAtPosition(Vector2Int position)
    {
        int row = position.x;
        int col = position.y;

        if (row >= 0 && row < numberOfRows && col >= 0 && col < numberOfColumns)
        {
            if (boardArray[row, col] != null && boardArray[row, col].transform.childCount > 0)
            {
                Debug.LogWarning("position : " + position + " nb of child : " + boardArray[row, col].transform.childCount);
                // return the piece GameObject if it exists in the specified position
                return boardArray[row, col].transform.GetChild(0).gameObject;

            }
        }

        // no piece found
        return null;
    }

    public void UpdateTilesClickability(List<Vector2Int> possibleMoves = null)
    {
       
        // si la piece appartient au joueur dont c'est le tour clickable sinon non clickable

        // Go through the board
        for (int i = 0; i < numberOfRows; i++)
        {
            for (int j = 0; j < numberOfColumns; j++)
            {
                // Get the piece info
                Vector2Int position = new Vector2Int(i, j);
                GameObject currentCase = boardArray[position.x, position.y];
                BoardCase boardCase = currentCase.GetComponent<BoardCase>();
                GameObject pieceAtPosition = GetPieceAtPosition(position);
                if (pieceAtPosition!=null)
                {
                   
                    if (pieceAtPosition.GetComponent<Piece>().player == currentPlayerTurn && !selectedPiece)
                    {
                        boardCase.isClickable = true;
                    }
                    else
                    {
                        boardCase.isClickable = false;
                    }
                }
                else if(!selectedPiece)
                {
                    if (possibleMoves != null)
                    {
                        bool isClickable = possibleMoves.Contains(position);
                        currentCase.GetComponent<BoardCase>().isClickable = isClickable;
                    }
                    else
                    {
                        currentCase.GetComponent<BoardCase>().isClickable = false;
                        return;
                    }
                }
            }
        }
    }

    public List<IPawn> GetAllPawn()
    {
        throw new NotImplementedException();
    }

    public List<IBoardCase> GetAllBoardCase()
    {
        throw new NotImplementedException();
    }

    public void DoAction(IPawn pawnTarget, Vector2Int position, EActionType actionType)
    {
        throw new NotImplementedException();
    }
}

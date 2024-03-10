using catchTheAI;
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
    [SerializeField] private CemeteryManager cemeteryManager;

    [SerializeField] private int numberOfRows;
    [SerializeField] private int numberOfColumns;
    [SerializeField] private float horizontalSpacing;
    [SerializeField] private float verticalSpacing;
    [SerializeField] private List<GameObject> pionDictionary;

    private bool isParachuting;

    private GameObject selectedPiece;
    private Vector2Int selectedPiecePosition;

    private BoardCase boardCase;
    private GameObject[,] boardArray; // Array that represent the board

    public Player player1;
    public Player player2;

    public Player currentPlayerTurn;

    private void SetPlayerPiece()
    {
        for (int i = 0; i < numberOfRows; i++)
        {
            for (int j = 0; j < numberOfColumns; j++)
            {
                if (boardArray[i, j].transform.childCount > 0)
                {
                    if (i < 2)
                    {

                        boardArray[i, j].transform.GetChild(0).GetComponent<Piece>().player = player1;
                        boardArray[i, j].transform.GetChild(0).GetComponent<Piece>().idPlayer = 1;
                    }
                    else
                    {

                        boardArray[i, j].transform.GetChild(0).GetComponent<Piece>().player = player2;
                        boardArray[i, j].transform.GetChild(0).GetComponent<Piece>().idPlayer = 2;
                    }
                }
            }
        }
    }

    private void Awake()
    {
        boardCase = casePrefab.GetComponent<BoardCase>();

    }

    private void Start()
    {
        GenerateBoard();
        StartFillArray();
        LogBoardArray();
        CreationOfPlayer();
        UpdateTilesAtTurnChange();
    }
    private void CreationOfPlayer()
    {
        ECampType pl1 = ECampType.PLAYER_ONE;
        ECampType pl2 = ECampType.PLAYER_TWO;

        player1.SetCamp(pl1);
        player1.SetName("joueur 1");
        player2.SetCamp(pl2);
        player2.SetName("joueur 2");

        currentPlayerTurn = player1;
        //Debug.Log($"player 1 {player1.GetName()} player 2 {player2.GetName()} et current player {currentPlayerTurn.GetName()}");
        //player1.StartTurn();

        SetPlayerPiece();
    }
    private void StartFillArray()
    {
        // player 1
        selectedPiece = pionDictionary[0];
        PlacePiece(new Vector2Int(0, 0), 1);
        selectedPiece = pionDictionary[1];
        PlacePiece(new Vector2Int(0, 1), 1);
        selectedPiece = pionDictionary[2];
        PlacePiece(new Vector2Int(0, 2), 1);
        selectedPiece = pionDictionary[3];
        PlacePiece(new Vector2Int(1, 1), 1);

        // player 2
        selectedPiece = pionDictionary[0];
        PlacePiece(new Vector2Int(3, 0), 2);
        selectedPiece = pionDictionary[1];
        PlacePiece(new Vector2Int(3, 1), 2);
        selectedPiece = pionDictionary[2];
        PlacePiece(new Vector2Int(3, 2), 2);
        selectedPiece = pionDictionary[3];
        PlacePiece(new Vector2Int(2, 1), 2);
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


    // Create the selected GameObject piece at a specific position on the board.
    public void PlacePiece(Vector2Int position, int player = 1)
    {
        int row = position.x;
        int col = position.y;

        // instantiate the selected piece on the new case
        if (player != 1) boardArray[row, col].GetComponent<BoardCase>().InstantiatePiece(selectedPiece, 180f);
        else boardArray[row, col].GetComponent<BoardCase>().InstantiatePiece(selectedPiece);

        selectedPiece = null;
        selectedPiecePosition = Vector2Int.zero;

        //StartCoroutine(DelayedUpdateTilesClickability());
        UpdateTilesClickability();
    }

    public delegate void CemeteryRemove(GameObject selectedPiece);
    public static event CemeteryRemove removeButtonCemetary;

    // to move a piece to a new position
    public void MovePiece(Vector2Int position, int player = 1)
    {
        int row = position.x;
        int col = position.y;

        if (isParachuting)
        {
            removeButtonCemetary(selectedPiece);
        }

        // check if there is already a piece
        HandleEatingPiece(position);

        // move the selected piece on the new case
        if (player != 1) boardArray[row, col].GetComponent<BoardCase>().MovePiece(selectedPiece, 180f);
        else boardArray[row, col].GetComponent<BoardCase>().MovePiece(selectedPiece);

        selectedPiece = null;
        selectedPiecePosition = Vector2Int.zero;
        //ChangeTurn();
    }

    // Removes a the piece at a specific position on the board.
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

    public delegate void transferPionToMovementManager(SOPiece pion, Vector2Int position, GameObject[,] boardArray, Player player);
    public static event transferPionToMovementManager transferPion;

    public void OnCaseClicked(Vector2Int clickedPosition)
    {
        // got a piece on it
        if (selectedPiece)
        {
            if (selectedPiecePosition == clickedPosition && !isParachuting)
            {
                selectedPiece = null;
                selectedPiecePosition = Vector2Int.zero;
                UpdateTilesAtTurnChange();
                return;
            }

            if (currentPlayerTurn == player2) MovePiece(clickedPosition, 2);
            else MovePiece(clickedPosition, 1);
            ChangeTurn();
        }
        else
        {
            GameObject piece = GetPieceAtPosition(clickedPosition);
            selectedPiecePosition = clickedPosition;
            // got a piece on it
            if (piece)
            {
                //Debug.Log("piece : " + piece + "position : " + clickedPosition);
                selectedPiece = piece;
                selectedPiecePosition = clickedPosition;

                SOPiece soPiece = selectedPiece.GetComponent<Piece>().soPiece;
                transferPion(soPiece, clickedPosition, boardArray, currentPlayerTurn);
            }
        }
    }

    private void ChangeTurn()
    {
        bool player1IsPlaying;
        if (currentPlayerTurn == player1)
        {
            currentPlayerTurn = player2;
            player1IsPlaying = false;
        }
        else
        {
            currentPlayerTurn = player1;
            player1IsPlaying = true;
        }
        // Debug.Log(currentPlayerTurn.GetName());
        cemeteryManager.SetCemeteryButtonsInteractability(player1IsPlaying);
        UpdateTilesAtTurnChange();
    }

    private GameObject GetPieceAtPosition(Vector2Int position)
    {
        int row = position.x;
        int col = position.y;

        if (row >= 0 && row < numberOfRows && col >= 0 && col < numberOfColumns)
        {
            if (boardArray[row, col] != null && boardArray[row, col].transform.childCount > 0)
            {
                return boardArray[row, col].transform.GetChild(0).gameObject;
            }
        }

        // no piece found
        return null;
    }
    public void UpdateTilesAtTurnChange()
    {
        for (int i = 0; i < numberOfRows; i++)
        {
            for (int j = 0; j < numberOfColumns; j++)
            {
                Vector2Int position = new Vector2Int(i, j);
                GameObject currentCase = boardArray[position.x, position.y];
                GameObject pieceAtPosition = GetPieceAtPosition(position);

                if (pieceAtPosition != null)
                {
                    if (pieceAtPosition.GetComponent<Piece>().player == currentPlayerTurn)
                    {
                        currentCase.GetComponent<BoardCase>().isClickable = true;
                    }
                    else
                    {
                        currentCase.GetComponent<BoardCase>().isClickable = false;
                    }
                }
                else currentCase.GetComponent<BoardCase>().isClickable = false;
            }
        }
    }
    public void UpdateTilesClickability(List<Vector2Int> possibleMoves = null, bool parachuting = false)
    {
        isParachuting = parachuting;

        // Go through the board
        for (int i = 0; i < numberOfRows; i++)
        {
            for (int j = 0; j < numberOfColumns; j++)
            {
                // Get the piece info
                Vector2Int position = new Vector2Int(i, j);
                GameObject currentCase = boardArray[position.x, position.y];

                GameObject pieceAtPosition = GetPieceAtPosition(position);
                bool isEmpty = pieceAtPosition == null;

                // If we are in a parachuting scenario and the piece is empty
                if (parachuting && isEmpty)
                {
                    currentCase.GetComponent<BoardCase>().isClickable = true;
                }
                // If there are possible moves and the case is in the list of possible moves
                else if (possibleMoves != null && possibleMoves.Contains(position))
                {
                    currentCase.GetComponent<BoardCase>().isClickable = true;
                }
                // If its the same case
                else if (selectedPiece != null && pieceAtPosition == selectedPiece)
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
    private void HandleEatingPiece(Vector2Int newPosition)
    {
        GameObject pieceToEat = GetPieceAtPosition(newPosition);
        if (pieceToEat != null)
        {
            // add the piece in the cemetery
            int playerId = selectedPiece.GetComponent<Piece>().idPlayer; // id of the player who is gonna eat
            cemeteryManager.AddToCemetery(pieceToEat, playerId);
            ChangePieceTeam(pieceToEat);
        }
    }
    public void ChangePieceTeam(GameObject piece)
    {
        Piece pieceComponent = piece.GetComponent<Piece>();
        if (pieceComponent != null)
        {
            // change the team of the piece
            pieceComponent.player = currentPlayerTurn;
            pieceComponent.idPlayer = (short)(currentPlayerTurn == player1 ? 1 : 2);
        }
    }
}
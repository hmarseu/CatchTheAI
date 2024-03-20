using CartoonFX;
using catchTheAI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using YokaiNoMori.Enumeration;
using YokaiNoMori.Interface;

public class BoardManager : MonoBehaviour, IGameManager
{
    private int[,] pieceIds; // Array with all the position and the id of the pieces on the board

    int turnLeft = 0;
    bool sursis = false;
    Player POnSursis;

    [Header("Board Properties")]
    [SerializeField] private GameObject casePrefab;
    [SerializeField] private int numberOfRows;
    [SerializeField] private int numberOfColumns;
    [SerializeField] private float horizontalSpacing;
    [SerializeField] private float verticalSpacing;
    [SerializeField] private CemeteryManager cemeteryManager;
    [SerializeField] private List<GameObject> pionDictionary;
    private GameObject[,] boardArray; // Array that represent the board
    private BoardCase boardCase;

    [Header("Gameplay")]
    [SerializeField] SOPiece kodamaSamurai;
    [SerializeField] SOPiece kodama;
    private GameObject selectedPiece;
    private Vector2Int selectedPiecePosition;
    public Player player1;
    public Player player2;
    public Player currentPlayerTurn;
    private bool isParachuting;
    private bool isGameEnd = false;

    [Header("Player Moves")]
    [SerializeField] private int maxMovesUntilDraw = 6;
    private Dictionary<int, List<KeyValuePair<EPawnType, Vector2Int>>> playerMoves = new Dictionary<int, List<KeyValuePair<EPawnType, Vector2Int>>>();

    [Header("Visual Effects")]
    [SerializeField] private VFX_Manager _vfxManager;
    [SerializeField] private SFXManager _sfxManager;
    [SerializeField] private GameObject EndMenu;
    [SerializeField] TextMeshProUGUI winnerText;

    // Events
    public delegate void checkCanEatKoropokkuru(Player player, List<SOPiece> listPion, Vector2Int positionKor, List<Vector2Int> listPosition);
    public static event checkCanEatKoropokkuru CheckEatKor;

    public int[,] GetBoardWithIds()
    {
        return pieceIds;
    }

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
                        boardArray[i, j].transform.GetChild(0).GetComponent<Piece>().idPlayer = -1;
                    }
                }
            }
        }
    }

    private void PromotionZoneKoropokkuru(Vector2Int positionKor,Player player)
    {
        // check if the pieces can eat the king
        // get all pieces position in a list -> movemanager with the event
        List<SOPiece> listSo = new();
        List<Vector2Int> listPosition= new();
        int radius = 1;

        int startX = Mathf.Max(0, positionKor.x - radius);
        int endX = Mathf.Min(boardArray.GetLength(0) - 1, positionKor.x + radius);

        int startY = Mathf.Max(0, positionKor.y - radius);
        int endY = Mathf.Min(boardArray.GetLength(1) - 1, positionKor.y + radius);

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                GameObject obj = boardArray[x, y];
                if (obj!=null && obj.transform.childCount > 0)
                {
                    Piece piece = obj.GetComponentInChildren<Piece>();
                    if (piece.player != player) 
                    {
                        listSo.Add(piece.soPiece);
                        listPosition.Add(new Vector2Int(x, y));
                    }

                }
            }
        }

        CheckEatKor(player, listSo, positionKor, listPosition);
    }

    private void Awake()
    {
        boardCase = casePrefab.GetComponent<BoardCase>();
        _sfxManager = GameObject.Find("SFXManager").GetComponent<SFXManager>();
    }

    private void Start()
    {
        currentPlayerTurn = UnityEngine.Random.Range(1, 3) == 1 ? player1 : player2;

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

        SetPlayerPiece();
    }

    private void StartFillArray()
    {
        // player 1
        selectedPiece = pionDictionary[0];
        PlacePiece(new Vector2Int(0, 2), 1);
        selectedPiece = pionDictionary[1];
        PlacePiece(new Vector2Int(0, 1), 1);
        selectedPiece = pionDictionary[2];
        PlacePiece(new Vector2Int(0, 0), 1);
        selectedPiece = pionDictionary[3];
        PlacePiece(new Vector2Int(1, 1), 1);
        
        // player 2
        selectedPiece = pionDictionary[0];
        PlacePiece(new Vector2Int(3, 0), -1);
        selectedPiece = pionDictionary[1];
        PlacePiece(new Vector2Int(3, 1), -1);
        selectedPiece = pionDictionary[2];
        PlacePiece(new Vector2Int(3, 2), -1);
        selectedPiece = pionDictionary[3];
        PlacePiece(new Vector2Int(2, 1), -1);
    }
    private void GenerateBoard()
    {
        _sfxManager?.PlaySoundEffect(0);

        boardArray = new GameObject[numberOfRows, numberOfColumns];
        pieceIds = new int[numberOfRows, numberOfColumns];

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


                pieceIds[i, j] = 0;
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
        }
    }
    
    // debug
    public void LogPieceIds()
    {
        if (pieceIds == null)
        {
            Debug.LogWarning("The pieceIds array is null.");
            return;
        }

        int numRows = pieceIds.GetLength(0);
        int numCols = pieceIds.GetLength(1);

        for (int i = numRows - 1; i >= 0; i--)
        {
            string rowContent = "";
            for (int j = 0; j < numCols; j++)
            {
                rowContent += pieceIds[i, j].ToString();

                if (j < numCols - 1)
                {
                    rowContent += ", ";
                }
            }
            Debug.Log(rowContent);
        }
    }




    // Create the selected GameObject piece at a specific position on the board.
    public void PlacePiece(Vector2Int position, int player = 1)
    {
        int row = position.x;
        int col = position.y;

        
        int playerId = (player == 1) ? 1 : -1;
        pieceIds[row, col] = playerId * selectedPiece.GetComponent<Piece>().idPiece;

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

    public void GetSursis(Player player)
    {
        sursis = true;
        if (POnSursis == null)
        {
            POnSursis = player;

        }
    }

    public void TransformationKodamaOrWinKoro(GameObject pieceToTransform,Vector2Int pos,bool eaten)
    {
        Piece piece = pieceToTransform.GetComponent<Piece>();
        if (piece.soPiece.ePawnType == EPawnType.Koropokkuru)
        {
            PromotionZoneKoropokkuru(pos, currentPlayerTurn);
        }
        else if (!eaten && piece.soPiece.ePawnType == EPawnType.Kodama)
        {
            piece.soPiece = kodamaSamurai;
            piece.idPiece = 2 * piece.idPlayer;
            piece.remakeSprite();

        }
         else if(eaten && piece.soPiece.ePawnType == EPawnType.KodamaSamurai)
         {
            piece.soPiece = kodama;
            piece.idPiece = 1 * piece.idPlayer;
            piece.remakeSprite();
         }
    }

    // to move a piece to a new position
    public void MovePiece(Vector2Int position, int player = 1)
    {
        int row = position.x;
        int col = position.y;

        if (isParachuting)
        {
            removeButtonCemetary(selectedPiece);
        }
        else
        {
            // previous position is now empty
            pieceIds[selectedPiecePosition.x, selectedPiecePosition.y] = 0;
            
            if ((row == 0 && player == -1)|| row == 3 && player == 1)
            {
                // function is doing the verification of the piece type
                TransformationKodamaOrWinKoro(selectedPiece, new Vector2Int(row,col), false);

            }
        }

        EPawnType pawnType = selectedPiece.GetComponent<Piece>().GetPawnType();
        CheckPlayerHistory(position, player, pawnType);

        // check if there is already a piece
        HandleEatingPiece(position);

        // move the selected piece on the new case
        if (player != 1) boardArray[row, col].GetComponent<BoardCase>().MovePiece(selectedPiece, 180f);
        else boardArray[row, col].GetComponent<BoardCase>().MovePiece(selectedPiece);

        // fill new position
        Piece piece = selectedPiece.GetComponent<Piece>();
        pieceIds[row, col] = piece.idPlayer * piece.idPiece;


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
        MoveManager.koroWin += End;
        MoveManager.sursis += GetSursis;
    }

    private void OnDisable()
    {
        BoardCase.caseClicked -= OnCaseClicked;
        CemeteryButton.selectPiece -= PrepareParachuting;
        MoveManager.possibilities -= UpdateTilesClickability;
        MoveManager.koroWin -= End;
    }

    public void PrepareParachuting(GameObject piece)
    {
        if (!isGameEnd)
        {
            selectedPiece = piece;
            PlaySFXSelected();
            UpdateTilesClickability(null, true);
        }
    }

    public delegate void transferPionToMovementManager(SOPiece pion, Vector2Int position, GameObject[,] boardArray, Player player);
    public static event transferPionToMovementManager transferPion;

    public void OnCaseClicked(Vector2Int clickedPosition)
    {
        if(!isGameEnd)
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
                
                if (currentPlayerTurn == player2) MovePiece(clickedPosition, -1);
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
                    PlaySFXSelected();
                    selectedPiece = piece;
                    selectedPiecePosition = clickedPosition;

                    SOPiece soPiece = selectedPiece.GetComponent<Piece>().soPiece;
                    transferPion(soPiece, clickedPosition, boardArray, currentPlayerTurn);
                }
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
        if (turnLeft == 1)
        {
            if (POnSursis == player1)
            {
                End(1);
            }
            else
            {
                End(-1);
            }
        }
        if (sursis) 
        {
            turnLeft++;
        }
       
        cemeteryManager.SetCemeteryButtonsInteractability(player1IsPlaying);
        UpdateTilesAtTurnChange();
        
        LogPieceIds();
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
            if (pieceToEat.GetComponent<Piece>().soPiece.ePawnType == EPawnType.Koropokkuru)
            {
                End(playerId);
                Debug.Log("LAA");
                return;
            }
            TransformationKodamaOrWinKoro(pieceToEat,newPosition,true);
            cemeteryManager.AddToCemetery(pieceToEat, playerId);
            ChangePieceTeam(pieceToEat);
        }
    }

    private void End(int player)
    {
        isGameEnd = true;
        Debug.Log(player);
        if (player == 1)
        {
            winnerText.text = player1.name;
        }
        else if (player == -1)
        {
            winnerText.text = player2.name;
        }
        else
        {
            winnerText.text = "No one!\n(draw)";
        }

        // to play visual firewoks
        _vfxManager.PlayAtIndex(6, new Vector3(0, -3.50f, 0));
        // to play end sound
        _sfxManager.PlaySoundEffect(4);
       
        StartCoroutine(ShowEndMenu(3));
    }

    public void ChangePieceTeam(GameObject piece)
    {
        Piece pieceComponent = piece.GetComponent<Piece>();
        if (pieceComponent != null)
        {
            // change the team of the piece
            pieceComponent.player = currentPlayerTurn;
            pieceComponent.idPlayer = (short)(currentPlayerTurn == player1 ? 1 : -1);
        }
    }

    public void PlaySFXSelected()
    {
        _sfxManager?.PlaySoundEffect(2);
    }

    IEnumerator ShowEndMenu(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        EndMenu.SetActive(true);
    }

    public void CheckPlayerHistory(Vector2Int newPosition, int playerId, EPawnType pawnType)
    {
        if (!playerMoves.ContainsKey(playerId))
        {
            playerMoves[playerId] = new List<KeyValuePair<EPawnType, Vector2Int>>();
        }

        List<KeyValuePair<EPawnType, Vector2Int>> moves = playerMoves[playerId];

        if (moves.Count > 0)
        {
            KeyValuePair<EPawnType, Vector2Int> lastMove = moves[moves.Count - 1];

            // CHECK IF SOMEONE MOVES A DIFFERENT PIECE
            if (lastMove.Key != pawnType)
            {
                // for each player -> clear the list
                foreach (var playerMovesList in playerMoves.Values)
                {
                    if (moves.Count > 0)
                    {
                        moves.RemoveRange(0, moves.Count);
                    }
                }
                // Debug.LogWarning("Both players' moves cleared due to piece type change.");
            }

            else
            {
                // CHECK IF SOMEONE MOVES THE PIECE ON A DIFFERENT POSITION
                if (!moves.Any(move => move.Value == newPosition && move.Key == pawnType) && moves.Count >= 3)
                {
                    // for each player -> clear the list
                    foreach (var kvp in playerMoves)
                    {
                        List<KeyValuePair<EPawnType, Vector2Int>> movesList = kvp.Value;
                        if (movesList.Count > 0)
                        {
                            movesList.RemoveRange(0, movesList.Count);
                        }
                    }
                    // Debug.LogWarning("Player " + playerId + "'s moves cleared due to new position after the third move.");
                }

            }
        }

        moves.Add(new KeyValuePair<EPawnType, Vector2Int>(pawnType, newPosition));

        // Does one of the players have 6 moves registered
        if (moves.Count >= maxMovesUntilDraw)
        {
            int otherPlayerId = playerId == 1 ? -1 : 1;

            // Check if both player did 6 same moves
            if (playerMoves.ContainsKey(otherPlayerId) && playerMoves[otherPlayerId].Count >= maxMovesUntilDraw)
            {
                // Draw
                
                End(0);
            }
        }
    }

}
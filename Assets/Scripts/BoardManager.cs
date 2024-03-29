using CartoonFX;
using catchTheAI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using YokaiNoMori.Enumeration;
using YokaiNoMori.Interface;
using YokaiNoMori.Struct;

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
    private bool isGameEnd = false;

    [Header("Player Moves")]
    [SerializeField] private int maxMovesUntilDraw = 6;
    private Dictionary<int, List<KeyValuePair<EPawnType, Vector2Int>>> playerMoves = new Dictionary<int, List<KeyValuePair<EPawnType, Vector2Int>>>();
    EActionType eActionType;
    private SAction lastAction;

    [Header("Visual Effects")]
    [Header("Visual Effects")]
    [SerializeField] private VFX_Manager _vfxManager;
    [SerializeField] private SFXManager _sfxManager;
    [SerializeField] private GameObject EndMenu;
    [SerializeField] TextMeshProUGUI winnerText;

    private void Awake()
    {
        boardCase = casePrefab.GetComponent<BoardCase>();
        _sfxManager = GameObject.Find("SFXManager").GetComponent<SFXManager>();
    }

    private void Start()
    {
        //temp
        //currentPlayerTurn = UnityEngine.Random.Range(1, 3) == 1 ? player1 : player2;
        currentPlayerTurn = player1;
        GenerateBoard();
        StartFillArray();
        SetPiecePositions(); // give to the pieces their position
        // LogPiecePositions();
        CreationOfPlayer();
        // LogBoardArray();
        // LogPieceIds();
        UpdateTilesAtTurnChange();
        StartGame();
    }

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

    private void StartGame()
    {
        currentPlayerTurn.StartTurn();
    }

    private void CreationOfPlayer()
    {
        ECampType pl1 = ECampType.PLAYER_ONE;
        ECampType pl2 = ECampType.PLAYER_TWO;

        player1.SetCamp(pl1);
        player1.SetName("joueur 1");
        player1.idPlayer = 1;
        player2.SetCamp(pl2);
        player2.SetName("joueur 2");
        player2.idPlayer = -1;
        player2.isAI = true;
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

    public void SetPiecePositions()
    {
        if (boardArray == null)
        {
            Debug.LogWarning("The board array is null.");
            return;
        }

        int numRows = numberOfRows;
        int numCols = numberOfColumns;

        for (int i = 0; i < numRows; i++)
        {
            for (int j = 0; j < numCols; j++)
            {
                GameObject pieceObject = GetPieceObjectAtPosition(new Vector2Int(i, j));
                if (pieceObject != null)
                {
                    Piece piece = pieceObject.GetComponent<Piece>();
                    piece.SetCurrentPosition(new Vector2Int(i, j));
                }
            }
        }
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


    private void PromotionZoneKoropokkuru(Vector2Int positionKor, Player player)
    {
        // check if the pieces can eat the king
        // get all pieces position in a list -> movemanager with the event
        List<SOPiece> listSo = new();
        List<Vector2Int> listPosition = new();
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
                if (obj != null && obj.transform.childCount > 0)
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

        if (eActionType == EActionType.PARACHUTE) removeButtonCemetary(selectedPiece);

        else
        {
            // previous position is now empty
            pieceIds[selectedPiecePosition.x, selectedPiecePosition.y] = 0;

            if ((row == 0 && player == -1) || row == 3 && player == 1)
            {
                // function is doing the verification of the piece type
                TransformationKodamaOrWinKoro(selectedPiece, new Vector2Int(row, col), false);
            }
        }

        EPawnType pawnType = selectedPiece.GetComponent<Piece>().GetPawnType();
        CheckPlayerHistory(position, player, pawnType);


        // check if there is already a piece
        GameObject pieceEaten = HandleEatingPiece(position);
        // get which one it is
        IPawn takedPawn = null;
        if (pieceEaten)
        {
            Piece pieceComponent = pieceEaten.GetComponent<Piece>();
            if (pieceComponent) takedPawn = pieceComponent as IPawn;
        }


        // move the selected piece on the new case
        if (player != 1) boardArray[row, col].GetComponent<BoardCase>().MovePiece(selectedPiece, 180f);
        else boardArray[row, col].GetComponent<BoardCase>().MovePiece(selectedPiece);

        // fill new position
        Piece piece = selectedPiece.GetComponent<Piece>();
        pieceIds[row, col] = piece.idPlayer * piece.idPiece;

        // keep last action info
        UpdateLastAction(currentPlayerTurn.GetCamp(), piece.soPiece.ePawnType, eActionType, piece.GetCurrentPosition(), position, takedPawn);

        // give the piece its position
        piece.SetCurrentPosition(position);

        /*
        Debug.LogError("MOVE");
        LogPieceIds();
        Debug.LogError("END MOVE");
        */

        selectedPiece = null;
        selectedPiecePosition = Vector2Int.zero;
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
            Debug.Log("game not ended");
            // got a piece on it
            if (selectedPiece)
            {
                Debug.Log("piece selected");
                // IA not supposed to go in
                if (selectedPiecePosition == clickedPosition && eActionType == EActionType.MOVE)
                {
                    selectedPiece = null;
                    selectedPiecePosition = Vector2Int.zero;
                    UpdateTilesAtTurnChange();
                    return;
                }

                Debug.Log("move piece");
                if (currentPlayerTurn == player2) MovePiece(clickedPosition, -1);
                else MovePiece(clickedPosition, 1);

                // ChangeTurn(); // logic before, not now with AI
                // stop the turn of the current player

                Debug.Log("stop turn");
                currentPlayerTurn.StopTurn();
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

    public void ChangeTurn()
    {
        // change who is playing
        bool player1IsPlaying;
        if (currentPlayerTurn == player1)
        {
            Debug.Log("passage de joueur 1 a 2 ");
            currentPlayerTurn = player2;
            player2.StartTurn();
            player1IsPlaying = false;
            
        }
        else
        {
            Debug.Log("passage de joueur 2 a 1 ");
            currentPlayerTurn = player1;
            player1.StartTurn();
            player1IsPlaying = true;
        }

        // check if koro is about to win by being on the last line
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

        // prepare turn for the player 
        if (currentPlayerTurn.isAI) cemeteryManager.SetCemeteryButtonsProhibit();
        else cemeteryManager.SetCemeteryButtonsInteractability(player1IsPlaying);
        
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

                if (pieceAtPosition != null && !currentPlayerTurn.isAI)
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
        if (parachuting) eActionType = EActionType.PARACHUTE;
        else eActionType = EActionType.MOVE;

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

    private GameObject HandleEatingPiece(Vector2Int newPosition)
    {
        GameObject pieceToEat = GetPieceAtPosition(newPosition);
        if (pieceToEat != null)
        {
            // add the piece to the cemetery
            int playerId = selectedPiece.GetComponent<Piece>().idPlayer; // ID of the player whos gonna eat
            if (pieceToEat.GetComponent<Piece>().soPiece.ePawnType == EPawnType.Koropokkuru)
            {
                End(playerId);
                return pieceToEat;
            }
            TransformationKodamaOrWinKoro(pieceToEat, newPosition, true);
            cemeteryManager.AddToCemetery(pieceToEat, playerId);
            ChangePieceTeam(pieceToEat);

            return pieceToEat;
        }
        return null;
    }

    private void End(int player)
    {
        isGameEnd = true;
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

    public List<IPawn> GetAllPawn()
    {
        throw new NotImplementedException();
    }

    public List<IBoardCase> GetAllBoardCase()
    {
        List<IBoardCase> allBoardCases = new List<IBoardCase>();

        for (int i = 0; i < numberOfRows; i++)
        {
            for (int j = 0; j < numberOfColumns; j++)
            {
                BoardCase boardCase = boardArray[i, j].GetComponent<BoardCase>();
                if (boardCase != null)
                {
                    allBoardCases.Add(boardCase);
                }
            }
        }
         
        return allBoardCases;
    }

    private GameObject GetPieceObjectAtPosition(Vector2Int position)
    {
        GameObject boardCase = boardArray[position.x, position.y];
        if (boardCase != null && boardCase.transform.childCount > 0)
        {
            return boardCase.transform.GetChild(0).gameObject;
        }
        return null;
    }

    public void LogPiecePositions()
    {
        if (boardArray == null)
        {
            Debug.LogWarning("The board array is null.");
            return;
        }

        int numRows = numberOfRows;
        int numCols = numberOfColumns;

        for (int i = numRows - 1; i >= 0; i--)
        {
            string rowContent = "";
            for (int j = 0; j < numCols; j++)
            {
                GameObject pieceObject = GetPieceObjectAtPosition(new Vector2Int(i, j));
                if (pieceObject != null)
                {
                    Piece piece = pieceObject.GetComponent<Piece>();
                    Vector2Int position = piece.GetCurrentPosition();
                    rowContent += "(" + position.x + ", " + position.y + ")";
                }
                else
                {
                    rowContent += "Empty";
                }

                if (j < numCols - 1)
                {
                    rowContent += ", ";
                }
            }
            Debug.Log(rowContent);
        }
    }

    public void DoAction(IPawn pawnTarget, Vector2Int position, EActionType actionType)
    {
        
        Vector2Int newPosition = ConvertToYohanArray(position);

        if (pawnTarget is Piece)
        {
            Piece piece = (Piece)pawnTarget;

            if (piece == null)
            {
                Debug.LogError("DoAction: Invalid IPawn provided.");
                return;
            }

            selectedPiece = piece.gameObject;
            eActionType = actionType;

            if (eActionType == EActionType.MOVE)
            {
                /*
                Debug.LogWarning("START SHOW POSITION");
                LogPiecePositions();
                Debug.LogWarning("END SHOW POSITION");
                */

                selectedPiecePosition = piece.GetCurrentPosition();
            }

            //move function
            Debug.LogError("MOVE");
            OnCaseClicked(ConvertToYohanArray(newPosition));
        }
        else
        {
            Debug.LogError("DoAction: IPawn is not of type Piece.");
            return;
        }
      
    }

    public Vector2Int ConvertToYohanArray(Vector2Int position)
    {
        int x = position.y;
        int y = position.x;
        return new Vector2Int(x, y);
    }

    public List<IPawn> GetReservePawnsByPlayer(ECampType campType)
    {
        int playerId = campType == ECampType.PLAYER_ONE ? 1 : -1;
        List<IPawn> reservePawns = new List<IPawn>();

        List<GameObject> deadPieces = cemeteryManager.GetDeadPiecesByPlayer(playerId);
        foreach (GameObject pieceObject in deadPieces)
        {
            IPawn pawn = pieceObject.GetComponent<IPawn>();
            if (pawn != null)
            {
                reservePawns.Add(pawn);
            }
            else
            {
                Debug.LogError("Piece object in dead zone does not have IPawn component.");
            }
        }

        return reservePawns;
    }

    public List<IPawn> GetPawnsOnBoard(ECampType campType)
    {
        List<IPawn> pawnsOnBoard = new List<IPawn>();
        for (int i = 0; i < numberOfRows; i++)
        {
            for (int j = 0; j < numberOfColumns; j++)
            {
                // has smth?
                GameObject pieceObject = GetPieceObjectAtPosition(new Vector2Int(i, j));
                if (pieceObject != null)
                {
                    Piece piece = pieceObject.GetComponent<Piece>();
                    // check if belong to the player we want
                    if (piece.player.GetCamp() == campType)
                    {
                        pawnsOnBoard.Add(piece);
                    }
                }
            }
        }

        return pawnsOnBoard;
    }

    public IPawn GetPieceById(int id)
    {
        for (int i = 0; i < pieceIds.GetLength(0); i++)
        {
            for (int k = 0; k < pieceIds.GetLength(1); k++)
            {
                
                if (id == pieceIds[i, k])
                {
                    // Debug.Log($"id : {id} id dans le tableau {pieceIds[i, k]}");
                    IPawn piece = boardArray[i, k].transform.GetChild(0).GetComponent<IPawn>();
                    // Debug.Log(piece + " " + i + " " + k);
                    return piece;
                }
            }
        }
        return null;
    }

    public SAction GetLastAction()
    {
        return lastAction;
    }

    // update last action done
    private void UpdateLastAction(ECampType campType, EPawnType pawnType, EActionType actionType, Vector2Int oldPosition, Vector2Int newPosition, IPawn takedPawn)
    {
        lastAction.CampType = campType;
        lastAction.PawnType = pawnType;
        lastAction.ActionType = actionType;
        lastAction.StartPosition = oldPosition;
        lastAction.NewPosition = newPosition;
        lastAction.TakedPawn = takedPawn;
    }

    private void DebugLastAction()
    {
        Debug.Log("Last Action Debug:");
        Debug.Log("Camp Type: " + lastAction.CampType);
        Debug.Log("Pawn Type: " + lastAction.PawnType);
        Debug.Log("Action Type: " + lastAction.ActionType);
        Debug.Log("Start Position: " + lastAction.StartPosition);
        Debug.Log("New Position: " + lastAction.NewPosition);
        if (lastAction.TakedPawn != null && lastAction.ActionType == EActionType.MOVE)
        {
            Debug.Log("Taked Pawn: " + lastAction.TakedPawn);
        }
    }

    SAction IGameManager.GetLastAction()
    {
        throw new NotImplementedException();
    }
}
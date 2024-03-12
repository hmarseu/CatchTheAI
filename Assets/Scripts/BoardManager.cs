using CartoonFX;
using catchTheAI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using YokaiNoMori.Enumeration;
using YokaiNoMori.Interface;

public class BoardManager : MonoBehaviour, IGameManager
{
    //event 
    public delegate void checkCanEatKoropokkuru(Player player,SOPiece pion, Vector2Int positionKor,Vector2Int[] positionPiece);
    public static event checkCanEatKoropokkuru CheckEatKor;

    [SerializeField] private GameObject casePrefab;
    [SerializeField] private CemeteryManager cemeteryManager;

    [SerializeField] private int numberOfRows;
    [SerializeField] private int numberOfColumns;
    [SerializeField] private float horizontalSpacing;
    [SerializeField] private float verticalSpacing;
    [SerializeField] private List<GameObject> pionDictionary;

    [SerializeField] private VFX_Manager _vfxManager;
    private SFXManager _sfxManager;
    [SerializeField] private GameObject EndMenu;

    private bool isParachuting;
    private bool isGameEnd = false;
    private GameObject selectedPiece;

    private Vector2Int selectedPiecePosition;

    private BoardCase boardCase;
    private GameObject[,] boardArray; // Array that represent the board

    public Player player1;
    public Player player2;

    public Player currentPlayerTurn;

    [SerializeField] SOPiece kodamaSamurai;
    [SerializeField] SOPiece kodama;

    [SerializeField] TextMeshProUGUI winnerText;

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
    private void PromotionZoneKoropokkuru(Vector2Int positionKor,int playerId)
    {
        // verification de toutes les pieces autour si ils peuvent manger le roi 
        // prends les positions des pieces ennemies presente sous forme de tableau -> movemanager avec l'event 

        /* static List<object> GetObjetsAutour(object[,] tableau, int x, int y)
    {
        List<object> objetsAutour = new List<object>();

        // V�rification de la pr�sence des cases autour de la case sp�cifi�e
        for (int i = Math.Max(0, x - 1); i <= Math.Min(tableau.GetLength(0) - 1, x + 1); i++)
        {
            for (int j = Math.Max(0, y - 1); j <= Math.Min(tableau.GetLength(1) - 1, y + 1); j++)
            {
                // V�rification si la case n'est pas la case sp�cifi�e
                if (i != x || j != y)
                {
                    // Ajout de l'objet dans la liste
                    objetsAutour.Add(tableau[i, j]);
                }
            }
        }

        return objetsAutour;
    }*/

    }
    private void Awake()
    {
        boardCase = casePrefab.GetComponent<BoardCase>();
        _sfxManager = GameObject.Find("SFXManager").GetComponent<SFXManager>();
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

        // select the starting player
        int playerToStart = UnityEngine.Random.Range(1, 3);
        Debug.Log(playerToStart);
        if(playerToStart == 1) currentPlayerTurn = player1;
        else currentPlayerTurn = player2;

        //Debug.Log($"player 1 {player1.GetName()} player 2 {player2.GetName()} et current player {currentPlayerTurn.GetName()}");
        //player1.StartTurn();

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
        _sfxManager?.PlaySoundEffect(0);

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


    public void TransformationKodama(GameObject pieceToTransform, bool eaten)
    {
        Piece piece = pieceToTransform.GetComponent<Piece>();
        if (!eaten && piece.soPiece.ePawnType == EPawnType.Kodama)
        {
            _vfxManager.PlayAtIndex(11, piece.transform.position);
            piece.soPiece = kodamaSamurai;
            piece.remakeSprite();

        }
         else if(eaten && piece.soPiece.ePawnType == EPawnType.KodamaSamurai)
         {
             piece.soPiece = kodama;
             piece.remakeSprite();
         }
    }
    // to move a piece to a new position
    public void MovePiece(Vector2Int position, int player = 1)
    {
        int row = position.x;
        int col = position.y;

        // check if there is already a piece
        HandleEatingPiece(position);

        // move the selected piece on the new case
        if (player != 1) boardArray[row, col].GetComponent<BoardCase>().MovePiece(selectedPiece, 180f);
        else boardArray[row, col].GetComponent<BoardCase>().MovePiece(selectedPiece);


        // check if the kodama should transform 
        if (isParachuting)
        {
            removeButtonCemetary(selectedPiece);
        }
        else
        {
            if (row == 0 || row == 3)
            {
                // la fonction fait elle meme la verification du type de piece
                TransformationKodama(selectedPiece, false);
            }
        }
        CheckPlayerHistory(selectedPiece.GetComponent<Piece>(), position, player);

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
                    PlaySFXSelected();
                    //Debug.Log("piece : " + piece + "position : " + clickedPosition);
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
            if (pieceToEat.GetComponent<Piece>().soPiece.ePawnType == EPawnType.Koropokkuru)
            {
                End(playerId);
                return;
            }
            TransformationKodama(pieceToEat,true);
            cemeteryManager.AddToCemetery(pieceToEat, playerId);
            ChangePieceTeam(pieceToEat);
        }
    }
    private void End(int player)
    {
        isGameEnd = true;
        // ON GAME END
        // to play visual firewoks
        if (player == 1)
        {
            winnerText.text = player1.name;
        }
        else 
        {
            winnerText.text = player2.name;
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
            pieceComponent.idPlayer = (short)(currentPlayerTurn == player1 ? 1 : 2);
        }
    }
    public void PlaySFXSelected()
    {
        _sfxManager?.PlaySoundEffect(2);
    }

    IEnumerator ShowEndMenu(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // Utiliser WaitForSecondsRealtime pour ignorer Time.timeScale
        EndMenu.SetActive(true);
    }



    private Dictionary<int, List<KeyValuePair<int, Vector2Int>>> playerMoves = new Dictionary<int, List<KeyValuePair<int, Vector2Int>>>();
    private int maxMoves = 6;

    // Ajouter le mouvement du joueur � la liste correspondante
    // TODO : switch "pieceId" pcq la cest une variable qui est jamais remplie enft
    public void CheckPlayerHistory(Piece piece, Vector2Int newPosition, int playerId)
    {
        if (!playerMoves.ContainsKey(playerId))
        {
            playerMoves[playerId] = new List<KeyValuePair<int, Vector2Int>>();
        }

        List<KeyValuePair<int, Vector2Int>> moves = playerMoves[playerId];
        int pieceId = piece.idPiece;

        if (moves.Count > 0)
        {
            KeyValuePair<int, Vector2Int> lastMove = moves[moves.Count - 1];

            Debug.Log("piece id = " + pieceId + " last id = " + lastMove.Key);
            // VERIFICATION SI L'UN D'EUX D�PLACE UNE PIECE AVEC UN ID DIFF�RENT
            if (lastMove.Key != pieceId)
            {
                foreach (var playerMovesList in playerMoves.Values)
                {
                    if (moves.Count > 1)
                    {
                        moves.RemoveRange(0, moves.Count - 1); // Garde uniquement le dernier mouvement
                    }
                }
                Debug.LogWarning("Both players' moves cleared due to piece ID change.");
            }

            else
            {
                // VERIFICATION SI L'UN D'EUX D�PLACE LA PI�CE SUR UNE NOUVELLE POSITION
                if (!moves.Any(move => move.Value == newPosition) && moves.Count >= 3)
                {
                    // Pour chaque joueur, conservez les deux derniers mouvements
                    foreach (var kvp in playerMoves)
                    {
                        List<KeyValuePair<int, Vector2Int>> movesList = kvp.Value;
                        if (movesList.Count > 2)
                        {
                            movesList.RemoveRange(0, movesList.Count - 2);
                        }
                    }
                    Debug.LogWarning("Player " + playerId + "'s moves cleared due to new position after the third move.");
                }

            }
        }

        moves.Add(new KeyValuePair<int, Vector2Int>(pieceId, newPosition));

        Debug.Log("Player " + playerId + " - Number of moves: " + moves.Count);

        // VERIFICATION SI L'UN D'EUX A AU MOINS 6 ELEMENTS DANS SA LISTE
        if (moves.Count >= maxMoves)
        {
            // R�cup�rez l'autre joueur
            int otherPlayerId = OtherPlayer(playerId);

            // V�rifiez si l'autre joueur a aussi au moins 6 mouvements
            if (playerMoves.ContainsKey(otherPlayerId) && playerMoves[otherPlayerId].Count >= maxMoves)
            {
                // D�clarez un match nul
                Debug.Log("Both players made a round trip with the same piece. Draw!");
                EndGame();
            }
        }
    }

    // M�thode pour r�cup�rer l'ID de l'autre joueur
    private int OtherPlayer(int playerId)
    {
        return playerId == 1 ? 2 : 1;
    }

    // M�thode appel�e � la fin du jeu
    private void EndGame()
    {
        Debug.Log("Game ended in a draw.");
    }




}
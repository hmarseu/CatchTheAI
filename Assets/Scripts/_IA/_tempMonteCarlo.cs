using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

//ia.MonteCarloSearch(new Node(idplayer, new Vector2Int(), null, null, pieceIds), 100);

public class Node
{
    public int playerid;
    public SOPiece Piece;
    public int[,] piecesPosition; 
    public Vector2Int move; 
    public int visits;
    public double wins;
    public List<Node> childNodes;
    public Node parent;

    public Node(int idplayer,Vector2Int move,SOPiece idPiece,Node parent,int[,] boardArray)
    {
        playerid = idplayer;
        this.Piece = idPiece;
        this.move = move;
        wins = 0;
        visits = 0;
        childNodes = new List<Node>();
        this.parent = parent;
        this.piecesPosition = boardArray;
    }

    public double ScoreValue()
    {
        const double explorationWeight = 2;
        if (visits==0)
        {
            return double.MaxValue;
        }
        return (double)wins/visits+explorationWeight + Math.Sqrt(2*Math.Log(parent.visits)/visits);
    }
}
public class _tempMonteCarlo : MonoBehaviour
{
    [SerializeField] BoardManager boardManager;
    public SOPiece Kodama;
    public SOPiece KodamaSamourai;
    public SOPiece Kitsune;
    public SOPiece Koropokkuru;
    public SOPiece Tanuki;
    int[,] boardTab;
    private Dictionary<int, SOPiece> pieceDataDictionnary = new();
    private void Start()
    {
      
        
    }
    private void OnEnable()
    {
        PopulateDictionnary();
    }
    private void PopulateDictionnary()
    {
        pieceDataDictionnary[-5] = Koropokkuru;
        pieceDataDictionnary[-4] = Kitsune;
        pieceDataDictionnary[-3] = Tanuki;
        pieceDataDictionnary[-2] = KodamaSamourai;
        pieceDataDictionnary[-1] = Kodama;
        pieceDataDictionnary[0] = null;
        pieceDataDictionnary[1] = Kodama;
        pieceDataDictionnary[2] = KodamaSamourai;
        pieceDataDictionnary[3] = Tanuki;
        pieceDataDictionnary[4] = Kitsune;
        pieceDataDictionnary[5] = Koropokkuru;
    }

    //public Vector2Int StartMonteCarloSearch(Node rootNode, int visits)
    //{
    //    Vector2Int bestMove = new Vector2Int();
    //    AutoResetEvent searchFinishedEvent = new AutoResetEvent(false);
    //    Thread searchThread = new Thread(() =>
    //    {
    //        bestMove = MonteCarloSearch(rootNode, visits);

    //        searchFinishedEvent.Set();
    //    });
    //    searchThread.Start();
    //    searchFinishedEvent.WaitOne();
    //    return bestMove;
    //}



        /// <summary>
        /// will turn the number of time we decide to make more precise moves
        /// </summary>
        /// <param name="rootNode"></param>
        /// <param name="visits"></param>
        /// <returns></returns>
        public Vector2Int MonteCarloSearch(Node rootNode,int visits)
        {
            if (boardManager != null)
            {

            boardTab = boardManager.GetBoardWithIds();
            for (int i = 0; i < boardTab.GetLength(0); i++)
            {
                for (int j = 0; j < boardTab.GetLength(1); j++)
                {
                    Debug.Log($"tableau[{i},{j}] = {boardTab[i, j]}");
                }
            }
            for (int i = 0; i < visits; i++)
                {
                    Node node = Selection(rootNode);
                    Expansion(node);
                    double result = Simulation(node);
                    BackPropagation(node, result);
                }
                //Node bestChild = rootNode.childNodes.OrderByDescending(child => child.visits).FirstOrDefault();
                Node bestChild = null;
                int maxVisits = int.MinValue;

                foreach (Node child in rootNode.childNodes)
                {
                    if (child.visits > maxVisits)
                    {
                        maxVisits = child.visits;
                        bestChild = child;
                    }
                }
            Debug.Log($"piece : {bestChild.Piece.name} meilleur coup : {bestChild.move} ");
            //for (int i = 0; i < bestChild.piecesPosition.GetLength(0); i++)
            //{
            //    for (int j = 0; j < bestChild.piecesPosition.GetLength(1); j++)
            //    {
            //        Debug.Log($"tableau[{i},{j}] = {bestChild.piecesPosition[i, j]}");
            //    }
            //}
            if (bestChild != null)
            {
                return bestChild.move;
            }
            }
            return new Vector2Int();
        
    }


    /// <summary>
    /// choose a node in wich we will start the exploration based on UCB 
    /// </summary>
    /// <param name="id"></param>
    private Node Selection(Node node)
    {
        Node currentNode = node;
        while (currentNode.childNodes.Count>0) 
        {
            //currentNode = currentNode.childNodes.OrderByDescending(child =>child.ScoreValue()).First();
            double maxScoreValue = double.MinValue;

            foreach (Node child in currentNode.childNodes)
            {
                double scoreValue = child.ScoreValue();
                if (scoreValue > maxScoreValue)
                {
                    maxScoreValue = scoreValue;
                    currentNode = child;
                }
            }
        }
        return currentNode;
    }
    /// <summary>
    /// after the expansion -> it chooses a child node to simulate the game
    /// mais pour l'instant win ne se passe jamais ou draw non plus
    /// et j'ai un soucis avec l'index random
    /// </summary>
    /// <returns></returns>
    private double Simulation(Node node)
    {
        int indexTest=0;

        int currentPlayer = node.playerid;
        int[,] currentBoard = (int[,])node.piecesPosition.Clone();
        while(indexTest < 1000)
        {
            List<Vector3Int> playerPieces = GetAllPiecesOfPlayer(node);
            List<Vector2Int> validMoves = new List<Vector2Int>();
            foreach(Vector3Int piece in playerPieces)
            {
                List<Vector2Int> piecesMoves = GetValidMoves(piece, currentPlayer);
                validMoves.AddRange(piecesMoves);
            }
            if(validMoves.Count == 0)
            {
                return 0;
            }
            Vector2Int randomMove = validMoves[UnityEngine.Random.Range(0, validMoves.Count-1)];
            
            int pieceValue = currentBoard[randomMove.x,randomMove.y];
            currentBoard = ReplaceInTab(currentBoard, pieceValue, randomMove);
            //currentBoard[randomMove.x,randomMove.y] = currentBoard[pieceValue,randomMove.x];
            
            if (IsWinner(currentBoard,currentPlayer))
            {
                return 1;
            }
            else if (IsDraw(currentBoard))
            {
                return 0;
            }
            //Debug.Log(indexTest);
            indexTest++;   
        }
        return 0;
    }
    /// <summary>
    /// after selection -> if the node haven t all the solutions it add node 
    /// </summary>
    /// <returns></returns>
    private void Expansion(Node node)
    {
        // we need to generate all the nodes based on every possible moves 
        List<Vector3Int> posidpiece = GetAllPiecesOfPlayer(node);
        
        foreach (Vector3Int piece in posidpiece)
        {
            List<Vector2Int> moves = GetValidMoves(piece, node.playerid);
            //Debug.Log($"nombre de coup a partir de cette node : {moves.Count}");
            foreach(Vector2Int move in moves)
            {
                //Debug.Log($"move : {move}");
                Node child = new Node(node.playerid,move, pieceDataDictionnary[piece.z], node, ReplaceInTab(node.piecesPosition,piece.z,move));
                node.childNodes.Add(child);
                child.parent = node;
                
            }
        }
        
    }
    /// <summary>
    /// after the simulation -> the result is propaged to the node chosen and all its parents 
    /// it updates the statistiques of each node (number of win and number of simulation)
    /// </summary>
    /// <returns></returns>
    private void BackPropagation(Node node, double result)
    {
        //Debug.Log("backpropagate");
        while (node != null)
        {
            node.visits++;
            node.wins += result;
            node = node.parent;
        }
    }

    //----------------- complementary funct ----------------------

    private List<Vector2Int> GetValidMoves(Vector3Int posplusidpiece, int player)
    {
        SOPiece so = pieceDataDictionnary[posplusidpiece.z];
        List<Vector2Int> validMoves = new List<Vector2Int>();
        int[] deltaX;
        int[] deltaY;

        deltaX = new int[] { -1, 0, 1, -1, 0, 1, -1, 0, 1 };
        deltaY = new int[] { -1, -1, -1, 0, 0, 0, 1, 1, 1 };
        // take into account the player ROTATION

        if (player != 1)
        {
            for (int i = 0; i < deltaX.Length; i++)
            {
                deltaX[i] *= -1;
                deltaY[i] *= -1;
            }
        }

        for (int i = 0; i < so.PossibleMoves.Count; i++)
        {
            if (so.PossibleMoves[i])
            {
                int newX = posplusidpiece.x + deltaX[i];
                int newY = posplusidpiece.y + deltaY[i];
                if (IsInsideBoard(newX, newY) && IsReachable(new Vector2Int(newX, newY), player))
                {
                    validMoves.Add(new Vector2Int(newX, newY));
                }
            }
        }
        return validMoves;
    }

    private bool IsInsideBoard(int x, int y)
    {
        if (boardTab == null || boardTab.Length == 0)
        {
            Debug.LogError("tempBoardArray is null or empty");
            return false;
        }

        int numRows = boardTab.GetLength(0);
        int numCols = boardTab.GetLength(1);
        return x >= 0 && x < numRows && y >= 0 && y < numCols;
    }

    private bool IsReachable(Vector2Int pos, int player)
    {
        if (boardTab[pos.x, pos.y] != 0)
        {
           //todo 
        }
        return true;
    }
    private List<Vector3Int> GetAllPiecesOfPlayer(Node node)
    {
        List<Vector3Int> positionArrayAndPiecesId = new();
       int playerid = node.playerid;
        //Debug.Log($"id du joueur : { playerid}");
        for (int x = 0; x < node.piecesPosition.GetLength(0); x++)
        {
            for (int y = 0; y < node.piecesPosition.GetLength(1); y++)
            {
                int value = node.piecesPosition[x, y];
                if (value!= 0 && Math.Sign(value)==Math.Sign(playerid))
                {
                    positionArrayAndPiecesId.Add(new Vector3Int(x, y, value));
                }
            }
        }
        return positionArrayAndPiecesId;
    }

    int[,] ReplaceInTab(int[,] array1,int value,Vector2Int newposition )
    {
        int lignes = array1.GetLength(0);
        int colonnes = array1.GetLength(1);
        Vector2Int pos;    
        for (int i = 0; i < lignes; i++)
        {
            for (int j = 0; j < colonnes; j++)
            {
                if (array1[i, j] == value)
                {
                    // La pièce avec la valeur spécifiée a été trouvée
                    pos=  new Vector2Int (i, j );
                    array1[i, j] = 0;
                    array1[newposition.x,newposition.y] = value;
                    return array1;
                }
                
            }
            
        }
        return array1;
    }

    public bool IsWinner(int[,] array,int playerid )
    {
        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int k = 0; k < array.GetLength(1); k++)
            {
                if (array[i,k] == 10*playerid)
                {
                    return true;
                }           
            }
        }
        return false;
    }
    public bool IsDraw(int[,] array)
    {
        return false;
    }
}

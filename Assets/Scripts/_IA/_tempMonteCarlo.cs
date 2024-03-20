using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using static UnityEditor.Experimental.GraphView.GraphView;
using YokaiNoMori.Interface;


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

    public Node(Vector2Int move,SOPiece idPiece,Node parent,int[,] boardArray)
    {
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
    public SOPiece Kodama;
    public SOPiece KodamaSamourai;
    public SOPiece Kitsune;
    public SOPiece Koropokkuru;
    public SOPiece Tanuki;
    int[,] boardTab;
    private Dictionary<int, SOPiece> pieceDataDictionnary = new();

    private void OnEnable()
    {
        PopulateDictionnary();
    }
    private void PopulateDictionnary()
    {
        pieceDataDictionnary[-5] = KodamaSamourai;
        pieceDataDictionnary[-4] = Kodama;
        pieceDataDictionnary[-3] = Kitsune;
        pieceDataDictionnary[-2] = Tanuki;
        pieceDataDictionnary[-1] = Koropokkuru;
        pieceDataDictionnary[0] = null;
        pieceDataDictionnary[1] = Koropokkuru;
        pieceDataDictionnary[2] = Tanuki;
        pieceDataDictionnary[3] = Kitsune;
        pieceDataDictionnary[4] = Kodama;
        pieceDataDictionnary[5] = KodamaSamourai;
    }
    /// <summary>
    /// will turn the number of time we decide to make more precise moves
    /// </summary>
    /// <param name="rootNode"></param>
    /// <param name="visits"></param>
    /// <returns></returns>
    public Vector2Int MonteCarloSearch(Node rootNode,int visits)
    {
        for (int i = 0; i < visits; i++)
        {
            Node node = Selection(rootNode);
            Expansion(node);
            double result = Simulation(node);
            BackPropagation(node, result);
        }
        Node bestChild = rootNode.childNodes.OrderByDescending(child => child.visits).FirstOrDefault();
        Debug.Log($"piece : {bestChild.Piece.name} meilleur coup : {bestChild.move}");
        return bestChild.move;
    }


    /// <summary>
    /// choose a node in wich we will start the exploration based on UCB 
    /// </summary>
    /// <param name="id"></param>
    private Node Selection(Node node)
    {
        Node currentNode = node;
        while (currentNode.childNodes.Any()) 
        { 
            currentNode = currentNode.childNodes.OrderByDescending(child =>child.ScoreValue()).First();
        }
        return currentNode;
    }
    /// <summary>
    /// after the expansion -> it chooses a child node to simulate the game
    /// </summary>
    /// <returns></returns>
    private double Simulation(Node node)
    {
        int currentPlayer = node.playerid;
        int[,] currentBoard = (int[,])node.piecesPosition.Clone();
        while(true)
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
            Vector2Int randomMove = validMoves[UnityEngine.Random.Range(0, validMoves.Count)];

            int pieceValue = currentBoard[randomMove.x,randomMove.y];
            currentBoard[randomMove.x,randomMove.y] = currentBoard[pieceValue,randomMove.x];
            if (IsWinner(currentBoard,currentPlayer))
            {
                return 1;
            }
            else if (IsDraw(currentBoard))
            {
                return 0;
            }
        }
    }
    /// <summary>
    /// after selection -> if the node haven t all the solutions it add node 
    /// </summary>
    /// <returns></returns>
    private void Expansion(Node node)
    {
        // we need to generate all the nodes based on every possible moves 
        List<Vector3Int> posidpiece = GetAllPiecesOfPlayer(node);
        foreach(Vector3Int piece in posidpiece)
        {
            List<Vector2Int> moves = GetValidMoves(piece, node.playerid);
            foreach(Vector2Int move in moves)
            {
                Node child = new Node(move, pieceDataDictionnary[piece.z], node, ReplaceInTab(node.piecesPosition,piece.z,move));
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

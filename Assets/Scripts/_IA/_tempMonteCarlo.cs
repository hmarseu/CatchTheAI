using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;

//ia.MonteCarloSearch(new Node(idplayer, new Vector2Int(), null, null, pieceIds), 100);

public class Node
{
    public int playerid;
    public int Piece;
    public int[,] piecesPosition; 
    public Vector2Int move; 
    public int visits;
    public double wins;
    public List<Node> childNodes;
    public Node parent;

    public Node(int idplayer, Vector2Int move, int idPiece, Node parent, int[,] boardArray)
    {
        playerid = idplayer;
        this.Piece = idPiece;
        this.move = move;
        wins = 0;
        visits = 0;
        childNodes = new List<Node>();
        this.parent = parent;
        this.piecesPosition = (int[,]) boardArray.Clone();
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



    /// <summary>
    /// will turn the number of time we decide to make more precise moves
    /// </summary>
    /// <param name="rootNode"></param>
    /// <param name="visits"></param>
    /// <returns></returns>
    public Vector3Int MonteCarloSearch(Node rootNode,int visits)
    {
        if (boardManager != null)
        {
            boardTab = (int[,])boardManager.GetBoardWithIds().Clone();
            for (int i = 0; i < visits; i++)
            {
                Node node = Selection(rootNode);
                Expansion(node);
                double result = Simulation(node);
                BackPropagation(node, result);
            }

            Node bestChild = GetBestChild(rootNode);

            if (bestChild != null)
            {
                Debug.Log($"Meilleur coup trouv� - Pi�ce : {bestChild.Piece}, Position : {bestChild.move}, Score : {bestChild.wins}");
                return new Vector3Int(bestChild.move.x, bestChild.move.y, bestChild.Piece);
            }
            else
            {
                Debug.Log("Aucun meilleur coup trouv�.");
            }
        }
        return new Vector3Int();
    }

    private Node GetBestChild(Node node)
    {
        Node bestChild = null;
        double maxScore = double.MinValue;

        foreach (Node child in node.childNodes)
        {
            // Vous pouvez �galement choisir le meilleur enfant en utilisant la valeur UCB ici
            double ucbValue = child.ScoreValue();
            if (ucbValue > maxScore)
            {
                maxScore = ucbValue;
                bestChild = child;
            }

            //if (child.wins > maxScore)
            //{
            //    maxScore = child.wins;
            //    bestChild = child;
            //}
        }

        return bestChild;
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
            Node selectedChild = null;
            foreach (Node child in currentNode.childNodes)
            {
                double scoreValue = child.ScoreValue();
                if (scoreValue > maxScoreValue)
                {
                    maxScoreValue = scoreValue;
                    selectedChild = child;
                }
            }

            currentNode = selectedChild;
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
        double score = 0;
        int maxIterations = 100; // D�finir le nombre maximum d'it�rations
        int currentIteration = 0;

        int currentPlayer = node.playerid;
        int[,] currentBoard = (int[,])node.piecesPosition.Clone();

        while (currentIteration < maxIterations)
        {
            currentIteration++;
            List<Vector3Int> playerPieces = GetAllPiecesOfPlayer(node);
            List<Vector3Int> validMoves = new List<Vector3Int>();

            foreach (Vector3Int piece in playerPieces)
            {
                List<Vector3Int> piecesMoves = GetValidMoves(piece, currentPlayer);
                validMoves.AddRange(piecesMoves);
            }

            if (validMoves.Count == 0)
            {
                return 0; // La partie est termin�e, aucun joueur n'a gagn�
            }

            Vector3Int randomMove = validMoves[UnityEngine.Random.Range(0, validMoves.Count - 1)];

            currentBoard = ReplaceInTab(currentBoard, randomMove.z, new Vector2Int(randomMove.x, randomMove.y));

            if (IsWinner(currentBoard, currentPlayer))
            {
                score = double.MaxValue; // Le joueur actuel a gagn�, attribuer un score �lev�
                break; // Sortir de la boucle puisqu'on a un vainqueur
            }

            if (korSafe(currentBoard, currentPlayer))
            {
                score += 5000;
            }
            else
            {
                score -= 5000000;
            }

            // Condition de sortie bas�e sur le r�sultat de la simulation (le joueur adverse a gagn�)
            if (IsWinner(currentBoard, -currentPlayer))
            {
                score = double.MinValue; // Le joueur adverse a gagn�, attribuer un score tr�s bas
                break; // Sortir de la boucle
            }
        }

        return score;
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
            List<Vector3Int> moves = GetValidMoves(piece, node.playerid);
            //Debug.Log($"id joueur: {node.playerid}");
            foreach(Vector2Int move in moves)
            {
                //Debug.Log($"move : {move}");
                Node child = new Node(node.playerid,move, piece.z, node, ReplaceInTab(node.piecesPosition,piece.z,move));
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
    public bool korSafe(int[,] array, int playerid)
    {
        Vector2Int korPosition = new Vector2Int();
        int width = array.GetLength(0);
        int height = array.GetLength(1);
        for (int i = 0; i < width; i++)
        {
            for (int k = 0; k < height; k++)
            {
                if (array[i, k] == 10 * playerid)
                {
                    korPosition = new Vector2Int(i, k);
                }

            }
        }

        bool otherThanKor = false;

        // verificate if the king is in danger 
        for (int i = Math.Max(0, korPosition.x - 1); i <= Math.Min(korPosition.x + 1, width - 1); i++)
        {
            for (int j = Math.Max(0, korPosition.y - 1); j <= Math.Min(korPosition.y + 1, height - 1); j++)
            {
                if (i != korPosition.x || j != korPosition.y)
                {
                    if (array[i, j] != 0 && Math.Sign(array[i, j]) != Math.Sign(playerid))
                    {
                        otherThanKor = true;
                        break;
                    }
                }
            }
            if (otherThanKor) break;
        }

        return otherThanKor;
    }
    private List<Vector3Int> GetValidMoves(Vector3Int posplusidpiece, int player)
    {
        SOPiece so = pieceDataDictionnary[posplusidpiece.z];
        List<Vector3Int> validMoves = new List<Vector3Int>();
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
                    validMoves.Add(new Vector3Int(newX, newY, posplusidpiece.z));
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
           int pieceValue = boardTab[pos.x, pos.y];
            if (math.sign(pieceValue)!=math.sign(player))
            {
                return true;
            }
            else if(math.sign(pieceValue) == math.sign(player))
            {
                return false;
            }
            else
            {
                return true;
            }
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

    public bool IsWinner(int[,] array, int playerid)
    {
        bool stillourkor = true;
        bool stillotherkor = true;
        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int k = 0; k < array.GetLength(1); k++)
            {
                if (array[i, k] == 10 * playerid)
                {
                    stillourkor = true;
                }
                else if (array[i, k] == 10 * playerid * -1)
                {
                    stillotherkor = true;
                }
            }
        }
        if (stillourkor && !stillotherkor)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool IsDraw(int[,] array)
    {
        return false;
    }
}

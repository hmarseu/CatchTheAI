using System;
using System.Collections.Generic;
using UnityEngine;
using YokaiNoMori.Interface;
using System.Linq;
using UnityEngine.UIElements;

public class MoveManager : MonoBehaviour
{
    public delegate void Possibilities(List<Vector2Int> tabPossibilite, bool parachuting);
    public static event Possibilities possibilities;

    public delegate void KoropokkuruWin(int player);
    public static event KoropokkuruWin koroWin;

    public delegate void Sursis(Player player);
    public static event Sursis sursis;


    private GameObject[,] tempBoardArray;

    [SerializeField] private Player player1;
    [SerializeField] private Player player2;

    private void OnEnable()
    {
        BoardManager.transferPion += PossibleMove;
        BoardManager.CheckEatKor += CheckCanEatKor;
    }

    private void OnDisable()
    {
        BoardManager.transferPion -= PossibleMove;
        BoardManager.CheckEatKor -= CheckCanEatKor;
    }

    private void PossibleMove(SOPiece so, Vector2Int position, GameObject[,] boardarray, Player player)
    {
        tempBoardArray = boardarray;
        List<Vector2Int> validMoves = GetValidMoves(so, position, player);
        possibilities(validMoves, false);
    }
    private void CheckCanEatKor(Player player,List<SOPiece> so,Vector2Int PosKor, List<Vector2Int> position)
    {
        
        List<Vector2Int> finalList = new List<Vector2Int>();
        for (int i = 0; i < position.Count; i++)
        {
            List<Vector2Int> validMoves = GetValidMoves(so[i], position[i], player);
            finalList.AddRange(validMoves);
        }
        if (finalList.Contains(PosKor))
        {
            sursis(player);
        }
        else
        {
            if (player.GetName() == "joueur 1")
            {
                koroWin(1);
            }
            else
            {
                koroWin(2);
            }
        }
        
       
    }
    private List<Vector2Int> GetValidMoves(SOPiece so, Vector2Int position, Player player)
    {
        List<Vector2Int> validMoves = new List<Vector2Int>();
        int[] deltaX;
        int[] deltaY;

        deltaX = new int[] { -1, 0, 1, -1, 0, 1, -1, 0, 1 };
        deltaY = new int[] { -1, -1, -1, 0, 0, 0, 1, 1, 1 };
        // take into account the player ROTATION

        if (player != player1)
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
                int newX = position.x + deltaX[i];
                int newY = position.y + deltaY[i];
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
        if (tempBoardArray == null || tempBoardArray.Length == 0)
        {
            Debug.LogError("tempBoardArray is null or empty");
            return false;
        }

        int numRows = tempBoardArray.GetLength(0);
        int numCols = tempBoardArray.GetLength(1);
        return x >= 0 && x < numRows && y >= 0 && y < numCols;
    }

    private bool IsReachable(Vector2Int pos, ICompetitor player)
    {
        if (tempBoardArray[pos.x, pos.y].transform.childCount > 0)
        {
            Transform childTransform = tempBoardArray[pos.x, pos.y].transform.GetChild(0);
            Piece pion = childTransform.gameObject.GetComponent<Piece>();

            return !pion.player.Equals(player);
        }
        return true;
    }

}
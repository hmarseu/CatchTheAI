using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveManager : MonoBehaviour
{
    public delegate void Possibilities(List<Vector2Int> tabPossibilte);
    public static event Possibilities possibilities;
    private GameObject[,] tempBoardArray;
    private void OnEnable()
    {
        BoardManager.transferPion += PossibleMove;
       
    }
    private void OnDisable()
    {
        BoardManager.transferPion -= PossibleMove;
    }
    private void PossibleMove(SOPiece so, Vector2Int position, GameObject[,] boardarray)
    {
        tempBoardArray = boardarray;
        List<Vector2Int> validMoves = new List<Vector2Int>();
        /* 
         * quelle est mouvement peut faire la piece 
         * pour chaque mouvement qu'elle peut faire on verifie si le coup est valide si il n'est pas en dehors et si il n'arrive pas sur un allié
         
        [6 7 8]     
        [3   5]
        [0 1 2] 

        \ | /
        <- -> 
        / | \

        [3,0 3,1 3,2]
        [2,0 2,1 2,2]
        [1,0 1,1 1,2]
        [0,0 0,1 0,2] 
         
         */
        int[] deltaX = { -1,  0,  1, -1, 0, 1,-1, 0 ,1};
        int[] deltaY = { -1, -1, -1,  0, 0, 0, 1, 1 ,1};
        
        for (int i = 0; i < so.PossibleMoves.Count; i++)
        {
            if (so.PossibleMoves[i])
            {
                int newX = position.x + deltaX[i];
                int newY = position.y + deltaY[i];
                if (IsInsideBoard(newX,newY))
                {
                    // Debug.Log("INSIDE : x=" + newX + " y=" + newY);
                    if (IsCaseEmpty(new Vector2Int(newX, newY)))
                    {
                        validMoves.Add(new Vector2Int(newX,newY));
                    }
                }
                // else Debug.Log("NOT INSIDE : x=" + newX + " y=" + newY);
            }        
        }
        possibilities(validMoves);
    }
    // Méthode pour vérifier si une position est à l'intérieur des limites du plateau
    private bool IsInsideBoard(int x, int y)
    {
        if (tempBoardArray.Length <= 0)
        {
            Debug.LogError("tempBoardArray is empty");
            return false;
        }

        int numRows = tempBoardArray.GetLength(0);
        int numCols = tempBoardArray.GetLength(1);
        return x >= 0 && x < numRows && y >= 0 && y < numCols;
    }

    private bool IsCaseEmpty(Vector2Int pos)
    {
        //a ajouter la verification du "camp" de ce pion
        if (tempBoardArray[pos.x, pos.y].transform.childCount > 0)
        {
            Transform childTransform = tempBoardArray[pos.x, pos.y].transform.GetChild(0);

            // piece pion = tempBoardArray[pos.x, pos.y].transform.GetChild(0).gameObject.getcomponent<piece>()
            //if   pion.player ==...
            return false;
        }
       
        return true;
    }
}

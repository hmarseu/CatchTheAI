using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveManager : MonoBehaviour
{
    public delegate void Possibilities(List<bool> tabPossibilte);
    public static event Possibilities possibilities;

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
        List<bool> validMoves = new List<bool>();
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
        
        for (int i = 0;i <= so.PossibleMoves.Count;i++)
        {
            if (so.PossibleMoves[i])
            {
                int newX = position.x + deltaX[i];
                int newY = position.y + deltaY[i];
                if (IsInsideBoard(newX,newY))
                {
                    validMoves.Add(true);
                }
                else
                {
                    validMoves.Add(false);
                }
            }
            else
            {
                validMoves.Add(false);
            }
        }
        possibilities(validMoves);
    }
    // Méthode pour vérifier si une position est à l'intérieur des limites du plateau
    private bool IsInsideBoard(int x, int y)
    {
        return x >= 0 && x < 5 && y >= 0 && y < 4; // 5 lignes (0 à 4) et 4 colonnes (0 à 3)
    }
}

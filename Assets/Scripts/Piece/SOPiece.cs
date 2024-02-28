using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Piece", menuName = "ScriptableObject/Piece")]
public class SOPiece : ScriptableObject
{
    public Sprite Image;

    public List<bool> PossibleMoves;


}

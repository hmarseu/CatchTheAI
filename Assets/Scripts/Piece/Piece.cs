using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public SOPiece soPiece;
    
    public short idPlayer;
    protected short idPiece;

    //etre mang�
    protected virtual void Defeated()
    {
        // sors du terrain
    }
    //etre parachut�
    protected virtual void Parachuted()
    {
       
    }
}

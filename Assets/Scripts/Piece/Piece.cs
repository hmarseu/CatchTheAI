using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using YokaiNoMori.Enumeration;
using YokaiNoMori.Interface;

public class Piece : MonoBehaviour,IPawn
{
    public SOPiece soPiece;
    public Player player;
   
    protected short idPiece;

    private void Start()
    {
        SpriteRenderer sr =  gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = soPiece.Image;

    
    }
    //etre mangé
    protected virtual void Defeated()
    {
        // sors du terrain
    }

    public List<Vector2Int> GetDirections()
    {
        throw new System.NotImplementedException();
    }

    public ICompetitor GetCurrentOwner()
    {
        return player;
    }

    public IBoardCase GetCurrentBoardCase()
    {
        throw new System.NotImplementedException();
    }

    public EPawnType GetPawnType()
    {
        return soPiece.ePawnType;
    }
}

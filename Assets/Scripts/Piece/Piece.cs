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
    public short idPlayer;

    private void Start()
    {
        if (soPiece != null && soPiece.Image != null)
        {
            // sprite renderer already attached?
            SpriteRenderer sr = gameObject.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                // S'il n'y a pas de SpriteRenderer attach�, ajoutez-en un
                sr = gameObject.AddComponent<SpriteRenderer>();
            }
            sr.sprite = soPiece.Image;
        }
        else
        {
            Debug.LogWarning("soPiece or its Image is not assigned in the inspector.");
        }
    }
    public void remakeSprite()
    {
        Start();
    }
    // has been eaten
    protected virtual void Defeated()
    {
        
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

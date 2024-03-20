using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using YokaiNoMori.Enumeration;
using YokaiNoMori.Interface;

public class Piece : MonoBehaviour, IPawn
{
    public SOPiece soPiece;
    public Player player;

    public int idPlayer;
    public int idPiece;

    private void Start()
    {
        if (soPiece != null && soPiece.Image != null)
        {
            // already got a sprite renderer?
            SpriteRenderer sr = gameObject.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
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

    public List<Vector2Int> GetDirections()
    {
        MoveManager moveManager = FindObjectOfType<MoveManager>();
        List<Vector2Int> validMoves = moveManager.GetValidMoves(soPiece, GetCurrentPosition(), player);
        return validMoves;
    }

    public ICompetitor GetCurrentOwner()
    {
        return player;
    }

    public EPawnType GetPawnType()
    {
        return soPiece.ePawnType;
    }

    public Vector2Int GetCurrentPosition()
    {
        throw new System.NotImplementedException();
    }

    public IBoardCase GetCurrentBoardCase()
    {
        throw new System.NotImplementedException();
    }
}

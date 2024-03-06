using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public SOPiece soPiece;
    
    public short idPlayer;
    public short idPiece;

    private void Start()
    {
        // V�rifiez que soPiece n'est pas nul avant d'acc�der � son Sprite
        if (soPiece != null && soPiece.Image != null)
        {
            // V�rifiez si un SpriteRenderer est d�j� attach�
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

    // has been eaten
    protected virtual void Defeated()
    {
        // sors du terrain
    }
    
   
}

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
        // Vérifiez que soPiece n'est pas nul avant d'accéder à son Sprite
        if (soPiece != null && soPiece.Image != null)
        {
            // Vérifiez si un SpriteRenderer est déjà attaché
            SpriteRenderer sr = gameObject.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                // S'il n'y a pas de SpriteRenderer attaché, ajoutez-en un
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

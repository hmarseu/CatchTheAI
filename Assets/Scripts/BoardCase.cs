using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardCase : MonoBehaviour
{
    public bool isClickable = false;

    public Vector2 GetSize()
    {
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        float caseWidth = 0f;
        float caseHeight = 0f;

        if (spriteRenderer != null)
        {
            caseWidth = spriteRenderer.bounds.size.x;
            caseHeight = spriteRenderer.bounds.size.y;
        }
        else
        {
            Debug.LogError("SpriteRenderer not found in children of BoardCase GameObject!");
        }
        return new Vector2(caseWidth, caseHeight);
    }

    public void PlacePiece(GameObject piece)
    {
        GameObject newPiece = Instantiate(piece, transform.position, Quaternion.identity);
        newPiece.transform.parent = transform;
    }

    public void RemovePiece()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class BoardCase : MonoBehaviour
{
    public bool isClickable = false;
    private Vector2Int positionInBoard;

    public delegate void onClick(Vector2Int positionInBoard);
    public static event onClick caseClicked;

    public void OnMouseDown()
    {
        if (isClickable)
        {
            //Debug.Log("CLICK");
            caseClicked(positionInBoard);
        }
    }

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

    public void MovePiece(GameObject piece, float rotationZ = 0f)
    {
        // Changer le parent de la pièce pour la placer dans la case
        piece.transform.SetParent(transform);
        piece.transform.rotation = Quaternion.Euler(0f, 0f, rotationZ);
        piece.transform.localPosition = Vector3.zero; // Réinitialiser la position locale
        piece.name = piece.name; // Optionnel : Mettre à jour le nom de l'objet
    }

    public void InstantiatePiece(GameObject piece, float rotationZ = 0f)
    {
        GameObject newPiece = Instantiate(piece, transform.position, Quaternion.identity);
        newPiece.transform.parent = transform;
        newPiece.transform.rotation = Quaternion.Euler(0f, 0f, rotationZ);
        newPiece.transform.localScale = piece.transform.localScale;
        newPiece.name = piece.name;
    }

    public void RemovePiece ()
    {
        foreach (Transform child in transform)
        {
            child.SetParent(null);
        }
    }

    public void DestroyPiece()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void DefinePositionOnBoard(Vector2Int position)
    {
        positionInBoard = position;
    }

}

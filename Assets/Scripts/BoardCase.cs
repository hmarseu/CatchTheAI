using CartoonFX;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using YokaiNoMori.Enumeration;
using YokaiNoMori.Interface;

public class BoardCase : MonoBehaviour, IBoardCase
{
    [SerializeField] private bool _isClickable = false;
    private VFX_Manager _vfxManager;
    private SFXManager _sfxManager;

    public bool isClickable
    {
        get { return _isClickable; }
        set
        {
            _isClickable = value;
            HighlightCase();
        }
    }

    private Vector2Int positionInBoard;

    private static readonly Color HIGHLIGHTCOLOR = new Color(0.5f, 0.8f, 0.1f, 0.5f);
    private static readonly Color NORMALCOLOR = new Color(0.5f, 0.8f, 0.1f, 0f);

    public delegate void onClick(Vector2Int positionInBoard);
    public static event onClick caseClicked;

    public void Awake()
    {
        _vfxManager = GameObject.Find("VFX_List").GetComponent<VFX_Manager>();
        _sfxManager = GameObject.Find("SFXManager").GetComponent<SFXManager>();
    }

    public void OnMouseDown()
    {
        if (isClickable)
        {
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
        // change the parent of the piece
        piece.transform.SetParent(transform);
        PlayFX();
        piece.transform.rotation = Quaternion.Euler(0f, 0f, rotationZ);
        piece.transform.localPosition = Vector3.zero;
        piece.name = piece.name;
    }

    public void InstantiatePiece(GameObject piece, float rotationZ = 0f)
    {
        GameObject newPiece = Instantiate(piece, transform.position, Quaternion.identity);
        newPiece.transform.parent = transform;
        newPiece.transform.rotation = Quaternion.Euler(0f, 0f, rotationZ);
        newPiece.transform.localScale = piece.transform.localScale;
        newPiece.name = piece.name;
    }

    public void HighlightCase()
    {
        if (isClickable) this.gameObject.GetComponent<SpriteRenderer>().color = HIGHLIGHTCOLOR;
        else this.gameObject.GetComponent<SpriteRenderer>().color = NORMALCOLOR;
    }

    public void RemovePiece()
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

    public void PlayFX()
    {
        _vfxManager?.PlayAtIndex(4, this.transform.position);
        _sfxManager?.PlaySoundEffect(1);
    }

    public Vector2Int GetPosition()
    {
        return positionInBoard;
    }

    public IPawn GetPawnOnIt()
    {
        Transform firstChild = transform.GetChild(0);
        Piece piece = firstChild.GetComponent<Piece>();

        if (piece != null) return piece;
        else return null;
    }

    public bool IsBusy()
    {
        IPawn pawnOnCase = GetPawnOnIt();

        if (pawnOnCase != null) return true;
        else return false;
    }
}
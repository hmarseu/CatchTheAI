using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using YokaiNoMori.Enumeration;
using YokaiNoMori.Interface;

public class Player : MonoBehaviour, ICompetitor
{
    public int idPlayer;
    public BoardManager boardManager;
    private ECampType myCamp;
    private string name;
    _tempMonteCarlo ia;
    public bool isAI;

    private void Start()
    {
        ia = GameObject.FindObjectOfType<_tempMonteCarlo>();
        if (!boardManager) throw new System.ArgumentNullException();
    }

    public ECampType GetCamp()
    {
       return myCamp;
    }

    public string GetName()
    {
        return name;
    }
    public void SetName(string name)
    { 
        this.name = name;
    }

    public void SetCamp(ECampType camp)
    {
        this.myCamp = camp;
    }

    public void StartTurn()
    {
        // if the player is an IA
        if (isAI)
        {
            Vector3Int bestMove = ia.MonteCarloSearch(new Node(idPlayer, new Vector2Int(), 0, null, boardManager.GetBoardWithIds()), 500);
            //Debug.Log($"pieceid + position : {bestMove} ");

            // get a selectedPiece to play
            IPawn pawnTarget = boardManager.GetPieceById(bestMove.z);

            // get a postion where to move
            Vector2Int newPosition = new Vector2Int(bestMove.x,bestMove.y);

            // get selected action type
            EActionType actionType = EActionType.MOVE;

            // Debug.Log($"piece : {pawnTarget} direction : {newPosition}, type : {actionType}");

            // do action
            boardManager.DoAction(pawnTarget, newPosition, actionType);
            
        }
        else
        {
            // do nothing, changeTurn already did 
        }
    }

    public void StopTurn()
    {
        boardManager.LogPieceIds();
        Debug.LogWarning("_________");
        boardManager.ChangeTurn();
        //StartTurn();
    }

    public void GetDatas()
    {
        throw new System.NotImplementedException();
    }

    public void Init(IGameManager igameManager, float timerForAI, ECampType currentCamp)
    {
        throw new System.NotImplementedException();
    }
}

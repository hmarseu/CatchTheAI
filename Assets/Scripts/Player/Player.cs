using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YokaiNoMori.Enumeration;
using YokaiNoMori.Interface;

public class Player : MonoBehaviour, ICompetitor
{
    public BoardManager boardManager;
    private ECampType myCamp;
    private string name;

    public bool isAI;

    private void Start()
    {
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

    public void Init(IGameManager igameManager, float timerForAI)
    {
        throw new System.NotImplementedException();
    }

    public void StartTurn()
    {
        // if the player is an IA
        if (isAI)
        {
            // get a selectedPiece to play
            IPawn pawnTarget = null;

            // get a postion where to move
            Vector2Int newPosition = new Vector2Int(0, 0);

            // get selected action type
            EActionType actionType = EActionType.MOVE;

            // do action
            boardManager.DoAction(pawnTarget, newPosition, actionType);
        }
        else
        {
            // do nothing, changeTurn already did it
        }
    }

    public void StopTurn()
    {
        boardManager.ChangeTurn();
        StartTurn();
    }

    public void GetDatas()
    {
        throw new System.NotImplementedException();
    }

}

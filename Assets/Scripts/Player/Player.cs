using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YokaiNoMori.Enumeration;
using YokaiNoMori.Interface;

public class Player : MonoBehaviour, ICompetitor
{

    private ECampType myCamp;
    private string name;
 
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
        //donc tout va devoir se passer par la sur qui joue comment on joue 
        throw new System.NotImplementedException();
    }

    public void StopTurn()
    {
        //donc tout va devoir se passer par la sur comment on termine le tour
        throw new System.NotImplementedException();
    }
}

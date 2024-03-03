using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundOutcome : Singleton<RoundOutcome>
{
    [SerializeField]
    private GameObject P1Wins;

    [SerializeField]
    private GameObject P1Loses;

    [SerializeField]
    private GameObject Draw;

    public void Outcome(string outcome)
    {
        if (outcome == "Win")
        {
            P1Wins.SetActive(true);
            P1Loses.SetActive(false);
            Draw.SetActive(false);
        }
        else if (outcome == "Lose")
        {
            P1Wins.SetActive(false);
            P1Loses.SetActive(true);
            Draw.SetActive(false);
        }
        else
        {
            P1Wins.SetActive(false);
            P1Loses.SetActive(false);
            Draw.SetActive(true);
        }
    }

    public void HideAll()
    {
        P1Wins.SetActive(false);
        P1Loses.SetActive(false);
        Draw.SetActive(false);
    }
}

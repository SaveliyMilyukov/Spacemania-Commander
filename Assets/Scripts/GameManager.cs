using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Unit[] allUnitsAndBuildingsOnMap;

    public static GameManager instance;

    private void Awake()
    {
        instance = this;
        UpdateAllUnits();
    }

    public void UpdateAllUnits()
    {
        allUnitsAndBuildingsOnMap = FindObjectsOfType<Unit>();
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public Unit[] allUnitsAndBuildingsOnMap;
    public Tilemap groundTilemap;

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

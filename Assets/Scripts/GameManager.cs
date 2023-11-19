using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public Unit[] allUnitsAndBuildingsOnMap;
    public ResourceField[] resourceFields;
    public Tilemap groundTilemap;

    public static GameManager instance;

    private void Awake()
    {
        instance = this;
        UpdateAllUnits();
    }

    public void Update()
    {
        if(Time.deltaTime >= 1f) // Если время между вызовом функции больше 1 секунды (крайне низкий фпс), то редактор ставится на паузу
        {
            Debug.Break();
        }
    }

    public void UpdateAllUnits()
    {
        resourceFields = FindObjectsOfType<ResourceField>();
        allUnitsAndBuildingsOnMap = FindObjectsOfType<Unit>();
    }
}

using System.Collections.Generic;
using UnityEngine;

// Главный скрипт игрока, от которого наследуются контроллер бота (PlayerBot) и контроллер игрока (PlayerController)
public class PlayerCommander : MonoBehaviour
{
    [Header("Team")]
    public int playerNumber = 0;
    public int playerTeam = -1;
    [Header("Resources")]
    public int ore = 50; // Кол-во руды у игрока
    public int gas = 0; // Кол-во газа у игрока
    public int limitCurrent = 0; // Текущий лимит игрока (Сколько занимают лимита его юниты)
    public int limitMax = 15; // Текущий лимит игрока, который он может построить (Максимальное количество юнитов, которое может иметь в данный момент игрок. Зависит от построенных хранилищ/повелителей и т.п.)
    public const int gameLimitMax = 300; // Какой вообще может быть максимальный лимит у игрока в игре (в StarCraft абсолютный лимит - 200)
    [Header("Units")]
    public List<Unit> unitsAndConstructions;
    public List<UnitAI> units;
    [Space(5)]
    public List<Unit> unitsControlling;
    [Header("Buildings")]
    public Building[] buildingsPrefabs;
    public BuildingWorkplace buildingWorkplacePrefab;
    

    public virtual void Awake()
    {
        UpdateUnits();
    }

    public virtual void Start()
    {

    }

    public virtual void Update()
    {

    }

    public virtual void UpdateUnits()
    {
        Unit[] allUnits = FindObjectsOfType<Unit>();
        unitsAndConstructions.Clear();
        limitCurrent = 0;
        for(int i = 0; i < allUnits.Length; i++)
        {
            if(allUnits[i].playerNumber == playerNumber) // Если юнит принадлежит текущему игрому 
            {
                unitsAndConstructions.Add(allUnits[i]);
                if(allUnits[i].GetComponent<UnitAI>())
                {
                    units.Add(allUnits[i].GetComponent<UnitAI>());
                    limitCurrent += allUnits[i].GetComponent<UnitAI>().limitPrice;
                }
            }
        }
    }

    public virtual void AddOrderToUnits(UnitOrder.OrderType orderType_, Vector2 position_, Transform target_, bool replaceMode_)
    {
        for (int i = 0; i < unitsControlling.Count; i++)
        {
            if (unitsControlling[i].GetComponent<UnitAI>())
            {
                UnitAI u = unitsControlling[i].GetComponent<UnitAI>();
                if (replaceMode_)
                {
                    u.ClearOrders(true);
                }
                u.AddOrder(orderType_, position_, target_.transform);
            }
        }
    }

    public virtual void AddRallyPoints(Vector2 rallyPoint_)
    {
        for (int i = 0; i < unitsControlling.Count; i++)
        {
            if (unitsControlling[i].GetComponent<Building>())
            {
                Building b = unitsControlling[i].GetComponent<Building>();
                b.EditRallyPoint(rallyPoint_);
            }
        }
    }
}

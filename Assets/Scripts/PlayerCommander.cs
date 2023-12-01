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
    public List<Building> constructions;
    public List<UnitAI> units;
    [Space(5)]
    public List<Unit> unitsControlling;
    [Header("Buildings")]
    public Building[] buildingsPrefabs;
    [Space(5)]
    public BuildingMark buildingMarkPrefab;

    [HideInInspector] public bool isBot = false;

    public virtual void Awake()
    {
        UpdateUnits();
    }

    public virtual void Update()
    {

    }

    public virtual void UpdateUnits()
    {
        Unit[] allUnits = FindObjectsOfType<Unit>();
        unitsAndConstructions.Clear();
        units.Clear();
        limitCurrent = 0;
        int supplyCount = 0;
        for(int i = 0; i < allUnits.Length; i++)
        {
            if(allUnits[i].playerNumber == playerNumber) // Если юнит принадлежит текущему игрому 
            {
                if(allUnits[i].GetComponent<GetSupply>())
                {
                    supplyCount += allUnits[i].GetComponent<GetSupply>().supplyCount;
                }

                unitsAndConstructions.Add(allUnits[i]);
                if(allUnits[i].GetComponent<UnitAI>())
                {
                    units.Add(allUnits[i].GetComponent<UnitAI>());
                    limitCurrent += allUnits[i].GetComponent<UnitAI>().limitPrice;
                }
                else if(allUnits[i].GetComponent<Building>())
                {
                    constructions.Add(allUnits[i].GetComponent<Building>());
                }
            }
        }

        limitMax = supplyCount;
    }

    public virtual void AddOrderToUnits(UnitOrder.OrderType orderType_, Vector2 position_, Transform target_, bool replaceMode_)
    {
        for (int i = 0; i < unitsControlling.Count; i++)
        {
            if (unitsControlling[i] == null) continue;

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
            if (unitsControlling[i] == null) continue;
            if (unitsControlling[i].GetComponent<Building>())
            {
                Building b = unitsControlling[i].GetComponent<Building>();
                b.EditRallyPoint(rallyPoint_);
            }
        }
    }

    public virtual void ReturnResourcesByPrice(ResourcePrice resourcePrice_)
    {
        ore += resourcePrice_.orePrice;
        gas += resourcePrice_.gasPrice;

        Debug.Log("ReturnResourcesByPrice!\nSplendide:" + resourcePrice_.orePrice + "\nOleum:" + resourcePrice_.gasPrice);
    }

    public virtual bool CheckPrice(ResourcePrice resourcePrice_)
    {
        bool result = false;

        if (resourcePrice_.orePrice <= ore && resourcePrice_.gasPrice <= gas) result = true;

        return result;
    }

    public void DecreaseResources(ResourcePrice resourcePrice_)
    {
        ore -= resourcePrice_.orePrice;
        gas -= resourcePrice_.gasPrice;
    }
}

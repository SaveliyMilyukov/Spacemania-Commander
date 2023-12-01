using System.Collections.Generic;
using UnityEngine;

public class PlayerBot : PlayerCommander
{
    public enum BotActionType
    {
        None, // Бездействие
        Economy, // Экономика
        Army // Армия
    }

    public BotActionType currentBotAction = BotActionType.None;
    public int lastCycleState = -1;
    [Space(5)]
    public XagLair[] lairs;
    public List<XagLarva> larvas;
    public List<UnitAI> workers;
    public List<UnitAI> army;
    [Space(5)]
    [SerializeField]
    public XagLair localPlayerLair;

    public override void Awake()
    {
        isBot = true;
        base.Awake();

        Invoke(nameof(UpdateBases), 0.1f);
        InvokeRepeating(nameof(SwitchBotState), 1f, 10f);
    }


    public override void Update()
    {
        switch(currentBotAction) // Что делает бот в данный момент?
        {
            case BotActionType.None: // Бездействует                      

                break;

            case BotActionType.Economy: // Занимается экономикой             
                if(lairs.Length > 0) // Если есть ульи, то производится найм недостающих рабочих
                {
                    if(workers.Count < lairs.Length * 6) // Если мало рабочих
                    {
                        if(ore >= lairs[0].units[0].unitPrice.orePrice) // Если хватает денег
                        {
                            if (larvas.Count < lairs.Length)
                            {
                                for (int i = 0; i < lairs.Length; i++) // Найм личинок
                                {
                                    if (!lairs[i].isBuildingUnit && lairs[i].larvas > 0)
                                    {
                                        lairs[i].OrderToBuildUnit(0);
                                    }
                                }                             
                            }

                            for (int i = 0; i < larvas.Count; i++) // Производство рабочих из личинок
                            {
                                if (!larvas[i].isMutating)
                                {
                                    larvas[i].TryToStartMutation(0);
                                }
                            }
                        }
                    }
                }
                if(CheckPrice(buildingsPrefabs[2].buildingPrice)) // Если хватает денег на растворитель/экстрактор
                {
                    //Debug.Log("|||||||||");
                    int orders = 0;
                    for(int i = 0; i < lairs.Length; i++)
                    {
                        bool toBreak_ = false;
                        for (int j = 0; j < lairs[i].nearestResourceFields.Length; j++)
                        {
                            if (lairs[i].nearestResourceFields[j].buildingBuildedOn == null &&
                                lairs[i].nearestResourceFields[j].buildingMarkOn == null)
                            {
                                if (lairs[i].nearestResourceFields[j].resourceType == ResourceType.Ore &&
                                   CheckPrice(buildingsPrefabs[2].buildingPrice))
                                {
                                    OrderToNearestWorkerToBuild(2, lairs[i].nearestResourceFields[j].transform.position);
                                    orders++;
                                }
                                else if (lairs[i].nearestResourceFields[j].resourceType == ResourceType.Gas &&
                                  CheckPrice(buildingsPrefabs[1].buildingPrice))
                                {
                                    OrderToNearestWorkerToBuild(1, lairs[i].nearestResourceFields[j].transform.position);
                                    orders++;
                                }

                                if(orders * buildingsPrefabs[2].buildingPrice.orePrice >= ore)
                                {
                                    toBreak_ = true;
                                    break;
                                }
                            }
                        }
                        if (toBreak_) break;
                    }
                }

                for(int i = 0; i < workers.Count; i++) // Отправка бездействующих рабочих на добычу
                {
                    if(workers[i] == null)
                    {
                        continue;
                    }

                    //Debug.Log("workers[" + i + "] (" + workers[i].gameObject.name + ")\nisCanGather: " + workers[i].isCanGather + "\nResource in hands:" + workers[i].resourceInHands);
                    if(workers[i].nowOrder.isNull)
                    {
                       //Debug.Log("wi");
                        if(workers[i].resourceInHands == ResourceType.None)
                        {
                            //Debug.Log("w");
                            workers[i].FindNearestResourceField(ResourceType.None);
                        }
                        else
                        {
                            //Debug.Log("i");
                            workers[i].FindNearestResourceStorage();
                        }
                    }
                }             
                break;

            case BotActionType.Army: // Занимается армией
                if(larvas.Count < lairs.Length * 2)
                {
                    for (int i = 0; i < lairs.Length; i++) // Найм личинок
                    {
                        if (!lairs[i].isBuildingUnit && lairs[i].larvas > 0)
                        {
                            lairs[i].OrderToBuildUnit(0);
                        }
                    }
                }
                for (int i = 0; i < larvas.Count; i++) // Производство армии из личинок
                {
                    if (!larvas[i].isMutating)
                    {
                        if (!larvas[i].TryToStartMutation(2)) larvas[i].TryToStartMutation(1);
                    }
                }
                if(army.Count >= 3)
                {
                    for(int i = 0; i < army.Count; i++)
                    {
                        if(army[i].nowOrder.isNull)
                        {
                            army[i].ClearOrders(true);
                            army[i].AddOrder(UnitOrder.OrderType.MoveAndAttack, localPlayerLair.transform.position, localPlayerLair.transform);
                        }
                    }
                }
                break;
        }
    }

    public void OrderToNearestWorkerToBuild(int buildingIndex_, Vector2 placePosition_)
    {
        if (buildingIndex_ < 0) return;
        if (ore < buildingsPrefabs[buildingIndex_].buildingPrice.orePrice ||
           gas < buildingsPrefabs[buildingIndex_].buildingPrice.gasPrice) return;

       // Debug.Log("OrderToNearestWorkerToBuild! (" + buildingIndex_ + " " + placePosition_ + ")");

        // Поиск рабочего для дачи приказа на строительство
        bool isBuilderFound = false;
        XagDrone builder = null;
        float dstToBuilder = 999999;
        for (int i = 0; i < units.Count; i++)
        {
            if(units[i] == null)
            {
                units.Remove(units[i]);
                i--;
                continue;
            }
            if (units[i].GetComponent<XagDrone>())
            {
                isBuilderFound = true;
                float curDst = Vector2.Distance(placePosition_, units[i].transform.position);
                if(curDst < dstToBuilder || builder == null)
                {
                    builder = units[i].GetComponent<XagDrone>();
                    dstToBuilder = curDst;
                }
            }
        }
       
        if (!isBuilderFound) return;
        //Debug.Log("builder Found!");

        Vector3Int cellPos = GameManager.instance.groundTilemap.WorldToCell(placePosition_);
        Vector3 constructionPosition = GameManager.instance.groundTilemap.CellToWorld(cellPos);

        ResourceField placedOn = null;
        bool isCanBePlaced = false;
        if (buildingsPrefabs[buildingIndex_].need == Building.BuildingNeed.None)
        {
            isCanBePlaced = true;
        }
        else
        {
            for (int i = 0; i < GameManager.instance.resourceFields.Length; i++)
            {
                if (buildingsPrefabs[buildingIndex_].need == Building.BuildingNeed.OreField && GameManager.instance.resourceFields[i].resourceType == ResourceType.Ore ||
                    buildingsPrefabs[buildingIndex_].need == Building.BuildingNeed.Geyser && GameManager.instance.resourceFields[i].resourceType == ResourceType.Gas)
                {
                    if (cellPos == (Vector3Int)GameManager.instance.resourceFields[i].positionInGrid)
                    {
                        isCanBePlaced = true;
                        placedOn = GameManager.instance.resourceFields[i];
                        break;
                    }
                }
            }
        }

        if (!isCanBePlaced) return;
        //Debug.Log("Is can be placed!!");

        BuildingMark mark = Instantiate(buildingMarkPrefab, constructionPosition, Quaternion.identity);
        mark.building = buildingsPrefabs[buildingIndex_];
        mark.playerNumber = playerNumber;
        mark.render.sprite = buildingsPrefabs[buildingIndex_].cursorSprite;
        mark.buildingPrice = buildingsPrefabs[buildingIndex_].buildingPrice;
        mark.buildBlockDistance = buildingsPrefabs[buildingIndex_].buildBlockDistance;
        mark.placedOn = placedOn;

        builder.ClearOrders(true);
        builder.AddOrder(UnitOrder.OrderType.Build, mark.transform.position, mark.transform, buildingIndex_);

        ore -= buildingsPrefabs[buildingIndex_].buildingPrice.orePrice;
        gas -= buildingsPrefabs[buildingIndex_].buildingPrice.gasPrice;
    }

    public void UpdateBases()
    {
        PlayerController pl = FindObjectOfType<PlayerController>();
        for(int i = 0; i < pl.constructions.Count; i++)
        {
            if(pl.constructions[i].GetComponent<XagLair>())
            {
                localPlayerLair = pl.constructions[i].GetComponent<XagLair>();
            }
        }

        List<XagLair> lairsFound = new List<XagLair>();
        for (int i = 0; i < constructions.Count; i++)
        {
            if(constructions[i].GetComponent<XagLair>())
            {
                lairsFound.Add(constructions[i].GetComponent<XagLair>());
            }
        }

        lairs = lairsFound.ToArray();
    }

    void SwitchBotState()
    {
        switch(currentBotAction)
        {
            case BotActionType.None:
                currentBotAction = BotActionType.Economy;
                break;
            case BotActionType.Economy:
                currentBotAction = BotActionType.Army;
                break;
            case BotActionType.Army:
                currentBotAction = BotActionType.Economy;
                break;
        }
    }

    public override void UpdateUnits()
    {
        base.UpdateUnits();

        workers.Clear();
        for(int i = 0; i < units.Count; i++)
        {
            if(units[i].GetComponent<XagDrone>())
            {
                workers.Add(units[i]);
            }
            else if(!units[i].GetComponent<XagLarva>() &&
                !units[i].GetComponent<GetSupply>())
            {
                army.Add(units[i]);
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class UnitAI : Unit
{
    public int limitPrice = 1;
    [Header("Order")]
    [SerializeField] List<UnitOrder> orders;
    public UnitOrder nowOrder;
    [SerializeField] Vector2 targetPosition;
    [Space(5)]
    [Header("Moving")]
    public float moveSpeed = 5f;
    [Header("Gathering")]
    public bool isCanGather = false;
    public ResourceType resourceInHands = ResourceType.None;
    public GameObject[] resourcesInHandsSprites;
    [SerializeField] float gatherTime = 7.5f;
    [SerializeField] float timeToGather = 7.5f;
    public Transform lastResField;

    Rigidbody2D rb;

    public override void Awake()
    {
        base.Awake();

        rb = GetComponent<Rigidbody2D>();   
    }


    public override void Update()
    {
        base.Update();

        CheckOrder();

        if (!nowOrder.isNull)
        {
            switch (nowOrder.orderType)
            {
                case UnitOrder.OrderType.HoldPosition:
                    targetPosition = nowOrder.movePosition;
                    break;
                case UnitOrder.OrderType.Move:
                    targetPosition = nowOrder.movePosition;
                    break;
                case UnitOrder.OrderType.MoveAndAttack:
                    targetPosition = nowOrder.movePosition;
                    break;
                case UnitOrder.OrderType.Follow:
                    if (nowOrder.moveTarget == null)
                    {
                        FinishOrder(false);
                        return;
                    }
                    targetPosition = nowOrder.moveTarget.position;
                    break;
                case UnitOrder.OrderType.Attack:
                    if (nowOrder.moveTarget == null)
                    {
                        FinishOrder(false);
                        return;
                    }
                    targetPosition = nowOrder.moveTarget.position;
                    break;
                case UnitOrder.OrderType.Build:
                    if(nowOrder.moveTarget == null)
                    {
                        FinishOrder(false);
                        return;
                    }
                    targetPosition = nowOrder.moveTarget.position;
                    break;
                case UnitOrder.OrderType.Gather:
                    targetPosition = nowOrder.moveTarget.position;
                    break;
                case UnitOrder.OrderType.DelieverRes:
                    targetPosition = nowOrder.moveTarget.position;
                    break;
            }

            if (nowOrder.orderType != UnitOrder.OrderType.Gather) timeToGather = gatherTime;

            if(!isCanGather && nowOrder.orderType == UnitOrder.OrderType.Gather)
            {
                FinishOrder(true);
                return;
            }

            float distanceToTarget = Vector2.Distance(transform.position, targetPosition);
            if(!attack.isHited)
            {
                transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            }
            if(nowOrder.orderType != UnitOrder.OrderType.HoldPosition)
            {
                if(distanceToTarget < 0.2f)
                {                       
                    if(nowOrder.orderType == UnitOrder.OrderType.Build)
                    {
                        TryToBuildConstruction();
                    }
                    else if (nowOrder.orderType == UnitOrder.OrderType.Gather)
                    {
                        if(resourceInHands != ResourceType.Ore)
                        {
                            ResourceField resField = nowOrder.moveTarget.GetComponent<ResourceField>();
                            if (resField != null) // Если цель - месторождение
                            {
                                if(resField.busedBy == null || resField.busedBy == this)
                                {
                                    if(resField.resourceType == ResourceType.Ore)
                                    {
                                        if(timeToGather <= 0)
                                        {
                                            resField.busedBy = null;
                                            resourceInHands = ResourceType.Ore;
                                            resourcesInHandsSprites[(int)resourceInHands - 1].SetActive(true);
                                            resField.health -= 8;
                                            if(orders.Count < 2) FindNearestResourceStorage();
                                        }
                                        else
                                        {
                                            resField.busedBy = this;
                                            lastResField = resField.transform;
                                            timeToGather -= Time.deltaTime;
                                        }
                                    }
                                }
                                else
                                {
                                    FindNearestResourceField(resField.resourceType, resField, true);
                                }
                            }
                        }
                        else
                        {
                            FindNearestResourceStorage();
                        }
                    }
                    else if (nowOrder.orderType == UnitOrder.OrderType.DelieverRes)
                    {
                        ResourceStorage resStorage = nowOrder.moveTarget.GetComponent<ResourceStorage>();
                        if(resStorage != null) // Если цель - склад ресурсов
                        {
                            if(resStorage.playerNumber == playerNumber)
                            {
                                resStorage.TakeResource(resourceInHands);
                                resourcesInHandsSprites[(int)resourceInHands - 1].SetActive(false);
                                resourceInHands = ResourceType.None;
                            }
                        }
                    }
                    else
                    {
                        FinishOrder(true);
                    }
                }          
            }
        }
        else
        {
            timeToGather = gatherTime;
        }

    }

    private void CheckOrder()
    {
        if(nowOrder.isNull)
        {
            if(orders.Count > 0)
            {
                nowOrder = orders[0];
                orders.Remove(orders[0]);
            }
        }

        if (orders.Count <= 0) return;

        if(nowOrder.orderType == UnitOrder.OrderType.HoldPosition)
        {
            orders.Clear();
            return;
        }
    }
    public void ClearOrders(bool finishOrder_)
    {
        if(orders.Count > 1)
        {
            for(int i = 1; i < orders.Count; i++)
            {
                if(orders[i].orderType == UnitOrder.OrderType.Build)
                {
                    orders[i].moveTarget.gameObject.GetComponent<BuildingMark>().CancelBuild();
                }
            }
        }

        orders.Clear();
        attackTarget = null;

        if (finishOrder_) FinishOrder(true);
    }

    public void FinishOrder(bool cancelBuild_)
    {
        if(nowOrder.orderType == UnitOrder.OrderType.Gather)
        {
            if(nowOrder.moveTarget.GetComponent<ResourceField>())
            {
                if(nowOrder.moveTarget.GetComponent<ResourceField>().busedBy == this)
                {
                    nowOrder.moveTarget.GetComponent<ResourceField>().busedBy = null;
                }
            }
        }
        if(cancelBuild_)
        {
            if(nowOrder.orderType == UnitOrder.OrderType.Build)
            {
                if(nowOrder.moveTarget != null)
                {
                    nowOrder.moveTarget.gameObject.GetComponent<BuildingMark>().CancelBuild();
                }
            }
        }
        nowOrder.isNull = true;
    }

    public void AddOrder(UnitOrder.OrderType orderType_, Vector2 positionToMove_, Transform target_, int buildingIndex_ = -1)
    {
        if (orderType_ == UnitOrder.OrderType.Gather && !isCanGather) return;

        UnitOrder newOrder = new UnitOrder();
        newOrder.orderType = orderType_;
        newOrder.movePosition = positionToMove_;
        newOrder.moveTarget = target_;
        newOrder.buildingIndex = buildingIndex_;
        newOrder.isNull = false;
        if (newOrder.orderType == UnitOrder.OrderType.Attack) attackTarget = target_.GetComponent<Unit>();

        orders.Add(newOrder);
    }

    public void FindNearestResourceStorage()
    {
        ResourceStorage[] allStorages = FindObjectsOfType<ResourceStorage>();
        List<ResourceStorage> myStorages = new List<ResourceStorage>();
        for(int i = 0; i < allStorages.Length; i++)
        {
            if(allStorages[i].playerNumber == playerNumber)
            {
                myStorages.Add(allStorages[i]);
            }
        }

        ResourceStorage nearestResourceStorage = null;
        float minDst = 999999999;
        for(int i = 0; i < myStorages.Count; i++)
        {
            float curDst = Vector2.Distance(transform.position, myStorages[i].transform.position);
            if(curDst < minDst)
            {
                nearestResourceStorage = myStorages[i];
                minDst = curDst;
            }
        }

        if(nearestResourceStorage != null)
        {
            FinishOrder(true);
            AddOrder(UnitOrder.OrderType.DelieverRes, nearestResourceStorage.transform.position, nearestResourceStorage.transform);
        }
    }

    public void FindNearestResourceField(ResourceType resourceType_, ResourceField exception_ = null, bool exceptBused = false, float maxDistance = 7)
    {
        ResourceField[] allFields = FindObjectsOfType<ResourceField>();

        ResourceField nearestResourceField = null;
        float minDst = 999999999;
        for (int i = 0; i < allFields.Length; i++)
        {
            if (exceptBused)
            {
                if(allFields[i].buildingBuildedOn != null)
                {
                    if (allFields[i].buildingBuildedOn.unitIn != null) continue;
                }
                else
                {
                    if (allFields[i].busedBy != null) continue;
                }
            }
            if (resourceType_ != ResourceType.None) // None = Any
            {
                if (resourceType_ != allFields[i].resourceType) continue;
            }

            if (allFields[i].resourceType == ResourceType.Gas && allFields[i].busedBy == null) continue; // Пропускаем гейзеры без построенного добывателя Олеума

            float curDst = Vector2.Distance(transform.position, allFields[i].transform.position);
            if (curDst < minDst && curDst <= maxDistance)
            {
                if(exception_ == null || exception_ != allFields[i])
                {
                    nearestResourceField = allFields[i];
                    minDst = curDst;
                }
            }
        }

        FinishOrder(true);
        if (nearestResourceField != null)
        {          
            AddOrder(UnitOrder.OrderType.Gather, nearestResourceField.transform.position, nearestResourceField.transform);
        }
    }

    public void TryToBuildConstruction()
    {
        BuildingMark mark = nowOrder.moveTarget.GetComponent<BuildingMark>();
        mark.playerNumber = playerNumber;
        mark.building = myPlayer.buildingsPrefabs[nowOrder.buildingIndex];

        mark.TryToStartBuild(this);
        FinishOrder(false);
    }
}

[System.Serializable]
public class UnitOrder
{
    public enum OrderType
    { 
        HoldPosition, // Удерживать позицию
        Move, // Двигаться (ПКМ)
        Follow, // Преследовать (юнита)
        MoveAndAttack, // Двигаться и атаковать враждебных юнитов на пути (A - клик)
        Attack, // Атаковать (юнита)
        Build, // Строить здание
        Gather, // Добывать
        DelieverRes // Принести ресурс на базу
    }

    public bool isNull = true;
    [Space(5)]
    public OrderType orderType;
    [Space(5)]
    public Vector2 movePosition;
    public Transform moveTarget;
    [Space(5)]
    public int buildingIndex = -1;
}


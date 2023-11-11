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
    [SerializeField] float moveSpeed = 5f;

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
                    targetPosition = nowOrder.moveTarget.position;
                    break;
                case UnitOrder.OrderType.Attack:
                    targetPosition = nowOrder.moveTarget.position;
                    break;
                case UnitOrder.OrderType.Build:
                    targetPosition = nowOrder.moveTarget.position;
                    break;
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
                    else
                    {
                        FinishOrder(true);
                    }
                }          
            }
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
        for(int i = 0; i < orders.Count; i++)
        {
            if(orders[i].orderType == UnitOrder.OrderType.Build)
            {
                orders[i].moveTarget.gameObject.GetComponent<BuildingMark>().CancelBuild();
            }
        }
        orders.Clear();
        attackTarget = null;

        if (finishOrder_) FinishOrder(true);
    }

    public void FinishOrder(bool cancelBuild_)
    {
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
        UnitOrder newOrder = new UnitOrder();
        newOrder.orderType = orderType_;
        newOrder.movePosition = positionToMove_;
        newOrder.moveTarget = target_;
        newOrder.buildingIndex = buildingIndex_;
        newOrder.isNull = false;
        if (newOrder.orderType == UnitOrder.OrderType.Attack) attackTarget = target_.GetComponent<Unit>();

        orders.Add(newOrder);
    }

    public void TryToBuildConstruction()
    {
        BuildingMark mark = nowOrder.moveTarget.GetComponent<BuildingMark>();
        mark.playerNumber = playerNumber;
        mark.building = PlayerController.localPlayer.buildingsPrefabs[nowOrder.buildingIndex];

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
        Build // Строить здание
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


using System.Collections.Generic;
using UnityEngine;

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
            }

            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            if(nowOrder.orderType != UnitOrder.OrderType.HoldPosition)
            {
                if(Vector2.Distance(transform.position, targetPosition) < 0.2f)
                {
                    nowOrder.isNull = true;
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
        orders.Clear();

        if (finishOrder_) FinishOrder();
    }

    public void FinishOrder()
    {
        nowOrder.isNull = true;
    }

    public void AddOrder(UnitOrder.OrderType orderType_, Vector2 positionToMove_, Transform target_)
    {
        UnitOrder newOrder = new UnitOrder();
        newOrder.orderType = orderType_;
        newOrder.movePosition = positionToMove_;
        newOrder.moveTarget = target_;
        newOrder.isNull = false;
        orders.Add(newOrder);
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
        Attack // Атаковать (юнита)
    }

    public bool isNull = true;
    [Space(5)]
    public OrderType orderType;
    [Space(5)]
    public Vector2 movePosition;
    public Transform moveTarget;
}


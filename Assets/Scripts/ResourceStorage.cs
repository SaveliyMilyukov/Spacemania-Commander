using System.Collections.Generic;
using UnityEngine;

public class ResourceStorage : Building
{
    public ResourceField[] nearestResourceFields;
    public override void Awake()
    {
        base.Awake();
        Invoke(nameof(SetPlayer), 0.1f);
        Invoke(nameof(FindNearestResFields), 0.25f);
    }

    public void TakeResource(ResourceType resourceType_)
    {
        if (myPlayer == null) SetPlayer();

        if(resourceType_ == ResourceType.Ore)
        {
            myPlayer.ore += 8;   
        }
        else if (resourceType_ == ResourceType.Gas)
        {
            myPlayer.gas += 8;
        }
    }

    public virtual void SetPlayer()
    {
        myPlayer = FindPlayerByNumber(playerNumber);
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.transform.GetComponent<UnitAI>())
        {
            UnitAI u = collision.transform.GetComponent<UnitAI>();
            if (u.isCanGather)
            {
                GathererCollision(u);
            }
        }
    }
    public void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.transform.GetComponent<UnitAI>())
        {
            UnitAI u = collision.transform.GetComponent<UnitAI>();
            if(u.isCanGather)
            {
                GathererCollision(u);
            }
        }
    }

    public void GathererCollision(UnitAI g)
    {
        print("GathererCollision");

        if (g.playerNumber == playerNumber)
        {
            if (g.isCanGather)
            {
                if(g.nowOrder.moveTarget == transform)
                {
                    print("target is me!");
                    g.FinishOrder(true);
                    if (g.resourceInHands != ResourceType.None)
                    {
                        if (g.lastResField != null)
                            g.AddOrder(UnitOrder.OrderType.Gather, g.lastResField.transform.position, g.lastResField);
                        else
                            g.FindNearestResourceField(g.resourceInHands);
                        TakeResource(g.resourceInHands);
                        g.resourcesInHandsSprites[(int)g.resourceInHands - 1].SetActive(false);
                        g.resourceInHands = ResourceType.None;
                    }
                }
            }
        }
    }

    public void FindNearestResFields()
    {
        ResourceField[] allResourceFields = GameManager.instance.resourceFields;
        List<ResourceField> resourceFieldsFound = new List<ResourceField>();

        for(int i = 0; i < allResourceFields.Length; i++)
        {
            float curDst = Vector2.Distance(transform.position, allResourceFields[i].transform.position);
            if(curDst <= 12f)
            {
                resourceFieldsFound.Add(allResourceFields[i]);
            }
        }

        nearestResourceFields = resourceFieldsFound.ToArray();
    }
}

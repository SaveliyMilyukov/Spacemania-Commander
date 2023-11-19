using UnityEngine;

public class ResourceGatherBuilding : Building
{
    [SerializeField] ResourceType resourceType;
    public UnitAI unitIn;
    [Space(5)]
    [SerializeField] float timeToEndGather = 3f;
    [SerializeField] float gatherTime = 3f;

    public override void Awake()
    {
        base.Awake();
        Invoke(nameof(HideSpritePart), 0.1f);
    }

    public override void Update()
    {
        base.Update();

        if(unitIn != null)
        {
            timeToEndGather -= Time.deltaTime;
            if(timeToEndGather <= 0)
            {               
                unitIn.gameObject.SetActive(true);
                unitIn.lastResField = placedOn.transform;
                unitIn.ClearOrders(true);
                unitIn.FindNearestResourceStorage();

                unitIn = null;
                timeToEndGather = gatherTime;
            }
        }
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.GetComponent<UnitAI>())
        {
            UnitAI u = collision.GetComponent<UnitAI>();
            if (u.playerNumber == playerNumber)
            {
                if (u.isCanGather)
                {
                   if(!u.nowOrder.isNull)
                    {
                        if (u.nowOrder.moveTarget == transform ||
                           u.nowOrder.moveTarget == placedOn.transform)
                        {
                            if(u.resourceInHands != resourceType)
                            {
                                if(unitIn == null)
                                {
                                    unitIn = u;
                                    u.lastResField = placedOn.transform;
                                    u.resourceInHands = resourceType;
                                    u.resourcesInHandsSprites[(int)u.resourceInHands - 1].SetActive(true);
                                    u.gameObject.SetActive(false);
                                    timeToEndGather = gatherTime;
                                }
                            }
                            else
                            {
                                u.FindNearestResourceStorage();
                            }
                        }
                    }
                }
            }
        }
    }

    public void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.transform.GetComponent<UnitAI>())
        {
            UnitAI u = collision.transform.GetComponent<UnitAI>();
            if (u.playerNumber == playerNumber)
            {
                if (u.isCanGather)
                {
                    if (!u.nowOrder.isNull)
                    {
                        if (u.nowOrder.moveTarget == transform ||
                            u.nowOrder.moveTarget == placedOn.transform)
                        {
                            if (u.resourceInHands != resourceType)
                            {
                                if (unitIn == null)
                                {
                                    unitIn = u;
                                    u.lastResField = placedOn.transform;
                                    u.resourceInHands = resourceType;
                                    u.resourcesInHandsSprites[(int)u.resourceInHands - 1].SetActive(true);
                                    u.gameObject.SetActive(false);
                                    timeToEndGather = gatherTime;
                                }
                            }
                            else
                            {
                                u.FindNearestResourceStorage();
                            }
                        }
                    }
                }
            }
        }
    }

    public void HideSpritePart()
    {
        if (placedOn != null)
        {
            placedOn.spritePart.SetActive(false);
        }
    }

    public override void Die()
    {
        if (placedOn != null)
        {
            placedOn.spritePart.SetActive(false);
        }
        base.Die();
    }
}

using UnityEngine;

public class ResourceStorage : Building
{
    PlayerCommander myPlayer;

    public override void Awake()
    {
        base.Awake();
        Invoke(nameof(SetPlayer), 0.1f);
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

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<UnitAI>())
        {
            UnitAI u = collision.GetComponent<UnitAI>();
            if(u.playerNumber == playerNumber)
            {            
                if(u.isCanGather)
                {
                    if(u.resourceInHands != ResourceType.None)
                    {
                        u.FinishOrder(true);
                        u.AddOrder(UnitOrder.OrderType.Gather, u.lastResField.transform.position, u.lastResField);
                        TakeResource(u.resourceInHands);
                        u.resourcesInHandsSprites[(int)u.resourceInHands - 1].SetActive(false);
                        u.resourceInHands = ResourceType.None;
                    }    
                }
            }
        }
    }
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.GetComponent<UnitAI>())
        {
            UnitAI u = collision.transform.GetComponent<UnitAI>();
            if (u.playerNumber == playerNumber)
            {
                if (u.isCanGather)
                {
                    if (u.resourceInHands != ResourceType.None)
                    {
                        u.FinishOrder(true);
                        u.AddOrder(UnitOrder.OrderType.Gather, u.lastResField.transform.position, u.lastResField);
                        TakeResource(u.resourceInHands);
                        u.resourcesInHandsSprites[(int)u.resourceInHands - 1].SetActive(false);
                        u.resourceInHands = ResourceType.None;
                    }
                }
            }
        }
    }
}

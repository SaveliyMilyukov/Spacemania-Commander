using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XagLarva : UnitAI
{
    public bool isMutating = false;
    [Space(5)]
    public int unitCreatingIndex = -1;
    public XagLarvaUnitMutation[] unitsCanMutate;
    [Space(5)]
    public float timeToMutate = 10;
    [Space(5)]
    [SerializeField] GameObject eggSprite;

    public override void Update()
    {
        base.Update();

        if(isMutating)
        {
            moveSpeed = 0;

            timeToMutate -= Time.deltaTime;
            if(timeToMutate <= 0)
            {
                EndMutation();
            }
        }
    }

    public bool TryToStartMutation(int unitIndex_)
    {
        if (isMutating) return false;

        PlayerCommander pl = FindPlayerByNumber(playerNumber);
        if (pl.CheckPrice(unitsCanMutate[unitIndex_].unitPrice))
        {
            unitCreatingIndex = unitIndex_;
            isMutating = true;
            timeToMutate = unitsCanMutate[unitIndex_].mutationTime;
            pl.DecreaseResources(unitsCanMutate[unitIndex_].unitPrice);

            eggSprite.SetActive(true);

            health += 100;
            healthMax += 100;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void EndMutation()
    {
        isMutating = false;
        timeToMutate = 1;
        Unit u = Instantiate(unitsCanMutate[unitCreatingIndex].unitPrefab, transform.position, Quaternion.identity).GetComponent<Unit>();
        u.playerNumber = playerNumber;

        PlayerController pl = FindObjectOfType<PlayerController>();
        if(pl.playerNumber == playerNumber)
        {
            if(pl.unitsControlling.Contains(this))
            {
                pl.unitsControlling.Add(u);
                pl.unitsControlling.Remove(this);
            }
            u.GetComponent<UnitAI>().AddOrder(nowOrder.orderType, nowOrder.movePosition, nowOrder.moveTarget, nowOrder.buildingIndex);
        }

        Die();

        this.enabled = false;
    }

    public void CancelMutation()
    {
        ResourcePrice newPrice = new ResourcePrice(unitsCanMutate[unitCreatingIndex].unitPrice.orePrice / 2, 
                                                    unitsCanMutate[unitCreatingIndex].unitPrice.gasPrice / 2, 
                                                    unitsCanMutate[unitCreatingIndex].unitPrice.limitPrice);
        FindPlayerByNumber(playerNumber).ReturnResourcesByPrice(newPrice);

        Die();
    }

    public override void Die()
    {
        if (myPlayer.isBot) myPlayer.GetComponent<PlayerBot>().larvas.Remove(this);
        base.Die();
    }

    public override void FindMyPlayer()
    {
        base.FindMyPlayer();
        if(myPlayer.isBot) FindPlayerByNumber(playerNumber).GetComponent<PlayerBot>().larvas.Add(this);
    }
}

[System.Serializable]
public class XagLarvaUnitMutation
{
    public GameObject unitPrefab;
    public ResourcePrice unitPrice;
    [Space(5)]
    public float mutationTime = 10;
}

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

    public void TryToStartMutation(int unitIndex_)
    {
        if (isMutating) return;

        PlayerCommander pl = FindPlayerByNumber(playerNumber);
        if (pl.CheckPrice(unitsCanMutate[unitIndex_].unitPrice))
        {
            unitCreatingIndex = unitIndex_;
            isMutating = true;
            timeToMutate = unitsCanMutate[unitIndex_].mutationTime;
            pl.DecreaseResources(unitsCanMutate[unitIndex_].unitPrice);

            eggSprite.SetActive(true);
        }
    }

    public void EndMutation()
    {
        isMutating = false;
        timeToMutate = 1;
        Unit u = Instantiate(unitsCanMutate[unitCreatingIndex].unitPrefab, transform.position, Quaternion.identity).GetComponent<Unit>();
        u.playerNumber = playerNumber;

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
}

[System.Serializable]
public class XagLarvaUnitMutation
{
    public GameObject unitPrefab;
    public ResourcePrice unitPrice;
    [Space(5)]
    public float mutationTime = 10;
}

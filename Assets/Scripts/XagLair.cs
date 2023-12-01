using UnityEngine;
using UnityEngine.SceneManagement;

public class XagLair : ResourceStorage
{
    [Header("Xag Lair")]
    public int larvas = 0;
    [SerializeField] private int larvasMax = 4;
    [SerializeField] private GameObject[] larvaEggs;
    [Space(5)]
    [SerializeField] private float eggSpawnTime = 7f;
    [SerializeField] private float timeToSpawnNextEgg = 7f;

    public override void Awake()
    {
        base.Awake();

        RefreshEggsSprite();
        timeToSpawnNextEgg = eggSpawnTime;     
    }
   
    public override void Update()
    {
        base.Update();

        if(larvas < larvasMax)
        {
            timeToSpawnNextEgg -= Time.deltaTime;

            if(timeToSpawnNextEgg <= 0)
            {
                timeToSpawnNextEgg = eggSpawnTime;
                larvas++;
                RefreshEggsSprite();
            }
        }
    }

    private void RefreshEggsSprite()
    {
        for (int i = 0; i < larvaEggs.Length; i++)
        {
            if (i < larvas)
            {
                larvaEggs[i].SetActive(true);
            }
            else
            {
                larvaEggs[i].SetActive(false);
            }
        }
    }

    public override void OrderToBuildUnit(int unitIndex_)
    {
        if (myPlayer == null) FindMyPlayer();

        if (isBuildingUnit) return;
        if (larvas <= 0 || myPlayer.ore < units[0].unitPrice.orePrice) return;
        larvas--;
        RefreshEggsSprite();

        base.OrderToBuildUnit(unitIndex_);
    }
    public void OnDrawGizmosSelected()
    {
        if (attack.hitsByAttackCount > 0)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attack.attackDistance);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, mySize);
    }

    public override void Die()
    {
        PlayerController pl = FindObjectOfType<PlayerController>();

        if (playerNumber == pl.playerNumber)
        {
            int lairsCount = 0;
            for(int i = 0; i < pl.constructions.Count; i++)
            {
                if (pl.constructions[i] == null) continue;
                if (pl.constructions[i] == this) continue;

                if(pl.constructions[i].GetComponent<XagLair>())
                {
                    lairsCount++;
                }
            }

            Debug.Log("LocalPlayer's lairs was defeated. Lairs count: " + lairsCount);
            if(lairsCount <= 0)
            {
                pl.Defeat();
                return;
            }
        }
        else
        {
            int lairsCount = 0;
            XagLair[] lairs = FindObjectsOfType<XagLair>();
            for (int i = 0; i < lairs.Length; i++)
            {
                if (lairs[i] == null) continue;
                if (lairs[i] == this) continue;

                if(lairs[i].playerNumber != pl.playerNumber)
                {
                    lairsCount++;          
                }    
            }

            if (lairsCount <= 0)
            {
                pl.Win();
                return;
            }
        }

        base.Die();
    }
}

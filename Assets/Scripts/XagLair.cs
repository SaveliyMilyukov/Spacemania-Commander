using UnityEngine;

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
}

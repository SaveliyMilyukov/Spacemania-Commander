using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{

    [Header("General (Unit)")]
    public bool isInCameraView = false; // В поле зрения камеры?
    [Space(5)]
    public Sprite unitIcon;
    [Space(5)]
    public int playerNumber; // playerNumber (номер) игрока, которому принадлежит данный юнит
    public int unitID = 0; // ID (номер) юнита в игре (Не уникальный на каждого. К примеру у двух пехотинцев будет одинаковый unitID)
    public int controlPanelIndex = 0; // Index (номер) контольной панели (Это та, что справа внизу)
    [Space(3)]
    public SpriteRenderer[] marksOfDifference; // Цветные пометки для различия юнитов, кто чей
    [Space(5)]
    public bool isDamageCanBeTaken = true; // Может ли эта боевая единица получать урое
    [Space(3)]
    public int health = 100; // Текущее здоровье
    public int healthMax = 100; // Максимальное здоровье
    public Image healthBar;
    [Space(5)]
    public EnemyDetector myEnemyDetector;
    [Space(5)]
    public UnitAttack attack;
    public Unit attackTarget;
    [Space(5)]
    [SerializeField] private GameObject controlOutline; // Обводка при выделении юнита
    

    [SerializeField] PlayerCommander localPlayer;

    [HideInInspector] public PlayerCommander myPlayer;
    [HideInInspector] public UnitAI myUnitAI;
    [HideInInspector] public Building myBuilding;

    public virtual void Awake()
    {
        myUnitAI = GetComponent<UnitAI>();
        myBuilding = GetComponent<Building>();

        if(myEnemyDetector == null)
        {
            for(int i = 0; i < transform.childCount; i++)
            {
                if(transform.GetChild(i).GetComponent<EnemyDetector>())
                {
                    myEnemyDetector = transform.GetChild(i).GetComponent<EnemyDetector>();
                    break;
                }
            }
        }

        if(myEnemyDetector != null)
        {
            myEnemyDetector.myUnit = this;
        }

        if (GameManager.instance == null) GameManager.instance = FindObjectOfType<GameManager>();

        GameManager.instance.UpdateAllUnits();

        FindLocalPlayer();
        Invoke(nameof(FindMyPlayer), 0.05f);
        if(marksOfDifference.Length > 0) Invoke(nameof(SetColor), 0.175f);
    }

    public virtual void Update()
    {
        if(health <= 0)
        {
            Die();
            return;
        }
        if(healthBar != null)
        {
            float h = health, hM = healthMax;
            healthBar.fillAmount = h / hM;
            if (h / hM == 1) healthBar.transform.parent.gameObject.SetActive(false);
            else healthBar.transform.parent.gameObject.SetActive(true);
        }

        if(attack.isAttacked) // Если уже совершил атаку
        {
            attack.timeToNextAttack -= Time.deltaTime;
            if(attack.timeToNextAttack <= 0)
            {
                attack.timeToNextAttack = attack.timeBtwAttacks;
                attack.isAttacked = false;
            }
        }
        else if(attack.isHited) // Если совершил попадение
        {
            attack.timeToNextHit -= Time.deltaTime;
            if(attack.timeToNextHit <= 0)
            {
                attack.timeToNextHit = attack.timeBtwHits;
                attack.isHited = false;
            }
        }
        else if(attack.currentHitsCount < attack.hitsByAttackCount) // Если еще не совершил попадение или сделал не всё количество
        {
            if(attackTarget != null)
            {
                if(Vector2.Distance(transform.position, attackTarget.transform.position) < attack.attackDistance)
                {
                    AttackSomeone(attackTarget);
                }
            }
        }
            
    }

    public virtual void FindLocalPlayer()
    {
        localPlayer = FindObjectOfType<PlayerController>();
    }
    public virtual void FindMyPlayer()
    {
        myPlayer = FindPlayerByNumber(playerNumber);
    }

    public static PlayerCommander FindPlayerByNumber(int number_)
    {
        PlayerCommander[] players = FindObjectsOfType<PlayerCommander>();
        int plIndexInArray = -1;
        for(int i = 0; i < players.Length; i++)
        {
            if (players[i].playerNumber == number_)
            {
                plIndexInArray = i;
                break;
            }
        }

        if (plIndexInArray != -1)
            return players[plIndexInArray];
        else
            return null;
    }


    public virtual void AttackSomeone(Unit target_)
    {
        if (!target_.isDamageCanBeTaken) return;

        attack.isHited = true;
        attack.timeToNextHit = attack.timeBtwHits;
        attack.currentHitsCount++;
        target_.TakeDamage(attack.damageByOneHit);

        if(attack.currentHitsCount == attack.hitsByAttackCount)
        {
            attack.currentHitsCount = 0;
            attack.isAttacked = true;
            attack.isHited = false;
        }
    }

    public virtual void TakeDamage(int damage_)
    {
        if (!isDamageCanBeTaken) return;
        health -= damage_;
    }

    public virtual void Die()
    {
        myPlayer.UpdateUnits();
        Destroy(gameObject, 0.05f);
    }

    public virtual void SetControlOutline(bool state_)
    {
        controlOutline.SetActive(state_);
    }

    public virtual void SetColor()
    {
        if (marksOfDifference.Length <= 0) return;
        if (playerNumber < 0 || playerNumber >= GameManager.instance.playerColors.Length) return;

        for(int i = 0; i < marksOfDifference.Length; i++)
        {
            marksOfDifference[i].color = GameManager.instance.playerColors[playerNumber];
        }
    }

    public void OnBecameVisible()
    {
        isInCameraView = true;
    }

    public void OnBecameInvisible()
    {
        isInCameraView = false;
    }

    public void OnDrawGizmosSelected()
    {
        if(attack != null)
        {
            Gizmos.DrawWireSphere(transform.position, attack.attackDistance);
        }
    }
}

[System.Serializable]
public class UnitAttack
{
    public string attackName = "Attack";
    [Space(5)]
    public int damageByOneHit = 7; // Урон от одного попадания (удара, попадения пули и т.п.)
    public int hitsByAttackCount = 1; // Кол-во попадений (ударов/выстрелов)
    public int currentHitsCount = 0;
    public float timeBtwHits = 0.2f; // Время между попадениями (ударами/выстрелами)
    public float timeToNextHit = 0.2f;
    [Space(3)]
    public bool isHited = false;
    [Space(5)]
    public float timeBtwAttacks = 1f; // Время между атаками
    // Допустим в одной атаке 2 удара, сначала производятся эти два удара, потом идет таймер на перезарядку всей атаки (тоесть обоих ударов)
    public float timeToNextAttack = 1f;
    public bool isAttacked = false;
    [Space(5)]
    public float attackDistance = 2f;
}
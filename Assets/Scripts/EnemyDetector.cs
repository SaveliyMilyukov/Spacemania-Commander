using System.Collections.Generic;
using UnityEngine;

public class EnemyDetector : MonoBehaviour
{
    //public LayerMask detectionMask;
    public List<Unit> enemiesDetected;
    public Unit myUnit;
    [Space(5)]
    [SerializeField] bool drawAttention = true;

    private void Awake()
    {
        if(!GetComponent<Collider2D>())
        {
            Debug.LogError(gameObject.name + " 's <EnemyDetector>() haven't <Collider2D>()! Enabled = false!");
            enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.transform.GetComponent<Unit>())
        {
            Unit u = collision.transform.GetComponent<Unit>();
            if (!CheckAlliance(myUnit, u))
            {
                enemiesDetected.Add(u);
                CheckNullUnits();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.GetComponent<Unit>())
        {
            Unit u = collision.transform.GetComponent<Unit>();
            if (enemiesDetected.Contains(u))
            {
                enemiesDetected.Remove(u);
                CheckNullUnits();
            }
        }
    }

    void CheckNullUnits()
    {
        for(int i = 0; i < enemiesDetected.Count; i++)
        {
            if(enemiesDetected[i] == null)
            {
                enemiesDetected.Remove(enemiesDetected[i]);
                i--;
                continue;
            }
        }
    }

    // Сравнить двух юнитов, и проверить, враждебны ли они друг ко другу
    public static bool CheckAlliance(Unit first_, Unit second_)
    {
        bool result = false; // Дружелюбны ли они друг ко другу (в одной команде или принадлежат одному и тому же игроку)

        if (first_.myPlayer == null) first_.myPlayer = Unit.FindPlayerByNumber(first_.playerNumber);
        if (second_.myPlayer == null) second_.myPlayer = Unit.FindPlayerByNumber(second_.playerNumber);

        if (first_.playerNumber == second_.playerNumber) result = true; // Если оба юнита принадлежат одному игроку,
        else if (first_.myPlayer.playerTeam != -1 && second_.myPlayer.playerTeam != -1) // Иначе: Если игроки обоих юнитов состоят в командах
        {
            if (first_.myPlayer.playerTeam == second_.myPlayer.playerTeam) result = true; // Если владельцы юнитов в одной команде - Дружелюбность = true;
            else result = false; // Иначе: если не в одной команде, то юниты враждебны друг ко другу - Дружелюбность = false;
        }
        else result = false; // Игроки не состоят в командах - соответственно нет смысла проверять, в одной ли они команде. Дружелюбность = false;

        return result;
    }

    public static bool CheckAlliance(PlayerCommander first_, Unit second_)
    {
        bool result = false; // Дружелюбны ли они друг ко другу (в одной команде или принадлежат одному и тому же игроку)

        if (second_.myPlayer == null) second_.myPlayer = Unit.FindPlayerByNumber(second_.playerNumber);

        if (first_.playerNumber == second_.playerNumber) result = true; // Если оба юнита принадлежат одному игроку,
        else if (first_.playerTeam != -1 && second_.myPlayer.playerTeam != -1) // Иначе: Если игроки обоих юнитов состоят в командах
        {
            if (first_.playerTeam == second_.myPlayer.playerTeam) result = true; // Если владельцы юнитов в одной команде - Дружелюбность = true;
            else result = false; // Иначе: если не в одной команде, то юниты враждебны друг ко другу - Дружелюбность = false;
        }
        else result = false; // Игроки не состоят в командах - соответственно нет смысла проверять, в одной ли они команде. Дружелюбность = false;

        return result;
    }

    public void OnDrawGizmosSelected()
    {
        if (!drawAttention) return;

        if(!GetComponent<Collider2D>())
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, 2f);
        }
        else if(!GetComponent<Collider2D>().isTrigger)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 2f);
        }
    }
}

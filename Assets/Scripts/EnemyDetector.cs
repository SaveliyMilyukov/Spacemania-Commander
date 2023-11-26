using System.Collections.Generic;
using UnityEngine;

public class EnemyDetector : MonoBehaviour
{
    public LayerMask detectionMask;
    public List<Unit> enemiesDetected;
    public Unit myUnit;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.transform.GetComponent<Unit>())
        {
            Unit u = collision.transform.GetComponent<Unit>();
            if (!CheckAlliance(myUnit, u))
            {
                enemiesDetected.Add(u);
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
            }
        }
    }

    // Сравнить двух юнитов, и проверить, враждебны ли они друг ко другу
    public static bool CheckAlliance(Unit first_, Unit second_)
    {
        bool result = false; // Дружелюбны ли они друг ко другу (в одной команде или принадлежат одному и тому же игроку)

        if (first_.playerNumber == second_.playerNumber) result = true; // Если оба юнита принадлежат одному игроку,
        else if (first_.myPlayer.playerTeam != -1 && second_.myPlayer.playerTeam != -1) // Иначе: Если игроки обоих юнитов состоят в командах
        {
            if (first_.myPlayer.playerTeam == second_.myPlayer.playerTeam) result = true; // Если владельцы юнитов в одной команде - Дружелюбность = true;
            else result = false; // Иначе: если не в одной команде, то юниты враждебны друг ко другу - Дружелюбность = false;
        }
        else result = false; // Игроки не состоят в командах - соответственно нет смысла проверять, в одной ли они команде. Дружелюбность = false;

        return result;
    }
}

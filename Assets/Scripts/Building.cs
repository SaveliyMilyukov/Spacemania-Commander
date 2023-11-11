using System.Collections.Generic;
using UnityEngine;

public class Building : Unit
{
    [Header("Building")]
    [Header("Build Mode")]
    public Sprite cursorSprite;
    public float buildBlockDistance = 1f;
    public float buildingTime = 5f;
    public ResourcePrice buildingPrice;
    [Header("Rally Point")]
    [SerializeField] bool generateRallyPointOnAwake = false;
    [Space(5)]
    [SerializeField] private Vector2 rallyPoint; // Точка сбора, куда будут отправляться нанятые юниты
    [SerializeField] bool enableRallyPointLine = true;
    [SerializeField] LineRenderer rallyPointLine;
    [Space(5)]
    [SerializeField] private float spawnOffset; // На каком расстоянии должен появиться юнит (расссчитывается по размеру здания в методе Awake())
    // Сторона же, где появится юнит, определяется по RallyPoint'у.
    [Space(5)]
    [Header("Production")]
    public bool isBuildingUnit = false;
    [SerializeField] private int unitCreatingIndex = 0;
    public CreateUnit[] units;
    [SerializeField] private float timeToCreateUnit = 10f;
    

    public override void Awake()
    {
        base.Awake();

        // Определение расстояния спавна
        if (GetComponent<CircleCollider2D>())
        {
            spawnOffset = 1f * GetComponent<CircleCollider2D>().radius;    
        }
        else if (GetComponent<BoxCollider2D>())
        {
            spawnOffset = 1f * GetComponent<BoxCollider2D>().size.magnitude;
        }

        // RallySpawn Line
        if (!enableRallyPointLine)
        {
            if(rallyPointLine != null)
            {
                rallyPointLine.positionCount = 0;
            }
        }
        else if(generateRallyPointOnAwake)
        {
            EditRallyPoint(transform.position + Vector3.down * spawnOffset);
            SetRallyPointVision(false);
        }
    }

    public override void Update()
    {
        base.Update();

        if(isBuildingUnit)
        {
            timeToCreateUnit -= Time.deltaTime;
            if(timeToCreateUnit <= 0)
            {
                timeToCreateUnit = 0.05f;
                isBuildingUnit = false;
                SpawnUnit();
            }
        }
    }

    public virtual void EditRallyPoint(Vector2 newRallyPoint_)
    {
        rallyPoint = newRallyPoint_;

        if(enableRallyPointLine)
        {
            rallyPointLine.positionCount = 2;
            rallyPointLine.SetPosition(0, transform.position);
            rallyPointLine.SetPosition(1, newRallyPoint_);
        }
    }

    public virtual void SetRallyPointVision(bool state_)
    {
        if (rallyPointLine == null) return;

        if(state_)
        {
            rallyPointLine.sortingOrder = 2;
        }
        else
        {
            rallyPointLine.sortingOrder = -999;
        }
    }

    public virtual void SpawnUnit()
    {
        // Формула: позиция здания + направление (позиция точки сбора - позиция здания [после нормализовать]) * расстояние спавна (spawnOffset)
        Vector2 spawningPosition = (Vector2)transform.position + (rallyPoint - (Vector2)transform.position).normalized * spawnOffset / 2;

        Unit u = Instantiate(units[unitCreatingIndex].unitPrefab, spawningPosition, Quaternion.identity).GetComponent<Unit>();
        u.playerNumber = playerNumber;
        u.GetComponent<UnitAI>().AddOrder(UnitOrder.OrderType.Move, rallyPoint, null);
        PlayerController.localPlayer.UpdateUnits();
    }

    public virtual void OrderToBuildUnit(int unitIndex_)
    {
        // Если у здания нет списка юнитов, которые оно может производить
        // Или индекс юнита (unitIndex_) выходит за рамки этого списка
        if (units.Length <= 0 || unitIndex_ >= units.Length || unitIndex_ < 0) return;

        // Проверка цены юнита
        if (PlayerController.localPlayer.ore < units[unitIndex_].unitPrice.orePrice || // Если не хватант руды
            PlayerController.localPlayer.gas < units[unitIndex_].unitPrice.gasPrice || // Или не хватает газа
            PlayerController.localPlayer.limitMax - PlayerController.localPlayer.limitCurrent < units[unitIndex_].unitPrice.limitPrice) // Или нет доступного лимита
        {
            return; // Выходим из функции (соответственно никого не заказываем)
        }

        // Проверка, не строится ли сейчас юнит
        if (isBuildingUnit) return; // Выходим из функции, если какой-либо юнит уже создается (соответственно никого не заказываем)

        // Отнимаем цену на юнита
        PlayerController.localPlayer.ore -= units[unitIndex_].unitPrice.orePrice; // Отнимаем руду
        PlayerController.localPlayer.gas -= units[unitIndex_].unitPrice.gasPrice; // Отнимаем газ
        PlayerController.localPlayer.limitCurrent += units[unitIndex_].unitPrice.limitPrice; // Прибавляем лимит (В общем делаем бо-бо ;D)

        // Нанимаем юнита
        isBuildingUnit = true;
        unitCreatingIndex = unitIndex_;
        timeToCreateUnit = units[unitIndex_].unitBuildingTime;
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, buildBlockDistance);
    }
}

[System.Serializable]
public class CreateUnit
{
    [SerializeField] private string unitName = "New Unit";
    [Space(5)]
    public GameObject unitPrefab;
    public float unitBuildingTime = 10f;
    public ResourcePrice unitPrice;
}

[System.Serializable]
public class ResourcePrice
{
    public int orePrice = 50;
    public int gasPrice = 0;
    public int limitPrice = 0;
}
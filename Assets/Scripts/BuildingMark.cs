using UnityEngine;

public class BuildingMark : MonoBehaviour
{
    public int playerNumber = 0;
    [Space(5)]
    public Building building;
    public ResourcePrice buildingPrice;
    public float buildBlockDistance = 1f;
    [Space(5)]
    [SerializeField] private BuildingWorkplace workplacePrefab;
    [Space(5)]
    public SpriteRenderer render;
    [Space(5)]
    public ResourceField placedOn = null;

    bool isBuildingStarted = false;

    private void Update()
    {
        if(placedOn != null)
        {
            if(placedOn.buildingBuildedOn != null ||
                placedOn.buildingMarkOn != null && placedOn.buildingMarkOn != this)
            {
                CancelBuild();
            }
            else
            {
                placedOn.buildingMarkOn = this;
            }
        }
    }

    public void TryToStartBuild(UnitAI builder_)
    {
        // Проверка, можно ли строить
        bool isCanStartBuild = true;
        for(int i = 0; i < GameManager.instance.allUnitsAndBuildingsOnMap.Length; i++)
        {
            if(GameManager.instance.allUnitsAndBuildingsOnMap[i] != builder_)
            {
                if (GameManager.instance.allUnitsAndBuildingsOnMap[i] == null) continue;

                if(building.need == Building.BuildingNeed.OreField && GameManager.instance.allUnitsAndBuildingsOnMap[i].GetComponent<ResourceField>() ||
                    building.need == Building.BuildingNeed.Geyser && GameManager.instance.allUnitsAndBuildingsOnMap[i].GetComponent<ResourceField>())
                {
                    continue;
                }

                float curDst = Vector2.Distance(transform.position, GameManager.instance.allUnitsAndBuildingsOnMap[i].transform.position);
                if(curDst < buildBlockDistance)
                {
                    //Debug.Log("BlockDistance: " + curDst + "/" + buildBlockDistance + " (" + GameManager.instance.allUnitsAndBuildingsOnMap[i].gameObject.name + ")");
                    isCanStartBuild = false;
                    break;
                }
            }
        }

        //Debug.Log(gameObject.name + " isCanStartBuild: " + isCanStartBuild);
        if(isCanStartBuild)
        {
            isBuildingStarted = true;

            Vector3Int cellPos = GameManager.instance.groundTilemap.WorldToCell(transform.position);
            Vector3 constructionPosition = GameManager.instance.groundTilemap.CellToWorld(cellPos);

            BuildingWorkplace work = Instantiate(workplacePrefab, constructionPosition, Quaternion.identity);
            work.buildingPrefab = building.gameObject;
            work.playerNumber = playerNumber;
            work.SetBuildTime(building.buildingTime);
            work.placedOn = placedOn;

            PlayerCommander pl = Unit.FindPlayerByNumber(playerNumber);

            if (pl.unitsControlling.Contains(builder_)) pl.unitsControlling.Remove(builder_);
            if (pl.unitsAndConstructions.Contains(builder_)) pl.unitsAndConstructions.Remove(builder_);
            if (pl.units.Contains(builder_)) pl.units.Remove(builder_);
            Destroy(builder_.gameObject);
            pl.UpdateUnits();
            PlayerController.localPlayer.UpdateUnitsControllingIcons();
        }
        else
        {
            CancelBuild();
        }

        Destroy(gameObject);
        enabled = false;
    }

    public void CancelBuild()
    {
        if (isBuildingStarted) return;

        Unit.FindPlayerByNumber(playerNumber).ReturnResourcesByPrice(buildingPrice);
        Destroy(gameObject);
        enabled = false;
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, buildBlockDistance);
    }
}

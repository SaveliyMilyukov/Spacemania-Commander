using UnityEngine;

public class ResourceField : Unit
{
    [Header("(Resource Field)")]
    public UnitAI busedBy;
    public ResourceType resourceType = ResourceType.Ore;
    public Vector2Int positionInGrid;
    [Space(5)]
    public GameObject spritePart;

    public override void Awake()
    {
        base.Awake();

        GameManager.instance = FindObjectOfType<GameManager>();
        positionInGrid = (Vector2Int)GameManager.instance.groundTilemap.WorldToCell(transform.position);
    }
}

public enum ResourceType
{
    None, Ore, Gas
}
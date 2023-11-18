using UnityEngine;

public class ResourceField : Unit
{
    [Header("(Resource Field)")]
    public UnitAI busedBy;
    public ResourceType resourceType = ResourceType.Ore;
}

public enum ResourceType
{
    None, Ore, Gas
}
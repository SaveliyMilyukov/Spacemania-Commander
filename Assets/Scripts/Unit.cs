using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("General")]
    public int health = 1;
    public int healthMax = 1;

    public virtual void Update()
    {
        if(health <= 0)
        {
            Destroy(gameObject);
            return;
        }
    }
}

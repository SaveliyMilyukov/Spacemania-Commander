using UnityEngine;

public class MMM : MonoBehaviour
{
    void Awake()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(0).SetParent(transform.parent);
        Destroy(gameObject);
    }
}

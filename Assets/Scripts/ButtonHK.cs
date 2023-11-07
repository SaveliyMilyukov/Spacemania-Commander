using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHK : MonoBehaviour
{
    [SerializeField] KeyCode hotKey;
    EventTrigger eventTrigger;

    private void Awake()
    {
        eventTrigger = GetComponent<EventTrigger>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(hotKey))
        {
            eventTrigger.OnPointerClick(null);
        }
    }
}

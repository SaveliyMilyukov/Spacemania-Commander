using UnityEngine;
using UnityEngine.UI;

public class UnitButton : MonoBehaviour
{
    public bool isUsingOrder = false;
    public UnitOrder.OrderType orderType;
    public bool isStopButton = false; 
    [Space(5)]
    [SerializeField] private PlayerController player;
    [Space(5)]
    public Image icon;
    [Space(5)]
    public Sprite standartSprite;
    public Sprite lightSprite;

    private void Update()
    {
        if(isUsingOrder)
        {
            icon.sprite = lightSprite;
        }
        else
        {
            icon.sprite = standartSprite;
        }

        if(icon.sprite == null)
        {
            icon.color = Color.clear;
        }   
        else
        {
            icon.color = Color.white;
        }
    }
}

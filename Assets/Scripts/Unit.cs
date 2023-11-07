using UnityEngine;

public class Unit : MonoBehaviour
{

    [Header("General (Unit)")]
    public bool isInCameraView = false; // В поле зрения камеры?
    [Space(5)]
    public Sprite unitIcon;
    [Space(5)]
    public int playerNumber; // playerNumber (номер) игрока, которому принадлежит данный юнит
    public int unitID = 0; // ID (номер) юнита в игре (Не уникальный на каждого. К примеру у двух пехотинцев будет одинаковый unitID)
    public int controlPanelIndex = 0; // Index (номер) контольной панели (Это та, что справа внизу)
    [Space(5)]
    public int health = 1; // Текущее здоровье
    public int healthMax = 1; // Максимальное здоровье
    [Space(5)]
    [SerializeField] private GameObject controlOutline; // Обводка при выделении юнита
    

    [SerializeField] PlayerCommander localPlayer;

    [HideInInspector] public UnitAI myUnitAI;
    [HideInInspector] public Building myBuilding;

    public virtual void Awake()
    {
        myUnitAI = GetComponent<UnitAI>();
        myBuilding = GetComponent<Building>();

        FindLocalPlayer();
    }

    public virtual void Update()
    {
        if(health <= 0)
        {
            Destroy(gameObject);
            return;
        }
    }

    public virtual void FindLocalPlayer()
    {
        localPlayer = FindObjectOfType<PlayerController>();
    }

    public void SetControlOutline(bool state_)
    {
        controlOutline.SetActive(state_);
    }

    public void OnBecameVisible()
    {
        isInCameraView = true;
    }

    public void OnBecameInvisible()
    {
        isInCameraView = false;
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : PlayerCommander
{
    [Header("Camera")]
    [SerializeField] private GameObject cameraObjectPrefab;
    [Space(5)]
    [SerializeField] private Transform cameraObject; // Объект камеры (с тегом Player Camera)
    private Camera cam;
    [SerializeField] private float cameraMoveSpeed = 5;
    [Space(5)]
    [SerializeField] private bool cursorOnUpSide = false;
    [SerializeField] private bool cursorOnDownSide = false;
    [SerializeField] private bool cursorOnRightSide = false;
    [SerializeField] private bool cursorOnLeftSide = false;
    [Header("Input")]
    [SerializeField] private bool keyboardAxisRaw = true;
    [SerializeField] private Vector2 keyboardAxis; // W A S D
    [SerializeField] private bool attack = false;
    [Space(5)]
    [SerializeField] private UnitOrder.OrderType orderTypeByChoosedUnit;
    [SerializeField] private UnitButton[] orderButtons;
    [Header("UI")]
    Vector2 resolution;
    [SerializeField] private Transform canvasObject;
    [SerializeField] private Text oreText;
    [SerializeField] private Text gasText;
    [SerializeField] private Text limitText;
    [Space(5)]
    [Header("Units Controlling")]
    [SerializeField] private GameObject[] controlPanels;
    [Space(5)]
    [SerializeField] private GameObject unitIconPrefab;
    [SerializeField] private GameObject[] unitIcons;
    [SerializeField] private Transform unitIconsPlace;
    [Space(5)]
    [SerializeField] private int unitID = 0;
    [SerializeField] private int unitIndex = 0;

    public static PlayerController localPlayer;

    public override void Awake()
    {
        base.Awake();

        localPlayer = this;

        // Получение разрешения экрана
        Rect canvasRect = canvasObject.GetComponent<RectTransform>().rect;
        resolution = new Vector2(canvasRect.width, canvasRect.height);

        // Поиск камеры
        if(cameraObject == null) // Первая проверка на объект камеры
        {
            cameraObject = GameObject.FindGameObjectWithTag("Player Camera").transform;
        }
        if (cameraObject == null) // Вторая проверка на объект камеры
        {
            Vector3 camPos = new Vector3(transform.position.x, transform.position.y, -10);
            cameraObject = Instantiate(cameraObjectPrefab, camPos, Quaternion.identity).transform;
        }

        cameraObject.SetParent(null);
        cameraObject.position = transform.position - Vector3.forward * 10;
        cam = cameraObject.GetChild(0).GetComponent<Camera>();

        // 
    }


    public override void Update()
    {
        base.Update();

        GetInput();
        UpdateUI();

        bool clearButtons = false;
        if (unitsControlling.Count > 0)
        {
            if (unitsControlling[unitIndex].myUnitAI != null)
            {
                orderTypeByChoosedUnit = unitsControlling[unitIndex].myUnitAI.nowOrder.orderType;
                for (int i = 0; i < orderButtons.Length; i++)
                {
                    if (unitsControlling.Count > 0 && !unitsControlling[unitIndex].myUnitAI.nowOrder.isNull && orderButtons[i].orderType == orderTypeByChoosedUnit && !orderButtons[i].isStopButton ||
                        unitsControlling.Count > 0 && unitsControlling[unitIndex].myUnitAI.nowOrder.isNull && orderButtons[i].isStopButton)
                    {
                        orderButtons[i].isUsingOrder = true;
                    }
                    else
                    {
                        orderButtons[i].isUsingOrder = false;
                    }
                }
            }
            else clearButtons = true;
        }
        else clearButtons = true;
        
        if(clearButtons)
        {
            for (int i = 0; i < orderButtons.Length; i++)
            {              
                orderButtons[i].isUsingOrder = false;                
            }
        }
    }

    private void FixedUpdate()
    {
        MoveCamera();
    }

    private void GetInput()
    {
        // Cursor
        Vector2 cursorPosition = Input.mousePosition;
        if (cursorPosition.y <= 1) cursorOnDownSide = true; else cursorOnDownSide = false; // Down
        if (cursorPosition.y >= resolution.y - 1) cursorOnUpSide = true; else cursorOnUpSide = false; // Up
        if (cursorPosition.x <= 1) cursorOnLeftSide = true; else cursorOnLeftSide = false; // Left
        if (cursorPosition.x >= resolution.x - 1) cursorOnRightSide = true; else cursorOnRightSide = false; // Right

        // Keyboard 
        if (keyboardAxisRaw)
        {
            keyboardAxis = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }
        else
        {
            keyboardAxis = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            attack = false;
        }
        else if(Input.GetAxisRaw("Attack") != 0)
        {
            attack = true;
        }
        else if (Input.GetAxisRaw("Stop") != 0)
        {
            for (int i = 0; i < unitsControlling.Count; i++)
            {
                if (unitsControlling[i].myUnitAI != null) unitsControlling[i].myUnitAI.ClearOrders(true);
            }
        }
        else if (Input.GetAxisRaw("Hold Position") != 0)
        {
            for (int i = 0; i < unitsControlling.Count; i++)
            {
                if (unitsControlling[i].myUnitAI != null)
                {
                    if (!Input.GetKey(KeyCode.LeftShift))
                    {
                        unitsControlling[i].myUnitAI.ClearOrders(true);
                    }
                    unitsControlling[i].myUnitAI.AddOrder(UnitOrder.OrderType.HoldPosition, unitsControlling[i].transform.position, null);
                }                 
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab) &&
            unitsControlling.Count > 1)
        {
            bool foundNewUnitID = false;
            for (int i = unitIndex; i < unitsControlling.Count; i++)
            {
                if (unitsControlling[i].unitID != unitsControlling[unitIndex].unitID)
                {
                    foundNewUnitID = true;
                    unitIndex = i;
                    unitID = unitsControlling[i].unitID;
                    ShowControlPanel(unitsControlling[i].controlPanelIndex);
                    break;
                }
            }

            if (!foundNewUnitID)
            {
                unitID = unitsControlling[0].unitID;
                unitIndex = 0;
                ShowControlPanel(unitsControlling[0].controlPanelIndex);
            }

            UpdateUnitsControllingIcons();
        }

        // Mouse
        if (Input.GetKeyDown(KeyCode.Mouse0)) // ЛКМ
        {
            attack = false;
            ChooseNearestUnit();
        }
        if (Input.GetKeyDown(KeyCode.Mouse1)) // ПКМ
        {           
            KeyboadAddOrdersToUnits();                  
        }
    }

    private void MoveCamera()
    {
        Vector2 mouseAxis = Vector2.zero;
#if !UNITY_EDITOR && !UNITY_EDITOR64 && !UNITY_EDITOR_WIN
        if (cursorOnUpSide) mouseAxis += Vector2.up;
        else if (cursorOnDownSide) mouseAxis += Vector2.down;
        if (cursorOnRightSide) mouseAxis += Vector2.right;
        else if (cursorOnLeftSide) mouseAxis += Vector2.left;
#endif

        Vector2 movingVector = (mouseAxis + keyboardAxis).normalized * cameraMoveSpeed * Time.deltaTime;
        cameraObject.position += (Vector3)movingVector;
    }

    private void UpdateUI()
    {
        oreText.text = ore.ToString();
        gasText.text = gas.ToString();
        limitText.text = limitCurrent + "/" + limitMax;
        if (limitCurrent > limitMax) limitText.color = Color.red; else limitText.color = Color.black;
    }

    private void KeyboadAddOrdersToUnits()
    {
        bool replaceMode = true;
        if (Input.GetKey(KeyCode.LeftShift)) replaceMode = false;
        UnitOrder.OrderType order = UnitOrder.OrderType.Move;

        // Поиск юнита (если игрок нажал на него, то нужно будет сделать приказ Follow или Attack)
        Vector2 cursorPosition = cam.ScreenToWorldPoint(Input.mousePosition);
        Unit nearestUnit = GameManager.instance.allUnitsAndBuildingsOnMap[0];
        float nearestDst = Vector2.Distance(cursorPosition, nearestUnit.transform.position);
        for (int i = 0; i < unitsAndConstructions.Count; i++)
        {
            float curDst = Vector2.Distance(cursorPosition, unitsAndConstructions[i].transform.position);
            if (curDst < nearestDst)
            {
                nearestDst = curDst;
                nearestUnit = unitsAndConstructions[i];
            }
        }

        bool isUnitUnderTheCursor = false;
        if (nearestUnit.GetComponent<CircleCollider2D>())
        {
            if (nearestDst < 1f * nearestUnit.GetComponent<CircleCollider2D>().radius)
            {
                isUnitUnderTheCursor = true;
            }
        }
        else if (nearestUnit.GetComponent<BoxCollider2D>())
        {
            if (nearestDst < 1f * nearestUnit.GetComponent<BoxCollider2D>().size.magnitude)
            {
                isUnitUnderTheCursor = true;
            }
        }

        if (isUnitUnderTheCursor)
        {
            if (attack)
            {
                order = UnitOrder.OrderType.Attack;
            }
            else
            {
                order = UnitOrder.OrderType.Follow;
            }
        }
        else if (attack)
        {
            order = UnitOrder.OrderType.MoveAndAttack;
        }
        else
        {
            order = UnitOrder.OrderType.Move;
        }

        AddOrderToUnits(order, cursorPosition, nearestUnit.transform, replaceMode);
        AddRallyPoints(cursorPosition);

        if(replaceMode) attack = false;
    }   
    
    private void ChooseNearestUnit()
    {
        Vector2 cursorPosition = cam.ScreenToWorldPoint(Input.mousePosition); // Считывание положения курсора в мировых координатах

        Unit nearestUnit = unitsAndConstructions[0];
        float nearestDst = Vector2.Distance(cursorPosition, nearestUnit.transform.position);
        for(int i = 0; i < unitsAndConstructions.Count; i++) // Поиск ближайшего юнита к курсору
        {
            float curDst = Vector2.Distance(cursorPosition, unitsAndConstructions[i].transform.position);
            if(curDst < nearestDst)
            {
                nearestDst = curDst;
                nearestUnit = unitsAndConstructions[i];
            }
        }

        bool isUnitUnderTheCursor = false; // Проверка есть ли тот самый ближайший юнит под курсором (только CircleCollider2D и BoxCollider2D)
        if (nearestUnit.GetComponent<CircleCollider2D>()) // Проверка с круговым коллайдером
        {
            if (nearestDst < 1f * nearestUnit.GetComponent<CircleCollider2D>().radius)
            {
                isUnitUnderTheCursor = true;
            }
        }
        else if (nearestUnit.GetComponent<BoxCollider2D>()) // Проверка с коробчатым коллайдером
        {
            if (nearestDst < 0.9f * nearestUnit.GetComponent<BoxCollider2D>().size.magnitude)
            {
                isUnitUnderTheCursor = true;
            }
        }

        if (!isUnitUnderTheCursor) { return; } // Если под курсором нет юнита - выходим из функции

        if(Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.LeftControl)) // Если зажат Shift и НЕ зажат Cntrl
        {
            if (unitsControlling.Contains(nearestUnit) && unitsControlling.Count > 1)
            {
                unitsControlling.Remove(nearestUnit); // Если юнит уже выделен - убираем выделение
                nearestUnit.SetControlOutline(false);
                if (nearestUnit.myBuilding != null) nearestUnit.myBuilding.SetRallyPointVision(false);
            }
            else
            {
                unitsControlling.Add(nearestUnit); // Если юнит еще не выделен - выделяем
                nearestUnit.SetControlOutline(true);
                if (nearestUnit.myBuilding != null) nearestUnit.myBuilding.SetRallyPointVision(true);
            }
        }
        else if(Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftShift)) // Если зажат Ctrl и НЕ зажат Shift
        {
            for (int i = 0; i < unitsControlling.Count; i++)// Снимаем выделение с уже выделенных юнитов
            {
                unitsControlling[i].SetControlOutline(false);
                if (unitsControlling[i].myBuilding != null) unitsControlling[i].myBuilding.SetRallyPointVision(false);
            }
            unitsControlling.Clear();

            for (int i = 0; i < unitsAndConstructions.Count; i++)
            {
                // Если юнит в поле зрения игрока, а также это такой же юнит, как и тот, на которого мы кликнули
                // Пример: мы кликнули на пехотинца, и если в поле зрения есть пехотинец, то он тоже выделяется
                if(unitsAndConstructions[i].isInCameraView && unitsAndConstructions[i].unitID == nearestUnit.unitID) 
                {                  
                    unitsControlling.Add(unitsAndConstructions[i]);
                    unitsAndConstructions[i].SetControlOutline(true);
                    if (unitsAndConstructions[i].myBuilding != null) unitsAndConstructions[i].myBuilding.SetRallyPointVision(true);
                }
            }
        }
        else if(Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl)) // Если зажаты и Shift и Cntrl
        {
            if (!unitsControlling.Contains(nearestUnit)) // Если юнит, на которого игрок, еще не выделен
            {
                unitsControlling.Add(nearestUnit); // Выделяем юнита, на которого кликнул игрок
                nearestUnit.SetControlOutline(true);
                if (nearestUnit.myBuilding != null) nearestUnit.myBuilding.SetRallyPointVision(true);
            }

            for (int i = 0; i < unitsAndConstructions.Count; i++)
            {
                // Если юнит в поле зрения игрока, а также это такой же юнит, как и тот, на которого мы кликнули
                // Пример: мы кликнули на пехотинца, и если в поле зрения есть пехотинец, то он тоже выделяется
                if (unitsAndConstructions[i].isInCameraView && unitsAndConstructions[i].unitID == nearestUnit.unitID)
                {
                    if(!unitsControlling.Contains(unitsAndConstructions[i]))
                    {
                        unitsControlling.Add(unitsAndConstructions[i]);
                        unitsAndConstructions[i].SetControlOutline(true);
                        if (unitsAndConstructions[i].myBuilding != null) unitsAndConstructions[i].myBuilding.SetRallyPointVision(true);
                    }
                }
            }
        }
        else // Если ничего из вышеперечисленного не зажато
        {
            for(int i = 0; i < unitsControlling.Count; i++)// Снимаем выделение с уже выделенных юнитов
            {
                unitsControlling[i].SetControlOutline(false);
                if (unitsControlling[i].myBuilding != null) unitsControlling[i].myBuilding.SetRallyPointVision(false);
            }
            unitsControlling.Clear(); 

            unitsControlling.Add(nearestUnit); // И выделяем только юнита, на которого мы кликнули
            nearestUnit.SetControlOutline(true);
            if (nearestUnit.myBuilding != null) nearestUnit.myBuilding.SetRallyPointVision(true);
        }

        unitID = unitsControlling[0].unitID;
        unitIndex = 0;
        ShowControlPanel(unitsControlling[0].controlPanelIndex);
        UpdateUnitsControllingIcons();
    }

    private void UpdateUnitsControllingIcons()
    {
        for(int i = 0; i < unitIcons.Length; i++)
        {
            Destroy(unitIcons[i]);
        }

        unitIcons = new GameObject[unitsControlling.Count];
        for(int i = 0; i < unitIcons.Length; i++)
        {
            Unit u = unitsControlling[i];
            GameObject newUnitIcon = Instantiate(unitIconPrefab, Vector2.zero, Quaternion.identity);
            newUnitIcon.transform.SetParent(unitIconsPlace);

            Image icon = newUnitIcon.transform.GetChild(0).GetComponent<Image>();
            Image border = newUnitIcon.gameObject.GetComponent<Image>();
            icon.sprite = unitsControlling[i].unitIcon;

            if(u.health > u.healthMax / 3 * 2)
            {
                icon.color = Color.green;
            }
            else if (u.health > u.healthMax / 2)
            {
                icon.color = Color.yellow;
            }
            else if (u.health > u.healthMax / 3)
            {
                icon.color = Color.red + Color.yellow / 2;
            }
            else
            {
                icon.color = Color.red;
            }

            if(unitID == unitsControlling[i].unitID)
            {
                border.color = Color.cyan;
            }
            else
            {
                border.color = Color.green;
            }

            unitIcons[i] = newUnitIcon;
        }
    }

    private void ShowControlPanel(int index_)
    {
        for(int i = 0; i < controlPanels.Length; i++)
        {
            if(i == index_)
            {
                controlPanels[i].SetActive(true);
            }
            else
            {
                controlPanels[i].SetActive(false);
            }
        }
    }

    public void BuildUnit(int unitIndex_)
    {
        if (unitsControlling[unitIndex].myBuilding == null) return;
        Building b = unitsControlling[unitIndex].myBuilding;

        b.OrderToBuildUnit(unitIndex_);
    }
}

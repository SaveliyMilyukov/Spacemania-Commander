using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : PlayerCommander
{
    [Header("Camera")]
    [SerializeField] GameObject cameraObjectPrefab;
    [Space(5)]
    [SerializeField] Transform cameraObject; // Объект камеры (с тегом Player Camera)
    [SerializeField] float cameraMoveSpeed = 5;
    [Space(5)]
    [SerializeField] bool cursorOnUpSide = false;
    [SerializeField] bool cursorOnDownSide = false;
    [SerializeField] bool cursorOnRightSide = false;
    [SerializeField] bool cursorOnLeftSide = false;
    [Header("Input")]
    [SerializeField] bool keyboardAxisRaw = true;
    [SerializeField] Vector2 keyboardAxis; // W A S D
    [Header("UI")]
    Vector2 resolution;
    [SerializeField] Transform canvasObject;
    [SerializeField] Text oreText;
    [SerializeField] Text gasText;
    [SerializeField] Text limitText;

    void Awake()
    {
        Rect canvasRect = canvasObject.GetComponent<RectTransform>().rect;
        resolution = new Vector2(canvasRect.width, canvasRect.height);

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
    }


    void Update()
    {
        GetInput();
        UpdateUI();
    }

    private void FixedUpdate()
    {
        MoveCamera();
    }

    void GetInput()
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
    }

    void MoveCamera()
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

    void UpdateUI()
    {
        oreText.text = ore.ToString();
        gasText.text = gas.ToString();
        limitText.text = limitNow + "/" + limitMax;
        if (limitNow > limitMax) limitText.color = Color.red; else limitText.color = Color.black;
    }   
}

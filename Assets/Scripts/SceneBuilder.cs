using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SceneBuilder : MonoBehaviour
{
    private bool isBuilt = false;

    void Start()
    {
        // Автоматически вызываем BuildScene при запуске (в runtime)
        if (!isBuilt)
        {
            BuildScene();
            isBuilt = true;
        }
    }

    [ContextMenu("Собрать сцену заново")]
    public void BuildScene()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj != this.gameObject) DestroyImmediate(obj);
        }

        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        camObj.tag = "MainCamera";
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
        cam.orthographic = false;
        cam.fieldOfView = 50f;
        camObj.transform.position = new Vector3(0f, 2.2f, -4.5f);
        camObj.transform.rotation = Quaternion.Euler(10f, 0f, 0f);

        GameObject lightObj = new GameObject("Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

        GameObject clawMachinePrefab = Resources.Load<GameObject>("claw_machine");
        if (clawMachinePrefab != null)
        {
            GameObject machineInstance = Instantiate(clawMachinePrefab, Vector3.zero, Quaternion.Euler(-90, 90, 0));
            machineInstance.name = "ClawMachineModel";
            machineInstance.transform.localScale = new Vector3(0.16f, 0.16f, 0.16f);

            foreach (Transform child in machineInstance.GetComponentsInChildren<Transform>())
            {
                if (child.name.Contains("SM_HorizontalBar")) child.gameObject.SetActive(false);
            }
        }

        GameObject invisibleFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        invisibleFloor.name = "InvisibleFloor";
        invisibleFloor.transform.position = new Vector3(0, 0.9f, 0);
        invisibleFloor.transform.localScale = new Vector3(4f, 0.2f, 4f);
        DestroyImmediate(invisibleFloor.GetComponent<Renderer>());

        GameObject cowInstance = null;
        GameObject cowPrefab = Resources.Load<GameObject>("cow_plush_toy");
        if (cowPrefab != null)
        {
            cowInstance = Instantiate(cowPrefab, new Vector3(0.5f, 1.2f, 0f), Quaternion.Euler(-90, 190, 90));
            cowInstance.name = "CowToy";
            cowInstance.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            if (cowInstance.GetComponent<Collider>() == null) cowInstance.AddComponent<BoxCollider>();
            Rigidbody rb = cowInstance.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        else
        {
            Debug.LogWarning("Модель cow_plush_toy не найдена!");
        }

        GameObject gm = new GameObject("GameManager");
        ClawController controller = gm.AddComponent<ClawController>();

        controller.maxAngle = 55f;
        controller.speed = 100f;
        controller.invertHand1 = true;
        controller.invertHand2 = false;
        controller.invertHand3 = false;
        controller.squareSize = 0.4f;
        controller.moveSpeed = 0.7f;
        controller.offsetX = 0f;
        controller.offsetZ = 0f;
        controller.minY = 1.2f;
        controller.lowerSpeed = 0.8f;
        controller.grabDistance = 1.5f;
        controller.ropeTopY = 3.5f;

        if (cowInstance != null)
        {
            controller.targetToy = cowInstance;
        }

        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();

        CreateButton(canvasObj.transform, "Btn_Grab", "СОГНУТЬ (A)", new Vector2(-250, 200), controller, true);
        CreateButton(canvasObj.transform, "Btn_Release", "РАЗОГНУТЬ (D)", new Vector2(250, 200), controller, false);

        // Добавляем экранный логгер для отладки на телефоне
        new GameObject("OnScreenLogger").AddComponent<OnScreenLogger>();

        Debug.Log("Сцена собрана с фиксированными настройками и экранным логгером!");
    }

    private void CreateButton(Transform parent, string name, string text, Vector2 pos, ClawController controller, bool isGrab)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 150);
        rt.anchoredPosition = pos;
        rt.anchorMin = new Vector2(0.5f, 0);
        rt.anchorMax = new Vector2(0.5f, 0);

        Image img = btnObj.AddComponent<Image>();
        img.color = isGrab ? new Color(0.2f, 0.6f, 1f) : new Color(1f, 0.6f, 0.2f);
        img.raycastTarget = true;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;

        UIButtonController uiBtn = btnObj.AddComponent<UIButtonController>();
        uiBtn.clawController = controller;
        uiBtn.isGrabButton = isGrab;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        Text t = textObj.AddComponent<Text>();
        t.text = text;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.fontSize = 32;

        RectTransform trt = textObj.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;
    }
}

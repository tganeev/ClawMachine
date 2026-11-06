using UnityEngine;

public class ClawController : MonoBehaviour
{
    [Header("Настройки сжатия")]
    public float maxAngle = 55f;
    public float speed = 100f;

    [Header("Инверсия для каждого пальца")]
    public bool invertHand1 = false;
    public bool invertHand2 = false;
    public bool invertHand3 = false;

    [Header("Движение по квадрату с диагоналями")]
    public float squareSize = 0.4f;
    public float moveSpeed = 1.5f;

    [Header("Смещение центра")]
    public float offsetX = 0f;
    public float offsetZ = 0f;

    [Header("Опускание и захват")]
    public float minY = 1.2f;
    public float lowerSpeed = 2f;
    public float grabDistance = 1.5f;

    [Header("Высота потолка")]
    public float ropeTopY = 3.5f;

    [Header("Ссылки")]
    public Transform hand1;
    public Transform hand2;
    public Transform hand3;
    public GameObject targetToy;

    private float currentAngle = 0f;
    private Quaternion baseRot1, baseRot2, baseRot3;
    private Transform clawPivot;

    private Vector3[] waypoints;
    private int currentWaypointIndex = 0;
    private bool isInitialized = false;

    private float originalY;
    private bool isLowering = false;
    private bool isGrabbing = false;
    private GameObject grabbedToy = null;
    private Rigidbody grabbedRigidbody;

    private bool isGrabbingActive = false;
    private bool isReleasingActive = false;

    private LineRenderer fakeRope;

    void Start()
    {
        InitializeClaw();
    }

    void InitializeClaw()
    {
        hand1 = FindTransformByName("SM_Hand_1") ?? hand1;
        hand2 = FindTransformByName("SM_Hand_2") ?? hand2;
        hand3 = FindTransformByName("SM_Hand_3") ?? hand3;

        if (hand1 != null) baseRot1 = hand1.localRotation;
        if (hand2 != null) baseRot2 = hand2.localRotation;
        if (hand3 != null) baseRot3 = hand3.localRotation;

        Transform oldRope = FindTransformByName("SM_Rope");
        if (oldRope == null) oldRope = FindTransformByName("SM_VerticalBar");
        if (oldRope != null) oldRope.gameObject.SetActive(false);

        GameObject pivotObj = new GameObject("ClawPivot");
        pivotObj.transform.position = new Vector3(0, ropeTopY - 1.5f, 0);

        clawPivot = pivotObj.transform;
        clawPivot.rotation = Quaternion.Euler(0, 90, 0);
        originalY = clawPivot.position.y;

        MoveToPivot("SM_Hand_Holder");
        MoveToPivot("SM_Hand_Holder_Top");
        MoveToPivot("SM_Hand_1");
        MoveToPivot("SM_Hand_2");
        MoveToPivot("SM_Hand_3");

        GameObject ropeObj = new GameObject("FakeRope");
        fakeRope = ropeObj.AddComponent<LineRenderer>();

        fakeRope.startWidth = 0.02f;
        fakeRope.endWidth = 0.02f;
        fakeRope.positionCount = 2;
        fakeRope.useWorldSpace = true;

        // ИСПРАВЛЕНИЕ: используем надежный шейдер для LineRenderer
        Material ropeMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
        if (ropeMaterial == null)
        {
            Debug.LogWarning("[ClawController] Не удалось создать материал для троса, используется дефолтный");
        }
        else
        {
            fakeRope.material = ropeMaterial;
            fakeRope.material.color = Color.black;
        }

        fakeRope.sortingOrder = -1;

        Vector3 center = clawPivot.position;
        Vector3 shiftedCenter = new Vector3(center.x + offsetX, center.y, center.z + offsetZ);

        Vector3 topLeft = new Vector3(shiftedCenter.x - squareSize, shiftedCenter.y, shiftedCenter.z + squareSize);
        Vector3 topRight = new Vector3(shiftedCenter.x + squareSize, shiftedCenter.y, shiftedCenter.z + squareSize);
        Vector3 bottomRight = new Vector3(shiftedCenter.x + squareSize, shiftedCenter.y, shiftedCenter.z - squareSize);
        Vector3 bottomLeft = new Vector3(shiftedCenter.x - squareSize, shiftedCenter.y, shiftedCenter.z - squareSize);

        waypoints = new Vector3[] {
            topLeft, topRight, bottomRight, bottomLeft,
            shiftedCenter, topLeft, shiftedCenter, bottomRight,
            shiftedCenter, topRight, shiftedCenter, bottomLeft
        };

        clawPivot.position = topLeft;
        isInitialized = true;
    }

    void MoveToPivot(string name)
    {
        Transform obj = FindTransformByName(name);
        if (obj != null) obj.SetParent(clawPivot, true);
    }

    Transform FindTransformByName(string name)
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (var obj in allObjects)
        {
            if (obj.name == name) return obj.transform;
        }
        return null;
    }

    public void StartGrabbing()
    {
        Debug.Log("[ClawController] StartGrabbing вызван");
        isGrabbingActive = true;
        isReleasingActive = false;

        if (!isLowering && !isGrabbing && grabbedToy == null)
        {
            isLowering = true;
        }
    }

    public void StopGrabbing()
    {
        Debug.Log("[ClawController] StopGrabbing вызван");
        isGrabbingActive = false;
    }

    public void StartReleasing()
    {
        Debug.Log("[ClawController] StartReleasing вызван");
        isReleasingActive = true;
        isGrabbingActive = false;
    }

    public void StopReleasing()
    {
        Debug.Log("[ClawController] StopReleasing вызван");
        isReleasingActive = false;
    }

    void Update()
    {
        if (!isInitialized || clawPivot == null || waypoints == null) return;

        bool grabActive = Input.GetKey(KeyCode.A) || isGrabbingActive;
        bool releaseActive = Input.GetKey(KeyCode.D) || isReleasingActive;

        if (grabActive)
        {
            currentAngle = Mathf.MoveTowards(currentAngle, maxAngle, Time.deltaTime * speed);

            if (!isLowering && !isGrabbing && grabbedToy == null)
            {
                isLowering = true;
            }
        }
        else if (releaseActive)
        {
            currentAngle = Mathf.MoveTowards(currentAngle, 0f, Time.deltaTime * speed);

            if (grabbedToy != null)
            {
                grabbedToy.transform.SetParent(null);
                if (grabbedRigidbody != null)
                {
                    grabbedRigidbody.isKinematic = false;
                    grabbedRigidbody.useGravity = true;
                }
                grabbedToy = null;
                grabbedRigidbody = null;
            }

            if (isLowering || isGrabbing)
            {
                isLowering = false;
                isGrabbing = true;
            }
        }

        if (isLowering)
        {
            if (clawPivot.position.y > minY)
            {
                clawPivot.position -= new Vector3(0, lowerSpeed * Time.deltaTime, 0);
            }
            else
            {
                clawPivot.position = new Vector3(clawPivot.position.x, minY, clawPivot.position.z);
                isLowering = false;
                isGrabbing = true;

                if (targetToy != null && grabbedToy == null)
                {
                    Vector3 clawPosFlat = new Vector3(clawPivot.position.x, 0, clawPivot.position.z);
                    Vector3 toyPosFlat = new Vector3(targetToy.transform.position.x, 0, targetToy.transform.position.z);

                    if (Vector3.Distance(clawPosFlat, toyPosFlat) < grabDistance)
                    {
                        grabbedToy = targetToy;
                        grabbedToy.transform.SetParent(clawPivot);
                        grabbedToy.transform.localPosition = new Vector3(0f, -0.5f, 0f);

                        grabbedRigidbody = grabbedToy.GetComponent<Rigidbody>();
                        if (grabbedRigidbody != null)
                        {
                            grabbedRigidbody.isKinematic = true;
                            grabbedRigidbody.useGravity = false;
                        }
                    }
                }
            }
        }

        if (isGrabbing)
        {
            if (clawPivot.position.y < originalY)
            {
                clawPivot.position += new Vector3(0, lowerSpeed * Time.deltaTime, 0);
            }
            else
            {
                clawPivot.position = new Vector3(clawPivot.position.x, originalY, clawPivot.position.z);
                isGrabbing = false;
            }
        }

        if (!isLowering && !isGrabbing)
        {
            Vector3 targetPos = waypoints[currentWaypointIndex];
            clawPivot.position = Vector3.MoveTowards(clawPivot.position, targetPos, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(clawPivot.position, targetPos) < 0.01f)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
        }

        if (fakeRope != null)
        {
            Vector3 topPoint = new Vector3(clawPivot.position.x, ropeTopY, clawPivot.position.z);
            Vector3 clawPoint = new Vector3(clawPivot.position.x, clawPivot.position.y + 0.1f, clawPivot.position.z);

            fakeRope.SetPosition(0, topPoint);
            fakeRope.SetPosition(1, clawPoint);
        }

        float angle1 = invertHand1 ? -currentAngle : currentAngle;
        float angle2 = invertHand2 ? -currentAngle : currentAngle;
        float angle3 = invertHand3 ? -currentAngle : currentAngle;

        if (hand1 != null) hand1.localRotation = baseRot1 * Quaternion.Euler(angle1, 0, 0);
        if (hand2 != null) hand2.localRotation = baseRot2 * Quaternion.Euler(angle2, 0, 0);
        if (hand3 != null) hand3.localRotation = baseRot3 * Quaternion.Euler(angle3, 0, 0);
    }
}

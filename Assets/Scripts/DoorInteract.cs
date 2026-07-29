using UnityEngine;
using UnityEngine.InputSystem;

public class DoorInteract : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HingeJoint hinge;
    [SerializeField] private Transform player;
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private Transform allowedSidePoint;

    [Header("Door")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float closedAngle = 0f;
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private bool opensFromFrontOnly = true;

    [Header("Message")]
    [SerializeField] private float messageDuration = 2f;

    private bool isOpen;
    private float messageTimer;
    private string currentMessage = "";

    private void Awake()
    {
        if (hinge == null)
            hinge = GetComponent<HingeJoint>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    private void OnEnable()
    {
        if (interactAction != null && interactAction.action != null)
        {
            interactAction.action.performed += OnInteract;
            interactAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (interactAction != null && interactAction.action != null)
        {
            interactAction.action.performed -= OnInteract;
            interactAction.action.Disable();
        }
    }

    private void Start()
    {
        SetDoorAngle(closedAngle);
    }

    private void Update()
    {
        if (messageTimer > 0f)
            messageTimer -= Time.deltaTime;
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (player == null || hinge == null)
            return;

        if (Vector3.Distance(player.position, transform.position) > interactRange)
            return;

        if (opensFromFrontOnly && IsPlayerOnBlockedSide())
        {
            ShowMessage("does not open from this side");
            return;
        }

        isOpen = !isOpen;
        SetDoorAngle(isOpen ? openAngle : closedAngle);
    }

    private bool IsPlayerOnBlockedSide()
    {
        if (allowedSidePoint == null)
            return false;

        float playerDistance = Vector3.Distance(player.position, allowedSidePoint.position);
        float doorDistance = Vector3.Distance(transform.position, allowedSidePoint.position);

        return playerDistance > doorDistance;
    }

    private void SetDoorAngle(float angle)
    {
        JointSpring spring = hinge.spring;
        spring.targetPosition = angle;
        hinge.spring = spring;
        hinge.useSpring = true;
    }

    private void ShowMessage(string message)
    {
        currentMessage = message;
        messageTimer = messageDuration;
    }

    private GUIStyle messageStyle;

    private void OnGUI()
    {
        if (messageTimer <= 0f)
            return;

        if (messageStyle == null)
        {
            messageStyle = new GUIStyle(GUI.skin.box);
            messageStyle.alignment = TextAnchor.MiddleCenter;
            messageStyle.fontSize = Mathf.RoundToInt(Screen.height * 0.035f); // scales with resolution
            messageStyle.wordWrap = true;
        }

        float boxWidth = Screen.width * 0.45f;
        float boxHeight = Screen.height * 0.08f;
        float x = (Screen.width - boxWidth) * 0.5f;
        float y = Screen.height * 0.82f;

        GUI.Box(new Rect(x, y, boxWidth, boxHeight), currentMessage, messageStyle);
    }
}
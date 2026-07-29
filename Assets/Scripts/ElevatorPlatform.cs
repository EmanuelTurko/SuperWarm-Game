using UnityEngine;
using UnityEngine.InputSystem;

public class ElevatorInteract : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform elevatorPlatform;
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private Transform player;

    [Header("Settings")]
    [SerializeField] private string uniqueId = "scene1_elevator_01";
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private bool goToTopOnly = true;

    private bool isPlayerNear;
    private bool isMoving;
    private bool isUnlocked;
    private Vector3 targetPosition;

    private string SaveKey => $"elevator_unlocked_{uniqueId}";

    private void Awake()
    {
        if (elevatorPlatform == null || pointA == null || pointB == null)
            return;

        isUnlocked = PlayerPrefs.GetInt(SaveKey, 0) == 1;

        elevatorPlatform.position = isUnlocked ? pointB.position : pointA.position;
    }

    private void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteract;
            Debug.Log("Interact action enabled: " + interactAction.action.name);
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteract;
            interactAction.action.Disable();
        }
    }

    private void Update()
    {
        if (player != null)
            isPlayerNear = Vector3.Distance(transform.position, player.position) <= interactRange;

        if (!isMoving || elevatorPlatform == null)
            return;

        elevatorPlatform.position = Vector3.MoveTowards(
            elevatorPlatform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(elevatorPlatform.position, targetPosition) < 0.01f)
        {
            elevatorPlatform.position = targetPosition;
            isMoving = false;

            if (!isUnlocked && pointB != null &&
                Vector3.Distance(elevatorPlatform.position, pointB.position) < 0.05f)
            {
                isUnlocked = true;
                PlayerPrefs.SetInt(SaveKey, 1);
                PlayerPrefs.Save();
                Debug.Log("Elevator unlocked permanently");
            }

            Debug.Log("Elevator reached target");
        }
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        Debug.Log("E pressed. isPlayerNear=" + isPlayerNear);

        if (!isPlayerNear || isMoving)
            return;

        if (elevatorPlatform == null)
        {
            Debug.LogError("elevatorPlatform is NULL");
            return;
        }

        if (pointB == null)
        {
            Debug.LogError("pointB is NULL");
            return;
        }

        if (goToTopOnly)
        {
            targetPosition = pointB.position;
        }
        else
        {
            if (pointA == null)
            {
                Debug.LogError("pointA is NULL");
                return;
            }

            float distToA = Vector3.Distance(elevatorPlatform.position, pointA.position);
            float distToB = Vector3.Distance(elevatorPlatform.position, pointB.position);

            targetPosition = distToA < distToB ? pointB.position : pointA.position;
        }

        Debug.Log("Set targetPosition to: " + targetPosition);
        isMoving = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("enter");
        if (other.CompareTag("Player"))
            isPlayerNear = true;
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("exit");
        if (other.CompareTag("Player"))
            isPlayerNear = false;
    }

    public void ResetElevatorState()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();

        isUnlocked = false;
        isMoving = false;

        if (elevatorPlatform != null && pointA != null)
            elevatorPlatform.position = pointA.position;
    }
}
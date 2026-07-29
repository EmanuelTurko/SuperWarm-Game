using UnityEngine;
using UnityEngine.InputSystem;

public class PersistentShortcutOpener : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject objectToDestroy;
    [SerializeField] private InputActionReference interactAction;

    [Header("Settings")]
    [SerializeField] private string uniqueId = "scene1_shortcut_01";
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private Transform player;

    private bool isPlayerNear;
    private bool alreadyOpened;

    private string SaveKey => $"shortcut_opened_{uniqueId}";

    private void Awake()
    {
        alreadyOpened = PlayerPrefs.GetInt(SaveKey, 0) == 1;

        if (alreadyOpened)
        {
            if (objectToDestroy != null)
                Destroy(objectToDestroy);

            Destroy(gameObject);
        }
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

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        Debug.Log("E pressed. isPlayerNear=" + isPlayerNear);

        if (!isPlayerNear || alreadyOpened)
            return;

        alreadyOpened = true;

        if (objectToDestroy != null)
            Destroy(objectToDestroy);

        PlayerPrefs.SetInt(SaveKey, 1);
        PlayerPrefs.Save();

        Destroy(gameObject);
    }

    private void Update()
    {
        if (player == null) return;

        isPlayerNear = Vector3.Distance(transform.position, player.position) <= interactRange;
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
}
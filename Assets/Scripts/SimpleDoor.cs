using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; 

namespace Assets.Scripts
{
    public class SimpleDoor : MonoBehaviour
    {
        [Header("Movement")]
        public Transform doorModel;          // The part that moves
        public float openDistance = 3f;      // How far down in Y
        public float openDuration = 1f;      // Time in seconds

        [Header("Triggers")]
        public Collider frontTrigger;
        public Collider backTrigger;

        [Header("UI")]
        public TMPro.TextMeshProUGUI messageText;  // optional UI text
        public string lockedMessage = "Does not open from this side";
        public float messageDuration = 2f;
        
        
        [Header("Input")]
        public InputActionReference interactAction;

        bool isMoving = false;
        bool isOpen = false;
        bool playerInFront = false;
        bool playerAtBack = false;
        Vector3 closedPosition;
        Vector3 openPosition;

        void Awake()
        {
            if (doorModel == null)
                doorModel = transform; // fallback

            closedPosition = doorModel.localPosition;
            openPosition = closedPosition + new Vector3(0f, -openDistance, 0f);

            if (messageText != null)
                messageText.gameObject.SetActive(false);
        }

        void OnEnable()
        {
            if (interactAction == null)
            {
                Debug.Log("interactAction is NULL");
                return;
            }

            interactAction.action.Enable();
        }

     
        void Update()
        {
      

            // Read button press using new Input System
            if (interactAction.action.WasPerformedThisFrame())
            {
                if (playerInFront && !isMoving && !isOpen)
                {
                    Debug.Log("Pressed E infront of the door");
                    StartCoroutine(OpenDoor());
                }
                else if (playerAtBack && !isMoving && !isOpen)
                {
                    if (messageText != null)
                        StartCoroutine(ShowLockedMessage());
                }
            }
        }

        IEnumerator OpenDoor()
        {
            isMoving = true;

            float t = 0f;
            Vector3 startPos = doorModel.localPosition;
            Vector3 targetPos = openPosition;

            while (t < 1f)
            {
                t += Time.deltaTime / openDuration;
                doorModel.localPosition = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            doorModel.localPosition = targetPos;
            isOpen = true;
            isMoving = false;
            Destroy(gameObject);
        }

        IEnumerator ShowLockedMessage()
        {
            messageText.gameObject.SetActive(true);
            messageText.text = lockedMessage;
            yield return new WaitForSeconds(messageDuration);
            messageText.gameObject.SetActive(false);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            Debug.Log("Hello");
            
            Vector3 playerPos = other.bounds.center;
            if (frontTrigger.bounds.Contains(playerPos))
            {   
                playerInFront = true;
                playerAtBack = false;
                Debug.Log("Player in front zone: " + playerInFront);
            }
            else if (backTrigger.bounds.Contains(playerPos))
            {
                playerAtBack = true;
                playerInFront = false;
                Debug.Log("Player in back zone: " + playerAtBack);
            }
            
        }

        void OnTriggerExit(Collider other)
        {
           
            if (!other.CompareTag("Player")) return;

            if (other == frontTrigger)
                playerInFront = false;
            else if (other == backTrigger)
                playerAtBack = false;
        }
    }
}
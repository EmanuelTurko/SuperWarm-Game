using Assets.Scripts;
using UnityEngine;

public class LevelStarter : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private TimeFreeze playerTimeFreeze;
    
    [Header("UI")]
    [SerializeField] private Canvas tutorialCanvas;   // HUD / tutorial canvas to show after landing

    private bool hasStarted = false;

    void Start()
    {
        // Optional safety: if nothing is wired in the Inspector, try to auto-find.
        if (playerTimeFreeze == null)
            playerTimeFreeze = FindAnyObjectByType<TimeFreeze>();
        

        // Hide tutorial / HUD at start
        if (tutorialCanvas != null)
            tutorialCanvas.enabled = false;

        // At the very beginning: no time-freeze effect (normal time),
        // and (if you want) character controller disabled so player just "falls in"
        if (playerTimeFreeze != null)
        {
            playerTimeFreeze.SetDefaultValues();   // all 1s = no time dilation
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasStarted) return;
        if (!other.CompareTag("Player")) return;
        hasStarted = true;


        // Apply the real SUPERHOT-like TimeFreeze values
        if (playerTimeFreeze != null)
        {
            playerTimeFreeze.SetGameValues();
        }

        // Show HUD / tutorial canvas
        if (tutorialCanvas != null)
           // tutorialCanvas.enabled = true;
        
        Destroy(gameObject);
        
    }

 
}
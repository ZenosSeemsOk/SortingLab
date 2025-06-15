using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SwipeMenuController : MonoBehaviour
{
    [Header("Menu Settings")]
    public List<GameObject> panels = new List<GameObject>();
    public float swipeThreshold = 50f;
    public float animationSpeed = 8f; // Increased default speed
    public float swipeDelay = 0.1f; // Minimum time between swipes
    public bool enableMouseInput = true; // For testing in editor

    [Header("UI References")]
    public Transform panelContainer;
    public ScrollRect scrollRect; // Optional: if using ScrollRect

    private int currentPanelIndex = 0;
    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private bool isAnimating = false;
    private Vector3 targetPosition;
    private float lastSwipeTime = 0f;

    // Panel positions
    private List<Vector3> panelPositions = new List<Vector3>();

    void Start()
    {
        InitializePanels();
        SetupPanelPositions();
        ShowPanel(0);
    }

    void Update()
    {
        HandleInput();
        AnimateToTarget();
    }

    void InitializePanels()
    {
        if (panels.Count == 0)
        {
            Debug.LogWarning("No panels assigned to SwipeMenuController!");
            return;
        }

        // Ensure all panels are children of panelContainer
        foreach (GameObject panel in panels)
        {
            if (panel.transform.parent != panelContainer)
            {
                panel.transform.SetParent(panelContainer);
            }
        }
    }

    void SetupPanelPositions()
    {
        panelPositions.Clear();

        for (int i = 0; i < panels.Count; i++)
        {
            // Position panels horizontally
            Vector3 position = new Vector3(i * Screen.width, 0, 0);
            panelPositions.Add(position);
            panels[i].transform.localPosition = position;
        }

        targetPosition = panelPositions[0];
    }

    void HandleInput()
    {
        // Check if we're in swipe cooldown
        if (Time.time - lastSwipeTime < swipeDelay) return;

        // Touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                startTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                endTouchPosition = touch.position;
                DetectSwipe();
            }
        }

        // Mouse input (for testing in editor)
        if (enableMouseInput)
        {
            if (Input.GetMouseButtonDown(0))
            {
                startTouchPosition = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                endTouchPosition = Input.mousePosition;
                DetectSwipe();
            }
        }
    }

    void DetectSwipe()
    {
        Vector2 swipeDirection = endTouchPosition - startTouchPosition;

        if (Mathf.Abs(swipeDirection.x) > swipeThreshold)
        {
            lastSwipeTime = Time.time; // Update last swipe time

            if (swipeDirection.x > 0)
            {
                // Swipe right - go to previous panel
                SwipeToPreviousPanel();
            }
            else
            {
                // Swipe left - go to next panel
                SwipeToNextPanel();
            }
        }
    }

    public void SwipeToNextPanel()
    {
        if (currentPanelIndex < panels.Count - 1)
        {
            currentPanelIndex++;
            ShowPanel(currentPanelIndex);
        }
    }

    public void SwipeToPreviousPanel()
    {
        if (currentPanelIndex > 0)
        {
            currentPanelIndex--;
            ShowPanel(currentPanelIndex);
        }
    }

    public void ShowPanel(int index)
    {
        if (index < 0 || index >= panels.Count) return;

        currentPanelIndex = index;
        targetPosition = new Vector3(-panelPositions[index].x, 0, 0);
        isAnimating = true;

        // Optional: Update UI indicators
        OnPanelChanged(index);
    }

    void AnimateToTarget()
    {
        if (!isAnimating) return;

        panelContainer.localPosition = Vector3.Lerp(
            panelContainer.localPosition,
            targetPosition,
            animationSpeed * Time.deltaTime
        );

        if (Vector3.Distance(panelContainer.localPosition, targetPosition) < 0.1f)
        {
            panelContainer.localPosition = targetPosition;
            isAnimating = false;
        }
    }

    // Override this method to handle panel change events
    protected virtual void OnPanelChanged(int newPanelIndex)
    {
        Debug.Log($"Switched to panel {newPanelIndex}");
    }

    // Public methods for UI buttons
    public void NextPanel()
    {
        SwipeToNextPanel();
    }

    public void PreviousPanel()
    {
        SwipeToPreviousPanel();
    }

    public void GoToPanel(int index)
    {
        ShowPanel(index);
    }

    // Get current panel info
    public int GetCurrentPanelIndex()
    {
        return currentPanelIndex;
    }

    public GameObject GetCurrentPanel()
    {
        return panels[currentPanelIndex];
    }
}
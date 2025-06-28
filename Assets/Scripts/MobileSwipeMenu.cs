using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SwipeMenuController : MonoBehaviour
{
    [Header("Menu Settings")]
    public List<GameObject> panels = new List<GameObject>();
    public float swipeThreshold = 50f;
    public float animationSpeed = 8f; // Adjust for desired swipe animation
    public float swipeDelay = 0.3f; // Prevent too rapid swiping
    public bool enableMouseInput = true;

    [Header("UI References")]
    public Transform panelContainer;
    public ScrollRect scrollRect; // Optional

    private int currentPanelIndex = 0;
    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private bool isAnimating = false;
    private Vector3 targetPosition;
    private float lastSwipeTime = 0f;

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
            Vector3 position = new Vector3(i * Screen.width, 0, 0);
            panelPositions.Add(position);
            panels[i].transform.localPosition = position;
        }

        targetPosition = panelContainer.localPosition;
    }

    void HandleInput()
    {
        if (isAnimating || Time.time - lastSwipeTime < swipeDelay) return;

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
            lastSwipeTime = Time.time;

            if (swipeDirection.x > 0)
            {
                SwipeToPreviousPanel();
            }
            else
            {
                SwipeToNextPanel();
            }
        }
    }

    public void SwipeToNextPanel()
    {
        if (isAnimating || currentPanelIndex >= panels.Count - 1) return;
        currentPanelIndex++;
        ShowPanel(currentPanelIndex);
    }

    public void SwipeToPreviousPanel()
    {
        if (isAnimating || currentPanelIndex <= 0) return;
        currentPanelIndex--;
        ShowPanel(currentPanelIndex);
    }

    public void ShowPanel(int index)
    {
        if (index < 0 || index >= panels.Count) return;

        currentPanelIndex = index;
        targetPosition = new Vector3(-panelPositions[index].x, 0, 0);
        isAnimating = true;

        OnPanelChanged(index);
        Debug.Log($"Switched to panel {index} at position {targetPosition}");
    }

    void AnimateToTarget()
    {
        if (!isAnimating) return;

        panelContainer.localPosition = Vector3.MoveTowards(
            panelContainer.localPosition,
            targetPosition,
            animationSpeed * Screen.width * Time.deltaTime
        );

        if (Vector3.Distance(panelContainer.localPosition, targetPosition) < 0.1f)
        {
            panelContainer.localPosition = targetPosition;
            isAnimating = false;
        }
    }

    protected virtual void OnPanelChanged(int newPanelIndex)
    {
        Debug.Log($"Switched to panel {newPanelIndex}");
    }

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

    public int GetCurrentPanelIndex()
    {
        return currentPanelIndex;
    }

    public GameObject GetCurrentPanel()
    {
        return panels[currentPanelIndex];
    }
}

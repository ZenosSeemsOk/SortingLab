using UnityEngine;
using System;
using System.Collections;
public class GameController : MonoBehaviour
{
    [Header("Bottle References")]
    public ColorChanger FirstBottle;
    public ColorChanger SecondBottle;

    [Header("Visual Settings")]
    public float pickedBottlePositionChange = 2f;
    public float animationDuration = 0.3f;
    public LeanTweenType easeType = LeanTweenType.easeOutBack;

    [Header("Camera Reference")]
    public Camera gameCamera; // Optional: assign specific camera

    private Vector3 firstBottleOriginalPosition;
    private int currentTweenId = -1;

    private bool isAnimating;

    void Start()
    {
        // Set default camera if none assigned
        if (gameCamera == null)
            gameCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }

    private void HandleMouseClick()
    {
        if (!isAnimating)
        {
            // Get mouse position in world coordinates
            Vector3 mousePos = gameCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            // Perform raycast
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            // Check if we hit something
            if (hit.collider == null) return;

            // Get the ColorChanger component
            ColorChanger clickedBottle = hit.collider.GetComponent<ColorChanger>();
            if (clickedBottle == null) return;

            // Handle bottle selection logic
            if (FirstBottle == null)
            {
                SelectFirstBottle(clickedBottle);
            }
            else if (FirstBottle == clickedBottle)
            {
                DeselectFirstBottle();
            }
            else
            {
                ProcessBottleTransfer(clickedBottle);
            }
        }

    }

    private void SelectFirstBottle(ColorChanger bottle)
    {
            FirstBottle = bottle;

            // Store original position for later restoration
            firstBottleOriginalPosition = bottle.transform.position;

            // Cancel any existing animation on this bottle
            LeanTween.cancel(bottle.gameObject);

            // Animate bottle up to show it's selected
            Vector3 targetPosition = bottle.transform.position;
            targetPosition.y += pickedBottlePositionChange;

            currentTweenId = LeanTween.move(bottle.gameObject, targetPosition, animationDuration)
                .setEase(easeType)
                .id;

            // Optional: Add visual feedback
            // bottle.SetSelected(true);   
        
    }

    private void DeselectFirstBottle()
    {
        // Cancel any existing animations
        LeanTween.cancel(FirstBottle.gameObject);

        // Animate back to original position
        LeanTween.move(FirstBottle.gameObject, firstBottleOriginalPosition, animationDuration)
            .setEase(LeanTweenType.easeOutQuad);

        // Optional: Remove visual feedback
        // FirstBottle.SetSelected(false);

        FirstBottle = null;
        currentTweenId = -1;
    }

    private void ProcessBottleTransfer(ColorChanger targetBottle)
    {
        isAnimating = true;

        SecondBottle = targetBottle;

        // Set up the transfer relationship
        FirstBottle.colorChangerRef = SecondBottle;

        // Update color values for both bottles
        FirstBottle.UpdateTopColorValues();
        SecondBottle.UpdateTopColorValues();

        // Check if transfer is possible and execute
        if (SecondBottle.FillBottleCheck(FirstBottle.topColor))
        {
            FirstBottle.StartColorTransfer();
        }
        else
        {
            DeselectFirstBottle();
            SecondBottle = null;
            isAnimating = false;
            return;
        }
        // Cancel any existing animations on first bottle
        LeanTween.cancel(FirstBottle.gameObject);

        // Animate first bottle back to original position
        LeanTween.move(FirstBottle.gameObject, firstBottleOriginalPosition, animationDuration)
            .setEase(LeanTweenType.easeOutQuad);

        // Optional: Remove visual feedback
        // FirstBottle.SetSelected(false);

        StartCoroutine(ClearReferences(targetBottle.timeToRotate));

    }


    private IEnumerator ClearReferences(float time)
    {
        yield return new WaitForSeconds(time + 2.35f);
        // Clear references
        FirstBottle = null;
        SecondBottle = null;
        currentTweenId = -1;
        isAnimating = false;
    }

    // Optional: Method to cancel current selection (could be called by UI button or ESC key)
    public void CancelSelection()
    {
        if (FirstBottle != null)
        {
            DeselectFirstBottle();
        }
    }

    // Optional: Get currently selected bottle for UI purposes
    public ColorChanger GetSelectedBottle()
    {
        return FirstBottle;
    }

    // Clean up any running tweens when the object is destroyed
    /*
    void OnDestroy()
    {
        LeanTween.cancelAll();
    }
    */

    // Optional: Method to check if any bottle is currently animating
    public bool IsBottleAnimating()
    {
        return currentTweenId != -1 && LeanTween.isTweening(currentTweenId);
    }
}
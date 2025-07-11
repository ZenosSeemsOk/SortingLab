using System.Collections;
using UnityEngine;

public class ColorChanger : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private GameObject smokeAnimation;

    public SpriteRenderer mainBottleSR;
    public SpriteRenderer bottleMaskSR;

    public Color[] bottleColors = new Color[4];

    public AnimationCurve ScaleAndRotationMultiplierCurve;
    public AnimationCurve FillAmountCurve;
    public AnimationCurve RotationSpeedMultiplier;

    public float[] fillAmounts;
    public float[] rotationValues;

    [Range(0, 4)]
    public int numberOfColorsInBottle = 4;

    public Color topColor;
    public int numberOfTopColorLayers = 1;

    public ColorChanger colorChangerRef;
    public bool justThisBottle = false;

    public Transform leftRotationPoint;
    public Transform rightRotationPoint;

    public LineRenderer lineRenderer;

    private MusicManager musicManager;

    [HideInInspector]
    public float bottleScale = 1f; // Assigned by BottleSpawner

    public float timeToRotate = 1f;

    private int rotationIndex = 0;
    private int numberOfColorsToTransfer = 0;
    private float directionMultiplier = 1f;

    private Transform chosenRotationPoint;
    private Vector3 originalPosition;
    private Vector3 startPosition;
    private Vector3 endPosition;

    private float baseLineLength = 1.45f;
    private bool isPouringActive = false; // Track pouring state

    private GameManager gameManager;
    private Coins coins;

    private bool hasBeenCompleted = false;

    private const string SHADER_IS_POURING_PROP = "_IsPouring";

    void Start()
    {
        coins = Coins.Instance;
        gameManager = GameManager.instance;
        musicManager = MusicManager.Instance;

        if (lineRenderer == null)
        {
            GameObject lineObj = GameObject.Find("LineRenderer");
            if (lineObj != null)
                lineRenderer = lineObj.GetComponent<LineRenderer>();

            if (lineRenderer == null)
                Debug.LogWarning("LineRenderer not found in scene for " + gameObject.name);
        }

        if (lineRenderer != null)
        {
            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = 2;
            lineRenderer.enabled = false;

            float width = 0.05f * bottleScale;
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;

            lineRenderer.sortingLayerName = bottleMaskSR.sortingLayerName;
        }

        bottleMaskSR.material.SetFloat("_BottleScale", bottleScale);
        bottleMaskSR.material.SetFloat("_FillAmount", fillAmounts[numberOfColorsInBottle]);

        originalPosition = transform.position;

        UpdateColorsOnShader();
        UpdateTopColorValues();

        bottleMaskSR.material.SetFloat(SHADER_IS_POURING_PROP, 0.0f);
    }

    void Update()
    {
        IsBottleInValidState();
        if (Input.GetKeyUp(KeyCode.P) && justThisBottle)
        {
            UpdateTopColorValues();

            if (colorChangerRef.FillBottleCheck(topColor))
            {
                ChoseRotationPointAndDirection();
                numberOfColorsToTransfer = Mathf.Min(numberOfTopColorLayers, 4 - colorChangerRef.numberOfColorsInBottle);

                for (int i = 0; i < numberOfColorsToTransfer; i++)
                    colorChangerRef.bottleColors[colorChangerRef.numberOfColorsInBottle + i] = topColor;

                colorChangerRef.UpdateColorsOnShader();
            }

            CalculatedRotationIndex(4 - colorChangerRef.numberOfColorsInBottle);
            StartCoroutine(RotateBottle());
        }
    }

    public bool IsBottleInValidState()
    {
        if (hasBeenCompleted) return true;

        if (numberOfColorsInBottle == 0) return true;

        if (numberOfColorsInBottle != 4) return false;

        Color referenceColor = bottleColors[0];
        for (int i = 1; i < 4; i++)
        {
            if (!bottleColors[i].Equals(referenceColor))
                return false;
        }

        hasBeenCompleted = true;

        Color smokeColor = topColor;
        smokeColor.a = 1;
        smokeAnimation.GetComponent<SpriteRenderer>().color = smokeColor;
        smokeAnimation.SetActive(true);
        StartCoroutine(DisableComponent());
        return true;
    }

    IEnumerator DisableComponent()
    {
        yield return new WaitForSeconds(1f);
        Coins.Instance.AddCoins(5);
        smokeAnimation.GetComponent<SpriteRenderer>().enabled = false;
        smokeAnimation.SetActive(false);
    }

    public void ResetCompletionStatus()
    {
        hasBeenCompleted = false;
    }

    public void StartColorTransfer()
    {
        ChoseRotationPointAndDirection();

        mainBottleSR.sortingOrder = 3;
        bottleMaskSR.sortingOrder = 2;

        numberOfColorsToTransfer = Mathf.Min(numberOfTopColorLayers, 4 - colorChangerRef.numberOfColorsInBottle);

        for (int i = 0; i < numberOfColorsToTransfer; i++)
            colorChangerRef.bottleColors[colorChangerRef.numberOfColorsInBottle + i] = topColor;

        colorChangerRef.UpdateColorsOnShader();
        CalculatedRotationIndex(4 - colorChangerRef.numberOfColorsInBottle);

        StartCoroutine(MoveBottle());
    }

    IEnumerator MoveBottle()
    {
        startPosition = transform.position;
        endPosition = (chosenRotationPoint == leftRotationPoint) ? colorChangerRef.rightRotationPoint.position : colorChangerRef.leftRotationPoint.position;

        float t = 0;
        while (t < 1)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            t += Time.deltaTime * 2f;
            yield return null;
        }

        transform.position = endPosition;
        StartCoroutine(RotateBottle());
    }

    IEnumerator MoveBottleBack()
    {
        startPosition = transform.position;
        endPosition = originalPosition;

        float t = 0;
        while (t < 1)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            t += Time.deltaTime * 2f;
            yield return null;
        }

        mainBottleSR.sortingOrder = 1;
        bottleMaskSR.sortingOrder = 0;
        transform.position = endPosition;

        StartCoroutine(WaitToSendGameOver());
    }

    IEnumerator WaitToSendGameOver()
    {
        yield return new WaitForSeconds(1f);
        gameManager.CheckGameOver();
    }

    IEnumerator RotateBottle()
    {
        float t = 0f;
        float lerpValue, angleValue, lastAngleValue = 0f;

        while (t < timeToRotate)
        {
            lerpValue = t / timeToRotate;
            angleValue = Mathf.Lerp(0f, directionMultiplier * rotationValues[rotationIndex], lerpValue);

            transform.RotateAround(chosenRotationPoint.position, Vector3.forward, lastAngleValue - angleValue);
            bottleMaskSR.material.SetFloat("_SARM", ScaleAndRotationMultiplierCurve.Evaluate(angleValue));

            if (fillAmounts[numberOfColorsInBottle] > FillAmountCurve.Evaluate(angleValue) + 0.005f)
            {
                if (!isPouringActive)
                {
                    StartPouringSound();
                    isPouringActive = true;
                    bottleMaskSR.material.SetFloat(SHADER_IS_POURING_PROP, 1.0f);
                }

                if (lineRenderer != null && !lineRenderer.enabled)
                {
                    Color solidColor = topColor;
                    solidColor.a = 1f;

                    lineRenderer.material.color = solidColor;
                    lineRenderer.startColor = solidColor;
                    lineRenderer.endColor = solidColor;

                    float pourLength = baseLineLength * bottleScale;

                    lineRenderer.SetPosition(0, chosenRotationPoint.position);
                    lineRenderer.SetPosition(1, chosenRotationPoint.position - Vector3.up * pourLength);
                    lineRenderer.enabled = true;
                }

                bottleMaskSR.material.SetFloat("_FillAmount", FillAmountCurve.Evaluate(angleValue));
                colorChangerRef.FillUp(FillAmountCurve.Evaluate(lastAngleValue) - FillAmountCurve.Evaluate(angleValue));
            }
            else if (isPouringActive)
            {
                StopPouringSound();
                isPouringActive = false;
                bottleMaskSR.material.SetFloat(SHADER_IS_POURING_PROP, 0.0f);
            }

            t += Time.deltaTime * RotationSpeedMultiplier.Evaluate(angleValue);
            lastAngleValue = angleValue;
            yield return null;
        }

        if (isPouringActive)
        {
            StopPouringSound();
            isPouringActive = false;
            bottleMaskSR.material.SetFloat(SHADER_IS_POURING_PROP, 0.0f);
        }

        angleValue = directionMultiplier * rotationValues[rotationIndex];
        bottleMaskSR.material.SetFloat("_SARM", ScaleAndRotationMultiplierCurve.Evaluate(angleValue));
        bottleMaskSR.material.SetFloat("_FillAmount", FillAmountCurve.Evaluate(angleValue));

        numberOfColorsInBottle -= numberOfColorsToTransfer;
        colorChangerRef.numberOfColorsInBottle += numberOfColorsToTransfer;

        if (lineRenderer != null)
            lineRenderer.enabled = false;

        StartCoroutine(RotateBottleBack());
    }

    IEnumerator RotateBottleBack()
    {
        float t = 0f;
        float lerpValue, angleValue;
        float lastAngleValue = directionMultiplier * rotationValues[rotationIndex];

        while (t < timeToRotate)
        {
            lerpValue = t / timeToRotate;
            angleValue = Mathf.Lerp(directionMultiplier * rotationValues[rotationIndex], 0f, lerpValue);

            transform.RotateAround(chosenRotationPoint.position, Vector3.forward, lastAngleValue - angleValue);
            bottleMaskSR.material.SetFloat("_SARM", ScaleAndRotationMultiplierCurve.Evaluate(angleValue));

            lastAngleValue = angleValue;
            t += Time.deltaTime;
            yield return null;
        }

        UpdateTopColorValues();
        transform.eulerAngles = Vector3.zero;
        bottleMaskSR.material.SetFloat("_SARM", ScaleAndRotationMultiplierCurve.Evaluate(0f));

        StartCoroutine(MoveBottleBack());
    }

    private void StartPouringSound()
    {
        if (MusicManager.Instance != null)
        {
            Debug.Log("Pouring Sound Started");
            MusicManager.Instance.PlayPouringSound();
        }
        else
        {
            Debug.LogWarning("MusicManager instance not found!");
        }
    }

    private void StopPouringSound()
    {
        if (MusicManager.Instance != null && MusicManager.Instance.sfxAudioSource != null)
        {
            if (MusicManager.Instance.sfxAudioSource.isPlaying)
            {
                MusicManager.Instance.sfxAudioSource.Stop();
                Debug.Log("Pouring Sound Stopped");
            }
        }
        else
        {
            Debug.LogWarning("MusicManager instance or SFX AudioSource not found!");
        }
    }

    public void UpdateColorsOnShader()
    {
        bottleMaskSR.material.SetColor("_C1", bottleColors[0]);
        bottleMaskSR.material.SetColor("_C2", bottleColors[1]);
        bottleMaskSR.material.SetColor("_C3", bottleColors[2]);
        bottleMaskSR.material.SetColor("_C4", bottleColors[3]);

        //  Pass the number of colors to the shader
        bottleMaskSR.material.SetFloat("_NumberofColors", numberOfColorsInBottle);
    }

    public void UpdateTopColorValues()
    {
        if (numberOfColorsInBottle == 0) return;

        topColor = bottleColors[numberOfColorsInBottle - 1];
        numberOfTopColorLayers = 1;

        for (int i = numberOfColorsInBottle - 1; i > 0; i--)
        {
            if (bottleColors[i] == bottleColors[i - 1])
                numberOfTopColorLayers++;
            else
                break;
        }

        rotationIndex = 3 - (numberOfColorsInBottle - numberOfTopColorLayers);
    }

    public bool FillBottleCheck(Color colorToCheck)
    {
        if (numberOfColorsInBottle == 0) return true;
        if (numberOfColorsInBottle == 4) return false;
        return topColor.Equals(colorToCheck);
    }

    private void CalculatedRotationIndex(int emptySpaces)
    {
        rotationIndex = 3 - (numberOfColorsInBottle - Mathf.Min(emptySpaces, numberOfTopColorLayers));
    }

    private void FillUp(float amount)
    {
        bottleMaskSR.material.SetFloat("_FillAmount", bottleMaskSR.material.GetFloat("_FillAmount") + amount);
    }

    private void ChoseRotationPointAndDirection()
    {
        if (transform.position.x > colorChangerRef.transform.position.x)
        {
            chosenRotationPoint = leftRotationPoint;
            directionMultiplier = -1.0f;
        }
        else
        {
            chosenRotationPoint = rightRotationPoint;
            directionMultiplier = 1.0f;
        }
    }
}

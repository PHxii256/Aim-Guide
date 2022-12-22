using System.Collections.Generic;
using UnityEngine;

public class AimGuide : MonoBehaviour
{
    [Tooltip("Max guide legnth.")]
    public float maxGuideLength = 16f;

    [Tooltip("Min space between guide points (Must be non zero).")]
    public float minGuidePointDist = 2f;

    [Tooltip("Max number of guide points (Must be non zero).")]
    public int maxGuidePointCount = 3;

    [Tooltip("Automatically calculates max guide point count.")]
    public bool autoCalcMaxGuides = false;

    [Range(0, 1f)]
    [Tooltip("Set to 1 if you want a (1 to 1) correspondence with mouse input.")]
    public float inputSensitivity = 0.75f; //how much the drag from mouse matches the guides visual drag

    [Range(0, 1f)]
    [Tooltip("Set to 1 if you (Don't) want a stretching effect, Set to 0 if you want it always to be stretching.")]
    public float stretchEffectStartPoint = 0.5f; //When to start streatching behaviour as a fraction of guideLength.

    public GuideMode mode;
    public GameObject guidePrefab;

    List<SpriteRenderer> guideSprites = new List<SpriteRenderer>();
    PlayerMovement playerMovement; //Instance to player.
    bool toogled; //True when spacebar is pressed (about to move).

    private void Awake()
    {
        PlayerMovement.PullOrRelease += Toogle;
        playerMovement = FindObjectOfType<PlayerMovement>();
        InstantiateGuides();
    }

    void Update()
    {
        if (toogled) VisualizeGuides();
        else ResetGuides();
    }

    void VisualizeGuides() 
    {
        float scale = Mathf.Clamp(playerMovement.RealDist * inputSensitivity, 0, maxGuideLength); //playerMovement.RealDist is delta mouse postion minus delta player postion 
        float dist = Vector3.Distance(transform.position, playerMovement.transform.position);     //which corrospondes to the actual distance moved with a specfic time-frame
        Vector3 pos = playerMovement.transform.position;                                          //it's specifc to this game but it can be discarded without any issues.
        
        guideSprites[0].transform.position = playerMovement.DesiredDir.normalized * scale * ModeFactor() + pos;  //Moves Main point.

        int IntermediatePointsCount = Mathf.Clamp(Mathf.FloorToInt(dist / minGuidePointDist),0,maxGuidePointCount);

        for (int i = 1; i < IntermediatePointsCount; i++) //Moves Intermediate Points.
        {
            Vector3 v = transform.position - pos;
            float factor;

            if (maxGuideLength * stretchEffectStartPoint >= dist) //Pre Stretch. ([dist - minGuidePointDist * i] is the distFromPlayer)
            {
                factor = (dist - minGuidePointDist * i) / dist;
            }

            else factor = i / (float)maxGuidePointCount; //Stretch.

            guideSprites[i].transform.position = v * factor + pos;
        }

        for (int i = IntermediatePointsCount; i < maxGuidePointCount; i++) //Resets Intermediate points' postions.
        {
            if (i != 0) guideSprites[i].transform.position = pos;
        }
    }

    int ModeFactor() //-1 to indicate drag, 1 to indicate predicted movement direction
    {
        if (mode == GuideMode.IndicatedPrediction) return 1;
        else return -1;
    }

    void InstantiateGuides() 
    {
        CalcMaxGuidesCount();
        guideSprites.Add(GetComponent<SpriteRenderer>());

        for (int i = 0; i < maxGuidePointCount - 1; i++)
        {
            GameObject obj = Instantiate(guidePrefab, transform);
            guideSprites.Add(obj.GetComponent<SpriteRenderer>());
        }
    }

    private void ResetGuides()
    {
        foreach (var guideSprite in guideSprites)
        {
            guideSprite.transform.localPosition = Vector3.zero;
        } 
    }

    private void Toogle() 
    {
        toogled = !toogled;
        foreach (var guideSprite in guideSprites)
        {
            guideSprite.enabled = !guideSprite.enabled;
        }
    }

    private void CalcMaxGuidesCount() 
    {
        if (autoCalcMaxGuides) maxGuidePointCount = Mathf.FloorToInt(maxGuideLength / minGuidePointDist);
    }

    [System.Serializable]
    public enum GuideMode
    {
        IndicateDrag,
        IndicatedPrediction
    }
}

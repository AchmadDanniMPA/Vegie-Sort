using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [Header("References")]
    public GameObject vegiePrefab;
    public GameObject targetPrefab;
    public Transform[] spawnPoints;
    public Transform[] targetPoints;
    public Sprite[] coloredSprites;
    public Sprite[] greyedSprites;
    public Transform canvasTransform;
    public Animator rabbitAnimator;
    public Transform rabbitHoverSpot;
    public Transform[] basketSlots;
    public Transform boxTarget;
    public Transform[] pourOrigins;
    public Sprite[] boxSprites;
    public Image boxImage;
    [Header("State")]
    private int currentStage = 0;
    private int correctDrops = 0;
    private int basketIndex = 0;
    private int boxFill = 0;
    private List<GameObject> spawnedVegies = new List<GameObject>();
    private List<GameObject> spawnedTargets = new List<GameObject>();
    private List<GameObject> hoverVegies = new List<GameObject>();
    private List<Sprite> savedPourSprites = new List<Sprite>();

    string[] vegieNames = new string[] {
        "Bokcoy", "Cabbage", "Carrot", "Corn", "Eggplant", "Potato", "Pumpkin", "Radish", "Tomato"
    };
    List<Transform> ShuffleList(List<Transform> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Transform temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
        return list;
    }
    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        Application.targetFrameRate = 60;
        StartStage();
    }
    void StartStage()
    {
        ClearStage();
        correctDrops = 0;
        List<int> pickedIndexes = new List<int>();
        while (pickedIndexes.Count < 3)
        {
            int rand = Random.Range(0, 9);
            if (!pickedIndexes.Contains(rand))
                pickedIndexes.Add(rand);
        }
        List<Transform> shuffledSpawnPoints = ShuffleList(new List<Transform>(spawnPoints));
        List<Transform> shuffledTargetPoints = ShuffleList(new List<Transform>(targetPoints));

        for (int i = 0; i < 3; i++)
        {
            int spriteIndex = pickedIndexes[i];
            Transform vegiePos = shuffledSpawnPoints[i];
            Transform targetPos = shuffledTargetPoints[i];

            SpawnPair(vegiePos, targetPos, spriteIndex);
        }
    }
    void SpawnPair(Transform vegiePos, Transform targetPos, int spriteIndex)
    {
        string vegieName = vegieNames[spriteIndex];
        GameObject vegie = Instantiate(vegiePrefab, vegiePos.position, Quaternion.identity, canvasTransform);
        vegie.GetComponent<Image>().sprite = coloredSprites[spriteIndex];
        vegie.GetComponent<VegiePiece>().vegieName = vegieName;
        vegie.transform.SetAsLastSibling();
        spawnedVegies.Add(vegie);
        GameObject target = Instantiate(targetPrefab, targetPos.position, Quaternion.identity, canvasTransform);
        target.GetComponent<Image>().sprite = greyedSprites[spriteIndex];
        target.GetComponent<TargetSlot>().expectedVegie = vegieName;
        spawnedTargets.Add(target);
    }
    public void OnCorrectDrop(VegiePiece piece, TargetSlot slot)
    {
        piece.isMatched = true;
        if (rabbitAnimator != null)
            rabbitAnimator.SetTrigger("Happy");
        Sprite matchedSprite = piece.GetComponent<Image>().sprite;
        Vector3 fromPosition = slot.transform.position;
        Destroy(piece.gameObject);
        Destroy(slot.gameObject);
        int currentIndex = basketIndex;
        basketIndex++;
        correctDrops++;
        if (correctDrops < 3)
        {
            StartCoroutine(MoveToBasket(fromPosition, matchedSprite, currentIndex));
        }
        else if (correctDrops == 3)
        {
            StartCoroutine(MoveToBasket(fromPosition, matchedSprite, currentIndex, () =>
            {
                List<Sprite> savedSprites = new List<Sprite>();
                foreach (GameObject vegie in hoverVegies)
                {
                    if (vegie == null) continue;
                    Image img = vegie.GetComponent<Image>();
                    if (img != null) savedSprites.Add(img.sprite);
                    Destroy(vegie);
                }
                hoverVegies.Clear();
                basketIndex = 0;
                savedPourSprites = savedSprites;
                if (rabbitAnimator != null) rabbitAnimator.SetTrigger("Pour");
            }));
        }
    }
    IEnumerator MoveToBasket(Vector3 from, Sprite vegieSprite, int slotIndex, System.Action onArrived = null)
    {
        GameObject hoverVegie = new GameObject("HoverVegie_" + slotIndex);
        hoverVegie.transform.SetParent(canvasTransform, false);
        Image image = hoverVegie.AddComponent<Image>();
        image.sprite = vegieSprite;
        image.SetNativeSize();
        RectTransform rect = hoverVegie.GetComponent<RectTransform>();
        rect.position = from;
        Vector3 target = basketSlots[slotIndex].position;
        float speed = 300f;
        hoverVegies.Add(hoverVegie);
        while (Vector3.Distance(rect.position, target) > 1f)
        {
            rect.position = Vector3.MoveTowards(rect.position, target, speed * Time.deltaTime);
            yield return null;
        }
        rect.position = target;
        hoverVegie.transform.SetParent(basketSlots[slotIndex], true);
        onArrived?.Invoke();
    }
    IEnumerator MoveToBox(GameObject vegie)
    {
        Vector3 target = boxTarget.position;
        RectTransform rect = vegie.GetComponent<RectTransform>();
        float speed = 400f;
        while (Vector3.Distance(rect.position, target) > 1f)
        {
            rect.position = Vector3.MoveTowards(rect.position, target, speed * Time.deltaTime);
            yield return null;
        }
        rect.position = target;
        Destroy(vegie);
    }
    public void TriggerVegiePour()
    {
        StartCoroutine(SpawnAndPourToBox(savedPourSprites));
    }
    IEnumerator SpawnAndPourToBox(List<Sprite> sprites)
    {
        for (int i = 0; i < sprites.Count; i++)
        {
            GameObject v = new GameObject("PourVegie_" + i);
            v.transform.SetParent(canvasTransform, false);
            Image img = v.AddComponent<Image>();
            img.sprite = sprites[i];
            img.SetNativeSize();
            RectTransform rect = v.GetComponent<RectTransform>();
            rect.position = pourOrigins[i].position;
            StartCoroutine(MoveToBox(v));
        }
        boxFill = Mathf.Clamp(boxFill + 1, 0, 3);
        boxImage.sprite = boxSprites[boxFill];
        yield return new WaitForSeconds(0.4f);
        currentStage++;
        if (currentStage >= 3)
        {
            if (rabbitAnimator != null)
                rabbitAnimator.SetTrigger("Jump");
        }
        else
        {
            StartStage();
        }
    }
    public void OnWrongDrop(VegiePiece piece)
    {
        piece.transform.position = piece.startPosition;
        rabbitAnimator.SetTrigger("Sad");
    }
    void ClearStage()
    {
        foreach (var obj in spawnedVegies) Destroy(obj);
        foreach (var obj in spawnedTargets) Destroy(obj);
        spawnedVegies.Clear();
        spawnedTargets.Clear();
    }
}

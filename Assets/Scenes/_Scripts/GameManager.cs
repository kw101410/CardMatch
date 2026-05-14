using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI; // Text 컴포넌트를 사용하기 위해 필수!
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro를 사용하기 위해 추가
public class GameManager : MonoBehaviour
{
    [Header("게임 설정")]
    public GameObject cardPrefab;
    public Transform cardGrid;
    public Sprite cardBackSprite;

    [Header("사용할 카드 앞면 (모든 종류)")]
    public List<Sprite> allCardSprites;

    [Header("난이도별 그리드 크기 (X=열, Y=행)")]
    public Vector2Int easyDimensions = new Vector2Int(4, 4);
    public Vector2Int normalDimensions = new Vector2Int(8, 4);
    public Vector2Int hardDimensions = new Vector2Int(10, 6);
    public Vector2Int insaneDimensions = new Vector2Int(13, 8);

    [Header("난이도별 시간 (초)")]
    public float easyTime = 180f;
    public float normalTime = 300f;
    public float hardTime = 600f;
    public float insaneTime = 1200f;

    [Header("UI (타이머, 게임오버)")]
    public Image timerImage;
    public GameObject gameOverPanel;
    public GameObject winPanel;
    public TextMeshProUGUI clearTimeText; // [★★ 수정됨 ★★] 클리어 시간을 표시할 텍스트

    [Header("오디오 설정")]
    public AudioSource sfxAudioSource;
    public AudioClip cardFlipSound;

    // === 내부 변수들 ===
    private List<Sprite> cardFrontSprites = new List<Sprite>();
    private List<Card> spawnedCards = new List<Card>();
    private Card firstCard = null;
    private Card secondCard = null;
    private bool canClick = true;
    private System.Random random = new System.Random();
    private float cardFlipDuration;

    private GridLayoutGroup gridLayout;
    private Vector2Int currentGridDimensions;
    private bool hasEmptySlot = false;

    private float timeLimit;
    private float currentTime;
    private bool isGameActive = false;

    void Start()
    {
        gridLayout = cardGrid.GetComponent<GridLayoutGroup>();
        if (winPanel != null) winPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        LoadDifficulty();
        SetupGame();

        currentTime = timeLimit;
        isGameActive = true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GoToMainMenu();
        }

        if (isGameActive)
        {
            currentTime -= Time.deltaTime;

            if (timerImage != null)
            {
                timerImage.fillAmount = currentTime / timeLimit;
            }

            if (currentTime <= 0)
            {
                currentTime = 0;
                HandleGameOver();
            }
        }
    }

    void LoadDifficulty()
    {
        int difficulty = PlayerPrefs.GetInt("Difficulty", 1);

        if (difficulty == 0)
        {
            currentGridDimensions = easyDimensions;
            timeLimit = easyTime;
        }
        else if (difficulty == 1)
        {
            currentGridDimensions = normalDimensions;
            timeLimit = normalTime;
        }
        else if (difficulty == 2)
        {
            currentGridDimensions = hardDimensions;
            timeLimit = hardTime;
        }
        else if (difficulty == 3)
        {
            currentGridDimensions = insaneDimensions;
            timeLimit = insaneTime;
        }
        else
        {
            currentGridDimensions = normalDimensions;
            timeLimit = normalTime;
        }

        int totalCards = currentGridDimensions.x * currentGridDimensions.y;
        hasEmptySlot = (totalCards % 2 != 0);
        int pairs = totalCards / 2;
        int requiredSprites = pairs;

        if (hasEmptySlot) Debug.Log($"홀수 그리드 감지: {pairs}쌍 + 빈 슬롯 1개");

        Shuffle(allCardSprites);
        cardFrontSprites.Clear();

        for (int i = 0; i < requiredSprites; i++)
        {
            if (i < allCardSprites.Count) cardFrontSprites.Add(allCardSprites[i]);
            else { Debug.LogError("All Card Sprites에 이미지가 부족합니다!"); break; }
        }
    }

    void SetupGame()
    {
        List<int> cardIDs = new List<int>();
        int pairCount = cardFrontSprites.Count;
        for (int i = 0; i < pairCount; i++) { cardIDs.Add(i); cardIDs.Add(i); }
        Shuffle(cardIDs);
        if (hasEmptySlot) cardIDs.Add(-1);

        AdjustGridSettings(currentGridDimensions.x, currentGridDimensions.y);

        foreach (int id in cardIDs)
        {
            GameObject cardObj = Instantiate(cardPrefab, cardGrid);
            if (id == -1) // 빈 슬롯
            {
                cardObj.GetComponent<Image>().enabled = false;
                cardObj.GetComponent<Button>().enabled = false;
                cardObj.GetComponent<Card>().enabled = false;
            }
            else // 일반 카드
            {
                Card card = cardObj.GetComponent<Card>();
                card.cardID = id;
                card.cardFront = cardFrontSprites[id];
                card.cardBack = cardBackSprite;
                cardObj.GetComponent<Image>().sprite = cardBackSprite;
                spawnedCards.Add(card);
            }
        }
        if (spawnedCards.Count > 0) cardFlipDuration = spawnedCards[0].flipDuration;
    }

    public void CardClicked(Card card)
    {
        if (!canClick || card.isMatched || card == firstCard || !isGameActive)
        {
            return;
        }

        card.FlipToFront();
        if (firstCard == null)
        {
            firstCard = card;
        }
        else
        {
            secondCard = card;
            canClick = false;
            StartCoroutine(CheckMatch());
        }
    }

    // [★★ 수정된 함수 ★★]
    void CheckForWin()
    {
        bool allMatched = spawnedCards.All(card => card.isMatched);
        if (allMatched)
        {
            Debug.Log("축하합니다! 게임 승리!");
            isGameActive = false; // 타이머 정지

            // [★★ 새로 추가된 로직 ★★]
            if (clearTimeText != null)
            {
                // 1. 걸린 시간 계산 (총 시간 - 남은 시간)
                float elapsedTime = timeLimit - currentTime;

                // 2. 시간 포맷팅 (MM:SS)
                int minutes = Mathf.FloorToInt(elapsedTime / 60F);
                int seconds = Mathf.FloorToInt(elapsedTime % 60F);

                // 3. 텍스트 UI에 표시
                clearTimeText.text = string.Format("Clear Time: {0:00}:{1:00}", minutes, seconds);
            }

            if (winPanel != null)
                winPanel.SetActive(true); // 승리 패널 켜기
        }
    }

    void HandleGameOver()
    {
        isGameActive = false;
        canClick = false;

        Debug.Log("게임 오버! 시간이 초과되었습니다.");
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void PlayCardFlipSound()
    {
        if (sfxAudioSource != null && cardFlipSound != null)
        {
            sfxAudioSource.PlayOneShot(cardFlipSound);
        }
    }

    void AdjustGridSettings(int cols, int rows)
    {
        float cardAspectRatio = 150f / 220f;
        RectTransform gridRect = cardGrid.GetComponent<RectTransform>();
        float gridWidth = gridRect.rect.width;
        float gridHeight = gridRect.rect.height;
        float paddingX = gridLayout.padding.left + gridLayout.padding.right;
        float paddingY = gridLayout.padding.top + gridLayout.padding.bottom;
        float spacingX = gridLayout.spacing.x;
        float spacingY = gridLayout.spacing.y;
        float cellWidth_byWidth = (gridWidth - paddingX - (cols - 1) * spacingX) / cols;
        float cellHeight_byWidth = cellWidth_byWidth / cardAspectRatio;
        float cellHeight_byHeight = (gridHeight - paddingY - (rows - 1) * spacingY) / rows;
        float cellWidth_byHeight = cellHeight_byHeight * cardAspectRatio;
        if (cellWidth_byWidth < cellWidth_byHeight)
        {
            gridLayout.cellSize = new Vector2(cellWidth_byWidth, cellHeight_byWidth);
        }
        else
        {
            gridLayout.cellSize = new Vector2(cellWidth_byHeight, cellHeight_byHeight);
        }
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = cols;
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    IEnumerator CheckMatch()
    {
        yield return new WaitForSeconds(1.0f);
        if (firstCard.cardID == secondCard.cardID)
        {
            firstCard.SetMatched();
            secondCard.SetMatched();
        }
        else
        {
            firstCard.FlipToBack();
            secondCard.FlipToBack();
            yield return new WaitForSeconds(cardFlipDuration);
        }
        firstCard = null;
        secondCard = null;
        canClick = true;
        CheckForWin();
    }
}
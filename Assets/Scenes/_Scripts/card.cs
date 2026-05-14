using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// CardPrefab에 붙어있어야 합니다.
public class Card : MonoBehaviour
{
    // === 변수 ===
    public int cardID;
    public Sprite cardFront;
    public Sprite cardBack;
    public bool isMatched = false;

    [Header("애니메이션 설정")]
    public float flipDuration = 0.3f; // 카드가 뒤집히는 데 걸리는 시간
    private bool isFlipping = false;  // 현재 뒤집히는 애니메이션 중인지 확인

    private Image cardImage;
    private Button button;
    private GameManager gameManager;

    // === 함수 ===

    void Start()
    {
        cardImage = GetComponent<Image>();
        button = GetComponent<Button>();
        gameManager = FindAnyObjectByType<GameManager>();
        button.onClick.AddListener(OnCardClicked);
    }

    // 카드가 클릭되었을 때 호출되는 함수
    public void OnCardClicked()
    {
        if (isFlipping || isMatched)
        {
            return;
        }

        // 일반 카드는 GameManager에게 알림
        gameManager.CardClicked(this);
    }

    public void FlipToFront()
    {
        StartCoroutine(DoFlip(cardFront));
    }

    public void FlipToBack()
    {
        StartCoroutine(DoFlip(cardBack));
    }

    public void SetMatched()
    {
        isMatched = true;
        button.interactable = false;
    }

    // 실제 뒤집기 애니메이션을 처리하는 코루틴
    private IEnumerator DoFlip(Sprite targetSprite)
    {
        isFlipping = true;

        // [★★ 효과음 재생 ★★]
        gameManager.PlayCardFlipSound(); // 게임매니저에게 소리 재생 요청!

        Vector3 originalScale = transform.localScale;
        float halfDuration = flipDuration / 2.0f;
        float timer = 0f;

        // 1. 납작하게
        while (timer < halfDuration)
        {
            float newX = Mathf.SmoothStep(originalScale.x, 0f, timer / halfDuration);
            transform.localScale = new Vector3(newX, originalScale.y, originalScale.z);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localScale = new Vector3(0, originalScale.y, originalScale.z);

        // 2. 스프라이트 교체
        cardImage.sprite = targetSprite;

        // 3. 펼치기
        timer = 0f;
        while (timer < halfDuration)
        {
            float newX = Mathf.SmoothStep(0f, originalScale.x, timer / halfDuration);
            transform.localScale = new Vector3(newX, originalScale.y, originalScale.z);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localScale = originalScale;
        isFlipping = false;
    }
}
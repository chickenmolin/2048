using System.Collections;
using TMPro;
using UnityEngine;

[DefaultExecutionOrder(-1)] // Chạy trước các script khác
public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    [SerializeField] private TileBoard board;
    [SerializeField] private CanvasGroup gameOver; // Dùng CanvasGroup để fade UI
    [SerializeField] private TextMeshProUGUI scoreText, hiscoreText;

    public int score { get; private set; } = 0;

    private void Awake() {
        // Singleton: hủy nếu đã tồn tại instance khác
        if (Instance != null) DestroyImmediate(gameObject);
        else Instance = this;
    }

    private void OnDestroy() {
        if (Instance == this) Instance = null;
    }

    public void NewGame() {
        SetScore(0);
        hiscoreText.text = LoadHiscore().ToString();

        gameOver.alpha = 0f;           // Ẩn UI game over
        gameOver.interactable = false;

        board.ClearBoard();
        board.CreateTile();            // Tạo 2 ô đầu tiên
        board.CreateTile();
        board.enabled = true;
    }

    public void GameOver() {
        board.enabled = false;         // Khóa input bàn cờ
        gameOver.interactable = true;
        StartCoroutine(Fade(gameOver, 1f, 1f)); // Fade in UI sau 1s
    }

    // Fade alpha CanvasGroup từ giá trị hiện tại đến `to` trong 0.5s
    private IEnumerator Fade(CanvasGroup canvasGroup, float to, float delay = 0f) {
        yield return new WaitForSeconds(delay);
        float elapsed = 0f, duration = 0.5f;
        float from = canvasGroup.alpha;
        while (elapsed < duration) {
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = to;
    }

    public void IncreaseScore(int points) => SetScore(score + points);

    private void SetScore(int score) {
        this.score = score;
        scoreText.text = score.ToString();
        SaveHiscore(); // Lưu ngay mỗi khi điểm thay đổi
    }

    // Chỉ lưu khi điểm hiện tại cao hơn kỷ lục
    private void SaveHiscore() {
        if (score > LoadHiscore())
            PlayerPrefs.SetInt("hiscore", score);
    }

    private int LoadHiscore() => PlayerPrefs.GetInt("hiscore", 0);
}

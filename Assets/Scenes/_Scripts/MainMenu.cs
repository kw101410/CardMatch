using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위해 필수!

public class MainMenu : MonoBehaviour
{
    // (Start, Update 함수는 필요 없으므로 삭제)
    // (패널 변수들 삭제됨)

    // '게임 시작' 버튼 함수가 삭제되었습니다.

    // 난이도 버튼을 눌렀을 때 (쉬움=0, 보통=1, 어려움=2)
    public void OnDifficultySelect(int difficulty)
    {
        // 선택한 난이도를 PlayerPrefs에 "Difficulty"라는 키로 저장
        PlayerPrefs.SetInt("Difficulty", difficulty);

        // "MainGame" 씬(Build Settings 1번)을 로드
        SceneManager.LoadScene("Scenes/SampleScene");
    }

    // '게임 종료' 버튼을 눌렀을 때
    public void OnQuitButtonClick()
    {
        Debug.Log("게임 종료!");
        Application.Quit();
    }
}
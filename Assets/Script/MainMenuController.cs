using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    private const string DailySudokuSceneName = "DailySudoku";

    public void OpenDailySudoku()
    {
        SceneManager.LoadScene(DailySudokuSceneName);
    }
}

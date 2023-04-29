using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameController : MonoBehaviour
{
    [SerializeField] private GameObject _ui;

    private void OnEnable()
    {
        GameStateManager.OnEndEnter += ShowUi;
    }

    private void OnDisable()
    {
        GameStateManager.OnEndEnter -= ShowUi;
    }

    private void ShowUi()
    {
        _ui.SetActive(true);
    }

    public void NavigateHome()
    {
        SceneManager.LoadScene("Scenes/MainMenu");
    }
}
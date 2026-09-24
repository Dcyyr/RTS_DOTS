using UnityEngine;

public class GameOverUI : MonoBehaviour
{

    private void Start()
    {
        DOTSEventsManager.instance.OnHQDead += OnHQDead;
        Hide();
    }

    private void OnHQDead(object sender, System.EventArgs e)
    {
        Show();
        Time.timeScale = 0;
    }

    public void Show()
    {
        gameObject.SetActive(true);

    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}

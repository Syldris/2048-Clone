using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class UIController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI textCurrentScore;
    [SerializeField]
    private TextMeshProUGUI textHighScore;
    [SerializeField]
    private GameObject panelGameOver;

    public void UpdateCurrentScore(int score)
    {
        textCurrentScore.text = score.ToString();
    }
    public void UpdateHighScore(int score)
    {
        textHighScore.text = score.ToString();
    }
    public void OnClickToMain()
    {
        SceneManager.LoadScene(0);
    }
    
    public void OnClickRestart()
    {
        SceneManager.LoadScene(1);
    }

    public void OnGameOver()
    {
        panelGameOver.SetActive(true);
    }
}

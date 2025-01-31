using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
public class MainScenario : MonoBehaviour
{
    [SerializeField]
    private Image imageMatrix;
    [SerializeField]
    private TextMeshProUGUI textMatrix;
    [SerializeField]
    private Sprite[] spriteMatrix;

    private int matrixIndex = 0;

    public void OnClickGameStart()
    {
        // matrixIndex가 0일 때 블록 개수는 3x3
        PlayerPrefs.SetInt("BlockCount",matrixIndex+3);
        // "02Game" 씬으로 이동
        SceneManager.LoadScene(1);
    }

    public void OnClickGameExit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
        #else
        Application.Quit();
        #endif
    }

    public void OnClickLeft()
    {
        matrixIndex = matrixIndex > 0 ? matrixIndex - 1 : spriteMatrix.Length - 1;

        imageMatrix.sprite = spriteMatrix[matrixIndex];
        textMatrix.text = spriteMatrix[matrixIndex].name;
    }

    public void OnClickRight()
    {
        matrixIndex = matrixIndex < spriteMatrix.Length - 1 ? matrixIndex + 1 : 0;

        imageMatrix.sprite = spriteMatrix[matrixIndex];
        textMatrix.text = spriteMatrix[matrixIndex].name;
    }
}

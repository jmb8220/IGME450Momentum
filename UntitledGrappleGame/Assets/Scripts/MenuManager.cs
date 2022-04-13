using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject MainMenu;
    [SerializeField] private GameObject LevelSelect;
    [SerializeField] private GameObject Settings;

    [SerializeField] private Text sensValLabel;
    [SerializeField] private Slider sensSlider;

    public float sensitivity = 100;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
    }
    void Update()
    {
        sensSlider.onValueChanged.AddListener(delegate { UpdateSensitivity(); });
    }

    public void MainMenuOpen()
    {
        MainMenu.SetActive(true);
        LevelSelect.SetActive(false);
        Settings.SetActive(false);
    }

    public void LevelSelectMenu()
    {
        MainMenu.SetActive(false);
        LevelSelect.SetActive(true);
        Settings.SetActive(false);
    }
    public void SettingsMenu()
    {
        MainMenu.SetActive(false);
        LevelSelect.SetActive(false);
        Settings.SetActive(true);
    }

    public void LoadLevel(int levelVal)
    {
        SceneManager.LoadScene(1); //1 is the gameplay scene
        SceneManager.LoadScene(levelVal + 1, LoadSceneMode.Additive);
        //levelVal corresponds with the number on the level button
    }

    public void UpdateSensitivity()
    {
        sensitivity = sensSlider.value;
        sensValLabel.text = sensitivity.ToString("F1");
    }
}
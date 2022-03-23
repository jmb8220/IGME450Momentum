using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject MainMenu;
    [SerializeField] private GameObject LevelSelect;
    
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
    }

    public void MainMenuOpen()
    {
        MainMenu.SetActive(true);
        LevelSelect.SetActive(false);
    }

    public void LevelSelectMenu()
    {
        MainMenu.SetActive(false);
        LevelSelect.SetActive(true);
    }

    public void LoadLevel(int levelVal)
    {
        SceneManager.LoadScene(1); //1 is the gameplay scene
        SceneManager.LoadScene(levelVal + 1, LoadSceneMode.Additive);
        //levelVal corresponds with the number on the level button
    }
}
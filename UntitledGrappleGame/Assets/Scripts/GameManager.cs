using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    public Canvas timer;
    [SerializeField]
    public Canvas airTimer;
    [SerializeField]
    public Text endTimer;
    [SerializeField]
    public Text endAirTimer;
    [SerializeField]
    public GameObject player;

    [SerializeField] public GameObject PauseMenu;
    [SerializeField] public GameObject EndScene;

    public Vector3 resetPos;

    public static GameManager Instance; // A static reference to the GameManager instance

    void Awake()
    {
        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            if (Instance == null) // If there is no instance already
            {
                DontDestroyOnLoad(gameObject); // Keep the GameObject, this component is attached to, across different scenes
                Instance = this;
            }
            else if (Instance != this) // If there is already an instance and it's not `this` instance
            {
                Destroy(gameObject); // Destroy the GameObject, this component is attached to
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        CrossScript.EndScene = EndScene;
        CrossScript.gameOver = false;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && SceneManager.GetActiveScene().buildIndex != 0 && !CrossScript.gameOver)
        {
            PauseGame();
        }
    }

    private void PauseGame()
    {
        Time.timeScale = 0;
        AudioListener.pause = true;
        Cursor.lockState = CursorLockMode.None;
        PauseMenu.SetActive(true);
    }
    public void ResumeGame()
    {
        Time.timeScale = 1;
        AudioListener.pause = false;
        Cursor.lockState = CursorLockMode.Locked;
        PauseMenu.SetActive(false);
    }
    public void RestartGame()
    {
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        AudioListener.pause = false;
        PauseMenu.SetActive(false);
        EndScene.SetActive(false);
        Destroy(gameObject);
        Instance = null;
        LoadLevel(SceneManager.GetActiveScene().buildIndex);
    }
    public void LoadLevel(int levelVal)
    {
        SceneManager.LoadScene(1); //1 is the gameplay scene
        SceneManager.LoadScene(CrossScript.currentLevelIndex, LoadSceneMode.Additive);
        //levelVal corresponds with the number on the level button
    }
    public void ReturnToMenu()
    {
        //Return player to main menu
        Time.timeScale = 1;
        AudioListener.pause = false;
        Destroy(gameObject);
        Instance = null;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene(0);
    }
}

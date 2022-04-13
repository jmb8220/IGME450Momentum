using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    public Canvas timer;
    [SerializeField]
    public Canvas airTimer;
    [SerializeField]
    public GameObject player;

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
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && SceneManager.GetActiveScene().buildIndex != 0)
        {
            PauseGame();
        }
    }

    private void PauseGame()
    {
        //Return player to main menu
        Destroy(gameObject);
        Instance = null;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene(0);
    }
}

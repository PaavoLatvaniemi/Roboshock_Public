using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scenetransistion : MonoBehaviour
{
    int sceneNumber;
    public void LoadMainScene()
    {
        var sceneLoading = SceneManager.LoadSceneAsync(2);
        
    }
    public void LoadTutorial()
    {
        SceneManager.LoadScene(1);
    }

    private void Start()
    {
        sceneNumber = SceneManager.GetActiveScene().buildIndex;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LoadNextScene();
        }
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(sceneNumber+1);
    }
}

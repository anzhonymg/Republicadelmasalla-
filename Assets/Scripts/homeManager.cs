using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class homeManager : MonoBehaviour
{
    bool isloading;
    public void LoadGame()
    {
        if(!isloading)
        {
            isloading = true;
            SceneManager.LoadSceneAsync("Loading");
        }
    }
}

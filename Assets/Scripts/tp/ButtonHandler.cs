using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{

    public void LoadScene(string scene)
    {
        Debug.Log("load " + scene);
        SceneManager.LoadScene(scene);
    }
}

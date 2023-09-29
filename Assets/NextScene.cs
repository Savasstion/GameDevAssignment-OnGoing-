using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.F))
            {
            LoadNextScene();
        }
    }
    public void LoadNextScene()
    {      
        SceneManager.LoadScene("Story3");
    }

}

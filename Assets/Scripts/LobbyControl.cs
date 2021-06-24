using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LobbyControl : MonoBehaviour
{
    public GameObject camaraTest;
    public GameObject paneles;
    public TextMeshProUGUI TextWait;
    // Start is called before the first frame update
    void Start()
    {
        if(ConnectionServer.instance != null && camaraTest)
        {
            Destroy(camaraTest);
        }

        if(ConnectionServer.instance.currentRoomData.PlayerCount == 3)
        {
            allUsersOK();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void allUsersOK()
    {
        StartCoroutine(timeBack());
    }

    IEnumerator timeBack()
    {
        for (int i = 10; i>0;i--)
        {
            TextWait.text = "Iniciando en " + i;
            yield return new WaitForSeconds(1);
        }

        DisableBox();
    }

    public void DisableBox()
    {
        paneles.SetActive(false);
        TextWait.gameObject.SetActive(false);
    }
}

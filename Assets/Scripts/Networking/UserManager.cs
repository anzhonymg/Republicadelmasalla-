using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkClases;

public class UserManager : MonoBehaviour
{
    public userData DatosDeUsuario;
    public static UserManager instancia;
    public static int MyTeam;

    public static bool isLogged;
    // Start is called before the first frame update
    private void Awake()
    {
        if (instancia != null)
            Destroy(this.gameObject);

        if(PlayerPrefs.HasKey("myKey"))
        {
            isLogged = true;
        }
        else
        {
            Debug.Log("Not exist myKey");
        }
    }

    void Start()
    {
        instancia = this;
        DontDestroyOnLoad(this.gameObject);

    }
}

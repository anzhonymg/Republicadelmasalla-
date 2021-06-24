using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class loadingManager : MonoBehaviour
{
    [Header("Zona de login")]
    public GameObject PanelLogin;
    public TMP_InputField UserInputLogin, PassInputLogin;

    [Header("Zona de registro")]
    public GameObject PanelRegister;
    public TMP_InputField UserInputReg, PassInputReg, nameReg;
    public TMP_InputField nacionalidad;

    [Header("Zona de genero")]
    public GameObject panelSending;

    public GameObject panelLoading;

    public Slider barraCarga;
    public TextMeshProUGUI textStatus;
    public int progress;

    public int procesosTotales;

    #region zona privada
    int procesosActuales;

    bool failLogged;
    bool isWaitVerify;
    Coroutine startAppProcess;

    bool isLoadingLevel;
    bool isLoadingScene;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        barraCarga.maxValue = 100;

        if (!isLoadingLevel)
            startAppProcess = StartCoroutine(StartingApp());
    }

    IEnumerator StartingApp(bool alreadyConnect = false)
    {
        PanelLogin.SetActive(false);
        panelSending.SetActive(false);
        PanelRegister.SetActive(false);

        isLoadingLevel = true;
        textStatus.text = "conectando al servidor";
        float progressFLoat = progress;
        //float amountAdd = (100 / procesosTotales) / 100;

        if (!alreadyConnect)
        {
            while (ConnectionServer.instance == null)
            {
                yield return null;
            }

            while (!ConnectionServer.instance.IsConnected)
            {
                yield return null;
            }

            procesosActuales = 1;


            while (progressFLoat < 30)
            {
                progressFLoat += 1f;
                barraCarga.value = progressFLoat;
                yield return new WaitForSeconds(0.05f);
            }

            progress = (int)progressFLoat;
        }
        else
        {
            barraCarga.value = 30;
            progress = 30;

        }

        if (UserManager.isLogged)
        {

            checkUser();
            isWaitVerify = true;
            Debug.Log("chequeando en el servidor el usuario");
            int chequeos = 0;
            while (isWaitVerify)
            {
                if (UserControl.isLogged)
                {
                    isWaitVerify = false;
                }

                if (failLogged || chequeos >= 40)
                {
                    isWaitVerify = false;
                    PanelLogin.SetActive(true);
                    panelLoading.SetActive(false);
                    isLoadingLevel = false;
                    yield break;
                }
                chequeos++;
                yield return null;
            }
        }
        else
        {
            PanelLogin.SetActive(true);
            panelLoading.SetActive(false);
            isLoadingLevel = false;
            yield break;
        }


        if (!isLoadingScene)
        {
            isLoadingScene = true;

            var sincScene = SceneManager.LoadSceneAsync("Inicio");
            sincScene.allowSceneActivation = false;

            float faltante = 100 - progress;
            int progressBeforeLoad = progress;

            while (sincScene.progress < 0.9f)
            {
                int val_ = (int)(sincScene.progress * faltante);
                progress = progressBeforeLoad + val_;
                barraCarga.value = progress;
                yield return new WaitForSeconds(0.05f);
            }

            sincScene.allowSceneActivation = true;
            barraCarga.value = 100;

            textStatus.text = "Cargando Lobby";
        }


        // hay que cargar la sala lobby peroo primero hay que ir a la escena de seleccion de genero

        /*
        Debug.Log("entrando a la sala");

        NetworkClases.RoomData roomData = new NetworkClases.RoomData();
        roomData.IsVisible = true;
        roomData.maxPlayer = 20;
        roomData.name = "Lobby";
        roomData.SyncScene = true;

        ConnectionServer.instance.JoinOrCreateRoomByType("MallScene", roomData);

        while (!ConnectionServer.instance.inRoom)
        {
            yield return null;
        }

        textStatus.text = "Entrando a la sala";

        while (progressFLoat < 60)
        {
            progressFLoat += 1;
            barraCarga.value = progressFLoat;
            yield return new WaitForSeconds(0.05f);
        }
        progress = (int)progressFLoat;

        if (!isLoadingScene)
        {
            isLoadingScene = true;

            var sincScene = SceneManager.LoadSceneAsync("Social Plaza");
            sincScene.allowSceneActivation = false;

            float faltante = 100 - progress;
            int progressBeforeLoad = progress;

            while (sincScene.progress < 0.9f)
            {
                int val_ = (int)(sincScene.progress * faltante);
                progress = progressBeforeLoad + val_;
                barraCarga.value = progress;
                yield return new WaitForSeconds(0.05f);
            }

            sincScene.allowSceneActivation = true;
            barraCarga.value = 100;

            textStatus.text = "Cargando entorno";
        }

        isLoadingLevel = false;
        */
    }

    public void Login()
    {
        if (!(UserInputLogin.text.Length > 4 && UserInputLogin.text.Contains("@") && PassInputLogin.text.Length >= 8))
        {
            ToastMessage.instancia.showToastOnUiThread("Completa el formulario primero!");
            return;
        }

        if (startAppProcess != null)
        {
            StopCoroutine(startAppProcess);
        }

        JSONObject data = new JSONObject();
        data.AddField("mail", UserInputLogin.text);
        data.AddField("pass", PassInputLogin.text);

        panelSending.SetActive(true);
        ConnectionServer.instance.Emit("loginUser", data);
    }

    public void Register()
    {
        string[] nameAndLast = nameReg.text.Split(' ');
        string name_ = nameAndLast[0];
        string lastName = nameAndLast[1];

        if ((UserInputReg.text.Length > 4 && UserInputReg.text.Contains("@") && PassInputReg.text.Length >= 8 &&
            name_.Length >= 4 && lastName.Length >= 4) == false)
        {
            ToastMessage.instancia.showToastOnUiThread("Completa el formulario primero!");
            Debug.Log("Completa el formulario primero!");
            return;
        }

        if (startAppProcess != null)
        {
            StopCoroutine(startAppProcess);
        }



        JSONObject data = new JSONObject();
        data.AddField("mail", UserInputReg.text);
        data.AddField("pw", PassInputReg.text);
        data.AddField("nombre", nameReg.text);
        data.AddField("apellido", lastName);
        data.AddField("telefono", "unknow");
        string empresa_ = "No Posee";
        data.AddField("empresa", empresa_);
        data.AddField("nacionalidad", "unknow");

        panelSending.SetActive(true);
        ConnectionServer.instance.Emit("registerUser", data);
    }

    public void responseLogin(bool response, string motivo = "")
    {
        panelSending.SetActive(false);
        Debug.Log("responseLogin called: response" + response);
        if (response)
        {
            panelLoading.SetActive(true);
            Debug.Log("Se supone que se activo el panel loading");

            failLogged = false;

            if (!isWaitVerify)
            {
                if (startAppProcess != null)
                {
                    StopCoroutine(startAppProcess);

                }

                if (!isLoadingLevel)
                    StartCoroutine(StartingApp());
            }
        }
        else
        {
            failLogged = true;
            ToastMessage.instancia.showToastOnUiThread("Error al iniciar sesion: " + motivo);
        }
    }

    public void responseRegister(bool response, string motivo = "")
    {
        panelSending.SetActive(false);
        if (response)
        {
            panelLoading.SetActive(true);
            if (!isLoadingLevel)
                StartCoroutine(StartingApp());
        }
        else
        {
            ToastMessage.instancia.showToastOnUiThread("Error al registrarte: " + motivo);
        }
    }

    public void checkUser()
    {
        if (PlayerPrefs.HasKey("myKey"))
        {
            JSONObject data = new JSONObject();
            string key_ = PlayerPrefs.GetString("myKey");

            data.AddField("llave", key_);

            Debug.Log("revisando llave");

            ConnectionServer.instance.Emit("VerifyUser", data);
        }
    }

    public void ChangePanelReg(bool b_)
    {
        PanelRegister.SetActive(b_);
    }
}

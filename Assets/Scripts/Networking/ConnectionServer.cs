using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using UnityEngine.SceneManagement;
using NetworkClases;
using UnityEngine.Events;

public class ConnectionServer : SocketIOComponent
{
    public static ConnectionServer instance;
    public List<string> ScenesMultiplayer;

    public bool isTest;

    public string SceneMain;

    public static bool InRoom;
    public static bool IsMasterClient;

    bool isDisconected, failConnection;
    float timeClock;
    int intentsFail = 0;

    public long myPing;

    public delegate void OnEventFromServer();
    public delegate void OnFailJoinedRoom(string msj);
    public delegate void OnRoomIsFull();

    public OnFailJoinedRoom onConnectionFail;

    public OnEventFromServer onJoinedRoom;
    public OnEventFromServer onPlayerEnter;
    public OnEventFromServer OnRoomIsReady;

    public OnFailJoinedRoom onFailJoinedRoom;
    public OnRoomIsFull onRoomIsFull;


    public RoomData currentRoomData;
    public GameObject playerLocal;
    //show how user are connected to server
    public static int PlayerCount;
    public static int TeamIndex;

    [HideInInspector]
    public bool WasReturnToLobby;

    [HideInInspector]
    public bool inRoom;

    public float maxWaitPlayers = 20;


    float timeWaitingPlayer;
    bool isWaitingPlayers;
    string myKeyTemp;

    public float TimeToCheckPing = 10;
    float clockPing;

    long timePing;
    long timePong;
    private void OnEnable()
    {
        if (isTest)
        {
            url = "ws://localhost:52300/socket.io/?EIO=4&transport=websocket";
        }
    }


    public override void Start()
    {
        base.Start();

        if (instance != null)
            Destroy(this.gameObject);

        instance = this;
        DontDestroyOnLoad(this.gameObject);


        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        SetCallBacks();

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
    //---------------------------------------------------------------
    public void OnConnectedToMyServer()
    {
        if (SceneManager.GetActiveScene().name != "SceneMain" && !isMultiplayerScene(SceneManager.GetActiveScene().name))
        {

        }

        Debug.Log("connected to master");

        failConnection = false;
        //transform.GetChild(0).gameObject.SetActive(false);
    }
    //---------------------------------------------------------------
    private bool isMultiplayerScene(string s_)
    {
        if (ScenesMultiplayer.Contains(s_))
        {
            return true;
        }

        return false;
    }
    //---------------------------------------------------------------
    public override void Update()
    {
        base.Update();

        if (failConnection && !isDisconected)
        {
            timeClock += Time.deltaTime;
            if (timeClock >= 15)
            {
                if (SceneManager.GetActiveScene().name == "Loading")
                {
                    timeClock = 0;
                    intentsFail++;

                    if (intentsFail > 10)
                    {
                        isDisconected = true;

                    }

                    if (!transform.GetChild(0).gameObject.activeSelf)
                        transform.GetChild(0).gameObject.SetActive(true);
                }
            }
        }

        if (isWaitingPlayers)
        {
            /*
            timeWaitingPlayer += Time.deltaTime;
            if (timeWaitingPlayer > maxWaitPlayers)
            {
                timeWaitingPlayer = 0;
                isWaitingPlayers = false;

                if (IsMasterClient)
                    Emit("CreateBotsForRoom");
            }
            */
        }

        if (IsConnected)
        {
            /*
            clockPing += Time.deltaTime;
            if (clockPing >= TimeToCheckPing)
            {
                clockPing = 0;
                Emit("Ping");
                System.DateTime data_ = System.DateTime.Now;
                timePing = ((System.DateTimeOffset)data_).ToUnixTimeMilliseconds();
            }
            */
        }
    }
    public void CheckPing()
    {
        System.DateTime data_ = System.DateTime.Now;
        timePong = ((System.DateTimeOffset)data_).ToUnixTimeMilliseconds();
        myPing = timePong - timePing;
    }

    //---------------------------------------------------------------
    public string deleteQuotes(string str_)
    {
        str_ = str_.Replace("\"", "");
        return str_;
    }
    //---------------------------------------------------------------
    public override void OnError(object sender, WebSocketSharp.ErrorEventArgs e)
    {
        print("no te pudiste conectar al servidor, motivo: " + e.Message);
        failConnection = true;

        onConnectionFail(e.Message);
    }
    //---------------------------------------------------------------
    //Call back SET
    private void SetCallBacks()
    {
        On("setKeyTemp", (e) => {
            myKeyTemp = deleteQuotes(e.data["id"].ToString());
            //Debug.Log("Key ID is: " + myKeyTemp);
            OnConnectedToMyServer();
        });

        On("LoginResponse", (e) => {
            string status = deleteQuotes(e.data["estado"].ToString());
            if (status == "ok")
            {
                print("user logged success");
                userData user__ = new userData();


                UserManager.isLogged = true;
                UserControl.isLogged = true;
                user__.keyUser = e.data["datos"]["key"].str;
                user__.genero = (int)e.data["datos"]["genero"].f;
                user__.nombre   = deleteQuotes(e.data["datos"]["nombre"].ToString());
                user__.apellido = deleteQuotes(e.data["datos"]["apellido"].ToString());
                //user__.telefono = deleteQuotes(e.data["datos"]["telefono"].ToString());


                user__.loginData = System.DateTime.Now.ToString();

                UserManager.instancia.DatosDeUsuario = user__;
                PlayerPrefs.SetString("myKey", e.data["datos"]["key"].str);

                print("datos saved");

                //debe ser implmentado y corregido
                
                loadingManager menu = FindObjectOfType<loadingManager>();
                if (menu)
                {
                    print("calling responseLogin");
                    menu.responseLogin(true);
                }
                else
                {
                    Debug.Log("no se consiguio el menu");
                }
                
            }
            else
            {
                print("user logged fail. reason: " + e.data["motivo"].ToString());
                
                loadingManager menu = FindObjectOfType<loadingManager>();
                if (menu)
                {
                    menu.responseLogin(false, e.data["motivo"].str);
                }
                
            }
        });

        On("SignupResponse", (e) => {
            string status = deleteQuotes(e.data["estado"].ToString());
            if (status == "ok")
            {
                print("user logged success");
                userData user__ = new userData();

                user__.keyUser = deleteQuotes(e.data["datos"]["key"].ToString());
                user__.nombre = deleteQuotes(e.data["datos"]["nombre"].ToString());

                user__.loginData = System.DateTime.Now.ToString();

                UserManager.isLogged = true;
                UserControl.isLogged = true;

                UserManager.instancia.DatosDeUsuario = user__;

                loadingManager menu = FindObjectOfType<loadingManager>();
                
                if (menu)
                {
                    menu.responseLogin(true);
                }
                else
                {
                    Debug.Log("Fail to find menu");
                }
                
            }
            else
            {
                print("user signup fail. reason: " + e.data["motivo"].ToString());

                /*
                Menu menu = FindObjectOfType<Menu>();
                if (menu)
                {
                    menu.responseRegister(false, e.data["motivo"].str);
                }
                else
                {
                    Debug.Log("no se consiguio el menu");
                }
                */
            }
        });

        On("updatePlayerCount", (e) => {
            string count__ = deleteQuotes(e.data["playerCount"].ToString());
            PlayerCount = int.Parse(count__);
        });

        On("OnJoinedRoom", (e) => {
            //Debug.Log("Joined room success! ");
            //onJoinedRoom();
            instance.SetRoomData(e);
            inRoom = true;

        });

        On("Pong", (e) => {
            CheckPing();
        });

        On("OnPlayerEnter", (e) => {
            if (currentRoomData != null)
            {
                Debug.Log("se actualizo la lista de jugadores " + e.data.ToString() );

                currentRoomData.PlayerCount = int.Parse(e.data["playerCount"].str);

                if(currentRoomData.PlayerCount >= 3)
                {
                    LobbyControl lobby_ = FindObjectOfType<LobbyControl>();
                    if(lobby_ != null)
                    { 
                        lobby_.allUsersOK();
                    }else{
                        Debug.Log("Noy hay lobby control");
                    }
                }else{
                    Debug.LogFormat("Fatan Jugadores {0}/3", currentRoomData.PlayerCount);
                }

                ChangePlayerListRoom(e.data["players"]);
            }

            //Debug.Log("A player enter in room");

            onPlayerEnter();
        });

        On("OnPlayerLeft", (e) => {
            if (currentRoomData != null)
            {
                currentRoomData.PlayerCount = int.Parse(deleteQuotes(e.data["playerCount"].ToString()));
                ChangePlayerListRoom(e.data["players"]);
            }

            //Debug.Log("A player enter in room");

            onPlayerEnter();
        });

        On("StartMultiplayerGame", (e) => {

            //Debug.Log("Room Is Ready for play");
            OnRoomIsReady();
            //Debug.Log("OnRoomIsReady was executed");
        });

        On("ChangeSceneAsync", (e) => {
            print("changing chene to scene in server");

            string sceneName = deleteQuotes(e.data["scene"].ToString());
            SceneManager.LoadSceneAsync(sceneName);
        });

        On("DeletePlayerObjects", (e) => {
            string makr = e.data["Maker"].str;
            ////Debug.Log(makr + " left room");

            NetworkView[] nv__ = FindObjectsOfType<NetworkView>();
            for (int i = nv__.Length - 1; i >= 0; i--)
            {
                if (nv__[i].Maker == makr)
                {
                    Destroy(nv__[i].gameObject);
                }
            }
        });

        On("InstantiateObjectInClient", (e) => {
            ////Debug.Log("InstantiateObjectInClient was called");
            Debug.Log("InstantiateObjectInClient was called, object: " + e.data["name"].str);
            if (e.data["isMine"].b)
            {
                Debug.Log("el objeto " + e.data["name"].str + " es mio");

                NetworkView[] nv__ = FindObjectsOfType<NetworkView>();
                bool objectHaveFinded = false;
                for (int i = 0; i < nv__.Length; i++)
                {
                    if (nv__[i].gameObject.GetInstanceID().ToString() == e.data["InstanceID"].str)
                    {
                        nv__[i].ObjectID = e.data["ID"].str;
                        nv__[i].Maker = e.data["maker"].str;
                        nv__[i].SetPadre(nv__[i].Maker);
                        objectHaveFinded = true;

                        ////Debug.Log("InstantiateObjectInClient was called, maker: " + e.data["maker"].str);
                    }
                }

                if (!objectHaveFinded)
                {
                    GameObject object_ = Resources.Load(e.data["name"].str) as GameObject;
                    Vector3 pos = new Vector3();
                    Quaternion rot = new Quaternion();

                    string[] posParts = e.data["pos"].str.Split('/');
                    string[] rotParts = e.data["rot"].str.Split('/');

                    string xPos = posParts[0].Replace(',', '.');
                    string yPos = posParts[1].Replace(',', '.');
                    string zPos = posParts[2].Replace(',', '.');

                    float x_ = float.Parse(xPos, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
                    float y_ = float.Parse(yPos, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
                    float z_ = float.Parse(zPos, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);

                    string xRot = rotParts[0].Replace(',', '.');
                    string yRot = rotParts[1].Replace(',', '.');
                    string zRot = rotParts[2].Replace(',', '.');
                    string wRot = rotParts[3].Replace(',', '.');

                    float xR_ = float.Parse(xRot, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
                    float yR_ = float.Parse(yRot, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
                    float zR_ = float.Parse(zRot, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
                    float wR_ = float.Parse(wRot, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);

                    pos = new Vector3(x_, y_, z_);
                    rot = new Quaternion(xR_, yR_, zR_, wR_);

                    GameObject go_ = Instantiate(object_, pos, rot);

                    if (e.data["name"].str.ToLower().Contains("player"))
                    {
                        GameObject PointSpawn = GameObject.Find("[PointSpawn]");

                        if (PointSpawn != null)
                        {
                            Debug.Log("El PointSpawn not is null, asignando pos");

                            int posIndex = Random.Range(0, PointSpawn.transform.childCount - 1);
                            pos = PointSpawn.transform.GetChild(posIndex).position;
                            rot = PointSpawn.transform.GetChild(posIndex).rotation;
                        }
                        else
                        {
                            Debug.Log("El PointSpawn is null");
                            pos = new Vector3(0.4199996f, 1.46f, 5.38f);
                            rot = Quaternion.identity;
                        }

                        if (playerLocal == null)
                        {
                            playerLocal = go_;
                        }
                    }

                    ////Debug.LogFormat("se instancio un objeto en la escena " + SceneManager.GetActiveScene().name);


                    NetworkView nv_ = go_.GetComponent<NetworkView>();
                    nv_.ObjectID = e.data["ID"].str;
                    nv_.Maker = e.data["maker"].str;
                    nv_.SetPadre(nv_.Maker);

                    ////Debug.Log("InstantiateObjectInClient was called, maker: " + e.data["maker"].str);

                }
            }
            else
            {
                Debug.Log("el objeto " + e.data["name"].str + " NO es mio");

                GameObject object_ = Resources.Load(e.data["name"].str) as GameObject;
                Vector3 pos = new Vector3();
                Quaternion rot = new Quaternion();

                string[] posParts = e.data["pos"].str.Split('/');
                string[] rotParts = e.data["rot"].str.Split('/');

                string xPos = posParts[0].Replace('.', ',');
                string yPos = posParts[1].Replace('.', ',');
                string zPos = posParts[2].Replace('.', ',');

                float x_ = float.Parse(xPos, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
                float y_ = float.Parse(yPos, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
                float z_ = float.Parse(zPos, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);

                string xRot = rotParts[0].Replace('.', ',');
                string yRot = rotParts[1].Replace('.', ',');
                string zRot = rotParts[2].Replace('.', ',');
                string wRot = rotParts[3].Replace('.', ',');

                float xR_ = float.Parse(xRot, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
                float yR_ = float.Parse(yRot, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
                float zR_ = float.Parse(zRot, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
                float wR_ = float.Parse(wRot, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);

                pos = new Vector3(x_, y_, z_);
                rot = new Quaternion(xR_, yR_, zR_, wR_);

                GameObject go_ = Instantiate(object_, pos, rot);

                ////Debug.LogFormat("se instancio un objeto en la escena " + SceneManager.GetActiveScene().name);


                NetworkView nv_ = go_.GetComponent<NetworkView>();
                nv_.ObjectID = e.data["ID"].str;
                nv_.Maker = e.data["maker"].str;
                nv_.SetPadre(nv_.Maker);

                ////Debug.Log("InstantiateObjectInClient was called, maker: " + e.data["maker"].str);

            }
        });

        On("InstantiatePlayerInClient", (e) => {
            InstantiatPlayerInLocal(e);
        });

        On("InstantiatePlayerInClientIfNotExist", (e) => {
            Debug.Log("InstantiatePlayerInClientIfNotExist was called");
            CheckPlayers(e);
        });

        On("DeleteObject", (e) => {
            string objectID_ = e.data["ObjectID"].str;

            NetworkView[] nv_ = FindObjectsOfType<NetworkView>();
            for (int i = 0; i < nv_.Length; i++)
            {
                if (nv_[i].ObjectID == objectID_)
                {
                    Destroy(nv_[i].gameObject);
                }
            }
        });
    }
    //---------------------------------------------------------------
    public string GetUserKey()
    {
        return myKeyTemp;
    }
    //---------------------------------------------------------------
    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (isMultiplayerScene(arg0.name))
        {
            if (InRoom)
            {
                if (currentRoomData.SyncScene)
                {
                    JSONObject jsonData_ = new JSONObject();
                    jsonData_.AddField("room", currentRoomData.name);
                    jsonData_.AddField("scene", arg0.name);
                    //Emit("SetRoomScene", jsonData_);
                }

                Invoke("CreateMyPlayers", 0.2f);
            }
        }
        else
        {
            ////Debug.LogFormat("la escena {0} no es online", arg0.name);
        }
    }

    public void CreateMyPlayers()
    {
        if (playerLocal == null)
        {
            Vector3 pos_ = new Vector3(12, 2.2f, 3.35f);
            Quaternion rot_ = Quaternion.identity;


            GameObject PointSpawn = GameObject.Find("[PointSpawn]");
            if (PointSpawn != null)
            {
                int azar = Random.Range(0, PointSpawn.transform.childCount - 1);
                pos_ = PointSpawn.transform.GetChild(azar).position;
                rot_ = PointSpawn.transform.GetChild(azar).rotation;
            }
            ////Debug.Log("instanciando player en la scena: " + SceneManager.GetActiveScene().name);

            string genre_ = "male";

            /*
            if (Menu.generoSelect == 0)
            {
                genre_ = "female";
            }
            */

            InstantiateObjectInServer("PJ/Player_" + SelectorPlayer.playerSelect, pos_, rot_);
        }
        else
        {
            ////Debug.Log("ya existia un player local");
        }

        Emit("GetPlayersOfRoom");
    }

    //---------------------------------------------------------------
    public void InstantiateObjectInServer(string nameObject, Vector3 pos, Quaternion rot)
    {
        string pos_ = pos.x + "/" + pos.y + "/" + pos.z;
        string rot_ = rot.x + "/" + rot.y + "/" + rot.z + "/" + rot.w;

        JSONObject jsonData_ = new JSONObject();
        jsonData_.AddField("nameObject", nameObject);
        jsonData_.AddField("pos", pos_);
        jsonData_.AddField("rot", rot_);

        if (nameObject.StartsWith("Champs"))
        {
            Emit("InstantiatePlayer", jsonData_);
        }
        else
        {
            Emit("InstantiateObject", jsonData_);
        }
    }

    public GameObject InstantiateObjectInServer_GO(string nameObject, Vector3 pos, Quaternion rot)
    {
        string pos_ = pos.x + "/" + pos.y + "/" + pos.z;
        string rot_ = rot.x + "/" + rot.y + "/" + rot.z + "/" + rot.w;

        GameObject go = Instantiate(Resources.Load(nameObject) as GameObject, pos, rot);

        JSONObject jsonData_ = new JSONObject();
        jsonData_.AddField("nameObject", nameObject);
        jsonData_.AddField("pos", pos_);
        jsonData_.AddField("rot", rot_);
        jsonData_.AddField("InstanceID", go.GetInstanceID().ToString());
        Emit("InstantiateObject", jsonData_);

        return go;
    }

    public void CheckPlayers(SocketIOEvent data_)
    {
        Debug.Log("checking player. " + data_.data.ToString());

        string objectID = data_.data["ID"].str;
        NetworkView[] nv_ = FindObjectsOfType<NetworkView>();
        bool existPlayer = false;

        for (int i = 0; i < nv_.Length; i++)
        {
            if (nv_[i].ObjectID == objectID)
            {
                existPlayer = true;
            }
        }

        if (!existPlayer)
        {
            Debug.Log("instanciando player ya que no estaba");
            InstantiatPlayerInLocal(data_);
        }
        else
        {
            Debug.Log("este player ya existia en la room");
        }
    }

    public void InstantiatPlayerInLocal(SocketIOEvent e)
    {
        Debug.Log("InstantiatPlayerInLocal was called");
        Debug.Log("datos para player: " + e.data.ToString());

        GameObject object_ = Resources.Load(e.data["name"].str) as GameObject;

        Debug.Log("InstantiatPlayerInLocal, object loaded");
        Vector3 pos = new Vector3();
        Quaternion rot = new Quaternion();



        GameObject PointSpawn = GameObject.Find("[PointSpawn]");
        int posIndex = 0;// Mathf.RoundToInt(e.data["slot"].f);
                         //int team = int.Parse(e.data["Team"].str);

        Debug.Log("InstantiatPlayerInLocal, object loaded 2");


        if (PointSpawn != null)
        {
            pos = PointSpawn.transform.GetChild(posIndex).position;
            rot = PointSpawn.transform.GetChild(posIndex).rotation;
        }
        else
        {
            pos = new Vector3(12, 2.17f, 3.329997f);
            rot = Quaternion.identity;
        }

        //string pos_ = e.data["pos"].str;
        //string[] posS = pos_.Split('/');
        //pos = new Vector3(float.Parse(posS[0]), float.Parse(posS[1]), float.Parse(posS[2]));

        ////Debug.LogFormat("el jugador {0} es del team {1}", e.data["maker"].str, team);
        ////Debug.LogFormat("el jugador {0} es un objeto tipo {1}", e.data["maker"].str, e.data["name"].str);
        GameObject go_ = Instantiate(object_, pos, rot);

        DontDestroyOnLoad(go_);

        Debug.LogFormat("se instancio un objeto tipo: " + e.data["name"].str);


        NetworkView nv_ = go_.GetComponent<NetworkView>();
        nv_.ObjectID = e.data["ID"].str;
        nv_.Maker = e.data["maker"].str;
        nv_.SetPadre(nv_.Maker);


        if (playerLocal == null)
        {
            if (nv_.isMine)
                playerLocal = go_;
        }
    }

    public void DestroyObject(GameObject GO, float time_ = 0)
    {
        if (GO)
        {
            NetworkView nv_ = GO.GetComponent<NetworkView>();
            if (nv_)
            {
                if (nv_.Maker == myKeyTemp)
                {
                    JSONObject jsonData = new JSONObject();
                    jsonData.AddField("ObjectID", nv_.ObjectID);
                    jsonData.AddField("Time", time_);
                    Emit("DestroyObjectRoom", jsonData);
                }
            }
            Destroy(GO, time_);
        }
    }
    //---------------------------------------------------------------
    public void SetRoomData(SocketIOEvent e)
    {
        currentRoomData = new RoomData();

        currentRoomData.name = deleteQuotes(e.data["roomName"].ToString());
        ////Debug.Log("data: " + e.data.ToString());

        int mp_ = Mathf.RoundToInt(e.data["maxPlayer"].f);
        int pc_ = Mathf.RoundToInt(e.data["PlayerCount"].f);


        currentRoomData.maxPlayer = mp_;
        currentRoomData.PlayerCount = pc_;
        InRoom = true;
        ////Debug.Log("SetRoomData, all vars was set");
        IsMasterClient = e.data["isMasterClient"].b;

        if (IsMasterClient)
        {
            isWaitingPlayers = true;
        }
    }

    //---------------------------------------------------------------
    public void QuitGame()
    {
        if (Application.isEditor)
        {
            Debug.Break();
        }
        else
        {
            Application.Quit();
        }
    }
    //---------------------------------------------------------------
    public void JoinOrCreateRoom(string name_, RoomData room_)
    {
        JSONObject jsonData_ = new JSONObject();
        jsonData_.AddField("nombre", name_);
        jsonData_.AddField("metodo", 0);
        jsonData_.AddField("roomData", JsonUtility.ToJson(room_));
        Emit("JoinOrCreateRoom", jsonData_);
    }
    //---------------------------------------------------------------
    public void JoinOrCreateRoomByType(string type_, RoomData room_)
    {
        JSONObject jsonData_ = new JSONObject();
        jsonData_.AddField("type", type_);
        jsonData_.AddField("metodo", 1);
        string rd_ = EncryptPlugin.Base64Encode(JsonUtility.ToJson(room_));
        jsonData_.AddField("roomData", rd_);

        ////Debug.Log("jsonData:" + JsonUtility.ToJson(room_));

        Emit("JoinOrCreateRoom", jsonData_);
    }

    //---------------------------------------------------------------
    public void ReturnToLobby()
    {
        Emit("GoLobby");
        NetworkView[] nv_ = FindObjectsOfType<NetworkView>();

        foreach (NetworkView nv in nv_)
        {
            Destroy(nv.gameObject);
        }
        SceneManager.LoadScene("Lobby");

        WasReturnToLobby = true;
    }

    public void ChangePlayerListRoom(JSONObject json_)
    {
        List<PlayerRoom> pr_ = new List<PlayerRoom>();
        for (int i = 0; i < json_.Count; i++)
        {
            bool isLocal = json_[i]["id"].str == myKeyTemp;
            PlayerRoom pr__ = new PlayerRoom();
            pr__.isLocal = isLocal;
            pr__.NickName = json_[i]["username"].str;
            pr__.UserId = json_[i]["id"].str;
            pr__.Team = json_[i]["currentTeam"].f.ToString();
            pr_.Add(pr__);
        }
    }
}

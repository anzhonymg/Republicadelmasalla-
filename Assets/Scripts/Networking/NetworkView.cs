using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkView : MonoBehaviour
{
    public enum TipoRPC { all, other }

    List<Object> ObjetosSend;

    public string ObjectID = string.Empty;
    public string Maker;
    public bool isMine { get { return IsMine; } }
    private bool IsMine = false;

    public float delayUpdate;
    float timer_ = 0;


    public bool isReady = false;

    private void Awake()
    {
        if (ConnectionServer.instance == null)
        {
            IsMine = true;
            return;
        }
    }

    private void Start()
    {
        isReady = false;
        DontDestroyOnLoad(this.gameObject);
        SetPadre(Maker);

        if(ObjectID == string.Empty)
        {
            string newID = "";
            string content_ = "aqswderfgtyhujkiolpmnbvcxz123456789-";
            for (int i = 0; i < 10; i++)
            {
                newID += content_.Substring(Random.Range(0, content_.Length - 1), 1);
            }

            ObjectID = newID;
        }

        SetReceptors();
    }

    public void SetPadre(string key_)
    {
        if(ConnectionServer.instance == null)
        {
            IsMine = true;
            return;
        }

        if(key_.Length < 4)
        {
            return;//if key length is less of 4, this is invalid!
        }

        if(key_ == ConnectionServer.instance.GetUserKey())
        {
            IsMine = true;
        }

        if(gameObject.name.ToLower().Contains("nine"))
        {
            Debug.Log("la bala con ID" + ObjectID + ", tiene la propiedad IsMine:" + isMine);
            Debug.Log("la bala con ID" + ObjectID + ", tiene fue creada por " + key_ + ", y este jugador es:" + ConnectionServer.instance.GetUserKey());
        }

        Invoke("setReady", 1f);

        //isReady = true; 
       // Debug.LogFormat("key del objeto {0}, key del usuario: {1}", key_, ConnectionServer.instance.GetUserKey());
    }

    void setReady()
    {
        isReady = true;
    }

    public void EnviarDatos(Object objeto)
    {
        if (ObjetosSend == null)
            ObjetosSend = new List<Object>();

        ObjetosSend.Add(objeto);
    }

    public void CallRPC(string rpcName, string args = "", TipoRPC tipoRPC = TipoRPC.all)
    {
        if (ConnectionServer.instance == null)
            return;

        JSONObject JSONObject__ = new JSONObject();
        JSONObject__.AddField("ObjectID", ObjectID);
        JSONObject__.AddField("rpcName", rpcName);
        string rpcType = tipoRPC == TipoRPC.all ? "All" : "Other";
        JSONObject__.AddField("Type", rpcType);

        if (args != "")
            JSONObject__.AddField("argumentos", EncryptPlugin.Base64Encode(args));

        ConnectionServer.instance.Emit("CallRPC", JSONObject__);
    }

    public void SetReceptors()
    {
        if (ConnectionServer.instance == null)
            return;

        ConnectionServer.instance.On("CallRPC", (e) =>
        {
            try
            {
                if (e.data["ObjectID"].str == ObjectID)
                {
                    if (e.data["arg"] != null)
                    {
                        if(this.gameObject)
                            this.gameObject.SendMessage(e.data["rpcName"].str, EncryptPlugin.Base64Decode(e.data["arg"].str));
                    }
                    else
                    {
                        if(this.gameObject)
                            this.gameObject.SendMessage(e.data["rpcName"].str);
                    }
                }
            }
            catch (System.Exception ee)
            {
                print("error. " + ee);
            }
        });
    }
}

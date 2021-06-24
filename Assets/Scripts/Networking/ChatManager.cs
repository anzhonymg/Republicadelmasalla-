using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChatManager : MonoBehaviour
{
    public TextMeshProUGUI MsjNew;
    public GameObject AlarmMsj;

    public GameObject PanelChat;
    public GameObject MsjPrefabOther, MsjPrefabMe;
    public Transform ParentMsj;

    public TMP_InputField inputMSG;

    public GameObject MicSignal;

    int newMsjAmount = 0;
    bool isChatOpen;

    public bool isChatMode{
        get{
            return isChatOpen;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        ConnectionServer.instance.On("ReceiveMSG", (e) =>
        {
            ReceiveMsg(e.data);
        });
    }

    public void ChangeStatusMic(bool b_)
    {
        MicSignal.SetActive(b_);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void managePanelChat()
    {
        isChatOpen = PanelChat.activeSelf;
        PanelChat.SetActive(!isChatOpen);

        isChatOpen = PanelChat.activeSelf;
        if(isChatOpen)
        {
            AlarmMsj.SetActive(false);
            MsjNew.text = newMsjAmount.ToString();
        }
    }

    public void ReceiveMsg(JSONObject data)
    {
        isChatOpen = PanelChat.activeSelf;
        newMsjAmount += 1;
        if (newMsjAmount > 99)
            newMsjAmount = 100;


        if (!isChatOpen)
        {
            AlarmMsj.SetActive(true);
            if(newMsjAmount < 100)
                MsjNew.text = newMsjAmount.ToString();
            else
                MsjNew.text = "99+";
        }
        else
        {
            newMsjAmount = 0;
        }

        bool msgIsMine = data["user"].str == ConnectionServer.instance.GetUserKey();
        GameObject objectToMake = msgIsMine ? MsjPrefabMe : MsjPrefabOther;
        GameObject msg_ = Instantiate(objectToMake, ParentMsj);

        string time_ = "";
        string date_ = data["date"].str;
        System.DateTime newDate = System.DateTime.Now;
        System.DateTime oldDate = System.Convert.ToDateTime(date_);

        System.TimeSpan tsNew = newDate.Subtract(oldDate);
        int seconds = tsNew.Seconds;
        if(seconds > 60)
        {
            time_ = tsNew.Minutes + " min";
        }
        else
        {
            time_ = seconds + " sec";
        }

        if (!msgIsMine)
        {
            msg_.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = data["userNick"].str;
            msg_.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = time_ + " ago";
            msg_.transform.GetChild(0).GetChild(2).GetComponent<TextMeshProUGUI>().text = data["msg"].str;
        }
        else
        {
            msg_.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = data["userNick"].str;
            msg_.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = time_ + " ago";
            msg_.transform.GetChild(0).GetChild(2).GetComponent<TextMeshProUGUI>().text = data["msg"].str;
        }
    }

    public void SendMsg()
    {
        string msg = inputMSG.text;
        if (msg.Length <= 0)
            return;

        inputMSG.text = string.Empty;

        string date_ = "";
        date_ = System.DateTime.Now.ToString();

        JSONObject data = new JSONObject();
        data.AddField("user", ConnectionServer.instance.GetUserKey());
        data.AddField("userNick", UserManager.instancia.DatosDeUsuario.nombre + " " + UserManager.instancia.DatosDeUsuario.apellido);
        data.AddField("msg", msg);
        data.AddField("date", date_);

        ConnectionServer.instance.Emit("SendMsg", data);
    }
}

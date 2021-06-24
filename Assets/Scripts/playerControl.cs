using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class playerControl : MonoBehaviour
{
    public float velocidadMovimiento;
    public float velocidadrotacion;
    public bool isTest;
    public Transform cuello, cabeza;
    public GameObject myMesh;
    public Animator Animador;

    public TextMeshPro nameText;

    NetworkView myview;
    Rigidbody mybody;

    Vector3 posmouse;

    bool dataWasSet;
    bool isFocus;
    bool isChatMode;

    ChatManager chatManager_;
    bool isRunMode;

    string myname;
    // Start is called before the first frame update
    void Start()
    {
        myview = GetComponent<NetworkView>();
        mybody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if(isTest || myview.isReady && myview.isMine)
        {
            if(chatManager_ == null)
            {
                chatManager_ = FindObjectOfType<ChatManager>();
            }

            isChatMode = chatManager_.isChatMode;
            movimiento();
            Rotation();

            if(Application.platform != RuntimePlatform.Android)
            {
                if(Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
                {
                    isRunMode=true;
                }

                if(Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
                {
                    isRunMode=false;
                }
            }
        }

        if(myview)
        {
            if(myview.isReady)
            {
                if(!dataWasSet)
                {
                    setData();
                    dataWasSet = true;

                    if(!myview.isMine)
                    {
                        Rigidbody body_ = GetComponent<Rigidbody>();
                        Destroy(body_);
                        myview.CallRPC("RequestName");
                    }else{
                        myname = UserManager.instancia.DatosDeUsuario.nombre + " " + UserManager.instancia.DatosDeUsuario.apellido;
                        nameText.text = myname;
                        myview.CallRPC("SetMyName", myname, NetworkView.TipoRPC.other);
                    }
                }
            }
        }
    }

    void RequestName()
    {
        if(myview.isReady && myview.isMine)
        {
            myview.CallRPC("SetMyName", myname);
        }
    }

    public void SetMyName(string name_)
    {
        if(!myview.isMine)
        {
            myname = name_;
            nameText.text = myname;
        }
    }

    void OnApplicationFocus(bool b_)
    {
       isFocus = b_;
    }

    void setData()
    {
        myMesh.SetActive(!myview.isMine);
        cabeza.gameObject.SetActive(myview.isMine);
    }

    void movimiento()
    {
        if(isChatMode)
            return; 

        bool isWalking = false;
        bool isWalkingSide = false;

        if (Input.GetKey(KeyCode.W))
        {
            float runMultiply = isRunMode ? 2.5f:1;
            transform.Translate(Vector3.forward * Time.deltaTime * velocidadMovimiento * runMultiply);
            Animador.SetBool("walkF",true);
            isWalking=true;
        }

        if (Input.GetKey(KeyCode.S))
        {
            float runMultiply = isRunMode ? 2.5f:1;
            transform.Translate(Vector3.back * Time.deltaTime * velocidadMovimiento * runMultiply);
            Animador.SetBool("walkB",true);
            isWalking=true;
        }

        if(!isWalking)
        {
            Animador.SetBool("walkF",false);
            Animador.SetBool("walkB",false);
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * Time.deltaTime * velocidadMovimiento);
            Animador.SetFloat("X-Vel",-1);
            isWalkingSide = true;
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * Time.deltaTime * velocidadMovimiento);
            Animador.SetFloat("X-Vel",1);
            isWalkingSide = true;
        }

        if(!isWalkingSide)
        {
            Animador.SetFloat("X-Vel",0);
        }
    }

    public void SetChatMode(bool b_)
    {
        isChatMode = b_;
    }

    void Rotation()
    {
        if(isChatMode || !isFocus)
            return; 

        if(posmouse == Vector3.zero)
        {
            posmouse = Input.mousePosition;
            return;
        }

        if(Input.GetMouseButtonDown(1))
        {
            posmouse = Input.mousePosition;
            return;
        }

        if(!Input.GetMouseButton(1))
            return;

        Vector3 newpos = Input.mousePosition - posmouse;

        float rotX = newpos.x * Time.deltaTime * velocidadrotacion;
        transform.Rotate(new Vector3(0, rotX, 0)) ;

        float rotY = newpos.y * Time.deltaTime * velocidadrotacion;
        cabeza.Rotate(new Vector3(rotY * -1, 0 , 0));

        if(cabeza.localEulerAngles.x > 45 && cabeza.localEulerAngles.x < 90)
        {
            Vector3 headEuler = Vector3.zero;
            headEuler.x = 45;
            cabeza.localEulerAngles = headEuler;
        }

        if (cabeza.localEulerAngles.x > 90 && cabeza.localEulerAngles.x < 315)
        {
            Vector3 headEuler = Vector3.zero;
            headEuler.x = 315;
            cabeza.localEulerAngles = headEuler;
        }

        posmouse = Input.mousePosition;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SelectorPlayer : MonoBehaviour
{
    public Transform Camara;
    public Transform pivotCamara;

    public GameObject myCanvas;
    public GameObject EffectSelect;
    public GameObject panelLoading;

    public Slider barraCarga;
    public TextMeshProUGUI textStatus;
    public int progress;

    public Button botonSelect;

    float clockStart;
    bool camaraIsInPos;
    int selectedPlayer = -1;

    Camera mycam;
    Transform oldTarget;

    public static int playerSelect = 0;
    bool isLoadingLevel;
    bool isLoadingScene;
    bool NeedRotate;
    // Start is called before the first frame update
    void Start()
    {
        mycam = Camara.GetComponent<Camera>();
    }

    public void SelectPlayer(int select_, Transform target)
    {
        Debug.Log("Player_" + select_);
        selectedPlayer = select_;
        playerSelect = select_;
        EffectSelect.transform.position = target.position;
        EffectSelect.SetActive(true);

        if(oldTarget)
        {
            Renderer renderT_ = oldTarget.GetComponent<Renderer>();
            if (renderT_)
            {
                renderT_.material.SetColor("_EmissionColor", Color.black);
            }

            Renderer[] rendersT_ = oldTarget.GetChild(0).GetComponentsInChildren<Renderer>();
            for (int i = 0; i < rendersT_.Length; i++)
            {
                rendersT_[i].material.SetColor("_EmissionColor", Color.black);
            }

            oldTarget.localEulerAngles = new Vector3(0,180,0);
        }

        Renderer render_ = target.GetComponent<Renderer>();
        if(render_)
        {
            render_.material.SetColor("_EmissionColor",Color.white);
        }

        Renderer[] renders_ = target.GetChild(0).GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renders_.Length; i++)
        {
            renders_[i].material.SetColor("_EmissionColor", Color.white);
        }

        oldTarget = target;
        NeedRotate=true;
        botonSelect.interactable = true;
    }

    public void ContinuarToLobby()
    {
        StartCoroutine(loadingRoom());
    }

    IEnumerator loadingRoom()
    {
        panelLoading.SetActive(true);

        Debug.Log("entrando a la sala");

        float progressFLoat = 30;
        barraCarga.value = progressFLoat;

        NetworkClases.RoomData roomData = new NetworkClases.RoomData();
        roomData.IsVisible = true;
        roomData.maxPlayer = 20;
        roomData.name = "Lobby";
        roomData.SyncScene = true;



        ConnectionServer.instance.JoinOrCreateRoomByType("Lobby", roomData);

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

            var sincScene = SceneManager.LoadSceneAsync("Lobby");
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
     
    }

    // Update is called once per frame
    void Update()
    {
        if(oldTarget)
        {
            if(NeedRotate)
            {
                Quaternion q_ = Quaternion.Euler(new Vector3(0,0,0));
                Quaternion qRot = Quaternion.Slerp(oldTarget.rotation, q_, Time.deltaTime * 3);
                oldTarget.rotation = qRot;

                if(q_ == oldTarget.rotation)
                {
                    NeedRotate=false;
                }
            }
        }
        
        if(clockStart>=2)
        {
            if(!camaraIsInPos)
            {
                Camara.transform.position = Vector3.Slerp(Camara.transform.position, pivotCamara.transform.position, Time.deltaTime);

                if(Vector3.Distance(Camara.transform.position,  pivotCamara.position) < 0.1f)
                {
                    Camara.transform.position = pivotCamara.position;
                    myCanvas.SetActive(true);
                    camaraIsInPos = true;
                }
            }

            if(Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                if(Physics.Raycast(mycam.ScreenPointToRay(Input.mousePosition), out hit, float.MaxValue))
                {
                    if(hit.collider.gameObject.tag == "avatar")
                    {
                        int id_ = hit.collider.transform.GetSiblingIndex();
                        SelectPlayer(id_, hit.collider.transform);
                    }
                }
            }

        }
        else
        {
            clockStart += Time.deltaTime;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkTransformView : MonoBehaviour
{
    Vector3 posicion;
    Vector3 rotacion;

    public bool SyncPos = true;
    public bool SyncRot = true;
    public bool IsPlayer = true;
    public bool useTime = false;

    public float Intervalo;

    public float distanceBtwUpdate = 0.1f;

    public bool isObject;

    NetworkView networkView_;
    ConnectionServer server_;


    int msjShow = 0;
    bool callBackIsSet = false;
    float timeInterval;

    // Start is called before the first frame update
    void Start()
    {
        networkView_ = GetComponent<NetworkView>();
        server_ = ConnectionServer.instance;

        if(!isObject)
            OnChangesFromServer();

        posicion = transform.position;
        rotacion = transform.eulerAngles;
    }

    private void Update()
    {
        if (networkView_ == null  || server_ == null)
            return;
        
        if(!networkView_.isReady && !IsPlayer)
        {
            return;
        }
        

       if(isObject)
        {
            UpdateTransformNew();
        }
        else
        {
            UpdateTransform();
        }
    }

    public void SetTransform(string data_)
    {
        if (networkView_.isMine)
            return;

        JSONObject data = new JSONObject(data_);

        string xPos = data["xPos"].str.Replace(',', '.');
        string yPos = data["yPos"].str.Replace(',', '.');
        string zPos = data["zPos"].str.Replace(',', '.');

        float x_ = transform.position.x;
        float y_ = transform.position.y;
        float z_ = transform.position.z;

        try
        {
             x_ = float.Parse(xPos, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
             y_ = float.Parse(yPos, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
             z_ = float.Parse(zPos, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
        }catch(System.FormatException)
        {
            Debug.LogFormat("formato invalido. ({0}), {1}, {2}", xPos, yPos, zPos);
        }

        

        posicion = new Vector3(x_, y_, z_);

        string xRot = data["xRot"].str.Replace(',', '.');
        string yRot = data["yRot"].str.Replace(',', '.');
        string zRot = data["zRot"].str.Replace(',', '.');

        float xR_ = float.Parse(xRot, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
        float yR_ = float.Parse(yRot, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
        float zR_ = float.Parse(zRot, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);

        rotacion = new Vector3(xR_, yR_, zR_);

        Debug.Log(gameObject.name + ": data=" + data_);
        Debug.Log(gameObject.name + ": pos=" + posicion.ToString());
        Debug.Log(gameObject.name + ": rot=" + rotacion.ToString());
    }


    void UpdateTransform()
    {
        if (networkView_.isMine)
        {
            if(useTime)
            {
                timeInterval += Time.deltaTime;
                if(timeInterval < Intervalo)
                {
                    return;
                }
                else
                {
                    timeInterval = 0;
                }
            }

            if (posicion != transform.position)
            {
                posicion = transform.position;
                JSONObject jsonData_ = new JSONObject();
                jsonData_.AddField("x", posicion.x.ToString("N3"));
                jsonData_.AddField("y", posicion.y.ToString("N3"));
                jsonData_.AddField("z", posicion.z.ToString("N3"));
                jsonData_.AddField("isPlayer", IsPlayer);
                jsonData_.AddField("objectID", networkView_.ObjectID);

                if (SyncPos)
                    server_.Emit("UpdatePos", jsonData_);
            }

            if (transform.eulerAngles != rotacion)
            {
                rotacion = transform.eulerAngles;
                JSONObject jsonData_ = new JSONObject();
                jsonData_.AddField("x", rotacion.x.ToString("N3"));
                jsonData_.AddField("y", rotacion.y.ToString("N3"));
                jsonData_.AddField("z", rotacion.z.ToString("N3"));
                jsonData_.AddField("isPlayer", IsPlayer);
                jsonData_.AddField("objectID", networkView_.ObjectID);

                if (SyncRot)
                    server_.Emit("UpdateRot", jsonData_);
            }
        }
        else
        {
            OnChangesFromServer();

            if (Vector3.Distance(posicion, transform.position) > distanceBtwUpdate)
            {
                if (SyncPos)
                    transform.position = Vector3.Slerp(transform.position, posicion, Time.deltaTime * 5);
            }

            if (Vector3.Distance(posicion, transform.position) > distanceBtwUpdate * 5)
            {
                if (SyncPos)
                    transform.position = posicion;
            }

            if (transform.eulerAngles != rotacion)
            {
                if (SyncRot)
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(rotacion), Time.deltaTime * 5);
            }
        }
    }

    void UpdateTransformNew()
    {
        if (networkView_.isMine)
        {
            if (useTime)
            {
                timeInterval += Time.deltaTime;
                if (timeInterval < Intervalo)
                {
                    return;
                }
                else
                {
                    timeInterval = 0;
                }
            }

            bool needSend = false;
            JSONObject jsonData_ = new JSONObject();

            if (Vector3.Distance(posicion, transform.position) > distanceBtwUpdate)
            {
                posicion = transform.position;
                needSend = true;
            }

            if (transform.eulerAngles != rotacion)
            {
                rotacion = transform.eulerAngles;
                needSend = true;
            }

            jsonData_.AddField("xPos", posicion.x.ToString("N3"));
            jsonData_.AddField("yPos", posicion.y.ToString("N3"));
            jsonData_.AddField("zPos", posicion.z.ToString("N3"));

            jsonData_.AddField("xRot", rotacion.x.ToString("N3"));
            jsonData_.AddField("yRot", rotacion.y.ToString("N3"));
            jsonData_.AddField("zRot", rotacion.z.ToString("N3"));

            if (needSend)
            {
                networkView_.CallRPC("SetTransform", jsonData_.ToString(), NetworkView.TipoRPC.other);
            }
        }
        else
        {
            if (Vector3.Distance(posicion, transform.position) > distanceBtwUpdate)
            {
                if (SyncPos)
                    transform.position = Vector3.Slerp(transform.position, posicion, Time.deltaTime * 5);
            }

            if (Vector3.Distance(posicion, transform.position) > distanceBtwUpdate * 4)
            {
                if (SyncPos)
                    transform.position = posicion;
            }

            if (transform.eulerAngles != rotacion)
            {
                if (SyncRot)
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(rotacion), Time.deltaTime * 5);
            }
        }
    }

    void OnChangesFromServer()
    {
        if (networkView_ == null || server_ == null)
            return;

        if(callBackIsSet)
        {
            return;
        }

        if (!networkView_.isMine)
        {
            server_.On("UpdatePos", (e) => {
                string objectID = e.data["objectID"].str;

                //Debug.Log(e.data.ToString());

                if(objectID == networkView_.ObjectID && !networkView_.isMine)
                {
                    string xPos = e.data["x"].str.Replace(',', '.');
                    string yPos = e.data["y"].str.Replace(',', '.');
                    string zPos = e.data["z"].str.Replace(',', '.');

                    //Debug.LogFormat("X:{0}, X:{1}, X:{2}", xPos, yPos, zPos);

                    float x_ = float.Parse(xPos, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture); 
                    float y_ = float.Parse(yPos, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
                    float z_ = float.Parse(zPos, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);

                    //Debug.LogFormat("Number(X:{0}, X:{1}, X:{2})", x_, y_, z_);

                    posicion = new Vector3(x_, y_, z_);
                }
            });

            server_.On("UpdateRot", (e) => {
                string objectID = e.data["objectID"].str;

                if (objectID == networkView_.ObjectID && !networkView_.isMine)
                {
                    string xRot = e.data["x"].str.Replace(',', '.');
                    string yRot = e.data["y"].str.Replace(',', '.');
                    string zRot = e.data["z"].str.Replace(',', '.');

                    float x_ = float.Parse(xRot, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
                    float y_ = float.Parse(yRot, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
                    float z_ = float.Parse(zRot, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);

                    rotacion = new Vector3(x_, y_, z_);
                }
            });

            callBackIsSet = true;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkAnimator : MonoBehaviour
{
    public Animator AnimatorToView;

    NetworkView networkView_;
    ConnectionServer server_;

    JSONObject paramenters;
    bool isConfigured;
    // Start is called before the first frame update
    void Start()
    {
        networkView_ = GetComponent<NetworkView>();
        server_ = ConnectionServer.instance;
        //OnChangesFromServer();
    }

    // Update is called once per frame
    void Update()
    {
        if(!isConfigured)
        {
            if(networkView_.isReady)
            {
                isConfigured=true;
                if(!networkView_.isMine)
                {
                    OnChangesFromServer();
                }
            }
        }

        if (networkView_.isMine && server_ != null)
        {
            AnimatorClipInfo[] clip__ = AnimatorToView.GetCurrentAnimatorClipInfo(0);
            if(clip__.Length == 0)
            {
                return;
            }

            JSONObject jsonData_ = new JSONObject();
            jsonData_.AddField("currentAnimation", clip__[0].clip.name);
            jsonData_.AddField("objectID", networkView_.ObjectID);

            Hashtable parametros = new Hashtable();
            JSONObject parametros_ = new JSONObject();

            AnimatorControllerParameter[] parameters_ = AnimatorToView.parameters;
            for (int i = 0; i < parameters_.Length; i++)
            {
                AnimatorControllerParameterType type__ = parameters_[i].type;
                switch(type__)
                {
                    case AnimatorControllerParameterType.Bool:
                        bool val_ = AnimatorToView.GetBool(parameters_[i].name);
                        parametros_.AddField(parameters_[i].name, val_);
                    break;

                    case AnimatorControllerParameterType.Float:
                        float valF_ = AnimatorToView.GetFloat(parameters_[i].name);
                        parametros_.AddField(parameters_[i].name, valF_.ToString());
                    break;

                    case AnimatorControllerParameterType.Int:
                        int valI_ = AnimatorToView.GetInteger(parameters_[i].name);
                        parametros_.AddField(parameters_[i].name, valI_.ToString());
                    break;
                }
            }

            jsonData_.AddField("parametros", parametros_);

            if (paramenters == null || paramenters.ToString() != jsonData_.ToString())
            {
                server_.Emit("UpdateAnimation", jsonData_);
            }
        }
    }

    void OnChangesFromServer()
    {
        if (!networkView_.isMine && server_ != null)
        {
            server_.On("UpdateAnimation", (e) => {
                string objectID = e.data["objectID"].str;
                if (objectID == networkView_.ObjectID)
                {
                        AnimatorControllerParameter[] parameters_ = AnimatorToView.parameters;
                        for (int i = 0; i < parameters_.Length; i++)
                        {
                            AnimatorControllerParameterType type__ = parameters_[i].type;
                            switch (type__)
                            {
                                case AnimatorControllerParameterType.Bool:
                                    if(e.data["parametros"][parameters_[i].name] != null)
                                        AnimatorToView.SetBool(parameters_[i].name, e.data["parametros"][parameters_[i].name].b);
                                    break;

                                case AnimatorControllerParameterType.Float:
                                    if (e.data["parametros"][parameters_[i].name] != null)
                                        AnimatorToView.SetFloat(parameters_[i].name, float.Parse(e.data["parametros"][parameters_[i].name].str, System.Globalization.NumberStyles.Any));
                                    break;

                                case AnimatorControllerParameterType.Int:
                                    if (e.data["parametros"][parameters_[i].name] != null)
                                        AnimatorToView.SetInteger(parameters_[i].name, int.Parse(e.data["parametros"][parameters_[i].name].str, System.Globalization.NumberStyles.Any));
                                    break;
                            }
                        }
                }
            });

            server_.On("SetTriggerAnimation", (e) =>
            {
                string objectID = e.data["objectID"].str;
                if (objectID == networkView_.ObjectID)
                {
                    AnimatorToView.SetTrigger(e.data["name"].str);
                }
                    
            });
        }
    }

    public void SetTrigger(string name_)
    {
        if(networkView_.isMine && server_ != null)
        {
            JSONObject jsonData_ = new JSONObject();
            jsonData_.AddField("objectID", networkView_.ObjectID);
            jsonData_.AddField("name", name_);
            server_.Emit("SetTriggerAnimation", jsonData_);
        }
    }
}

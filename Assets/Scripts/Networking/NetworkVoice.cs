using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class dataAudio
{
    /*
    public string audioText = "blabla";
    public byte[] bitsAudio;
    */
    // public int frecuencia;
    public int canal;
    //public float[] data;
    public byte[] bits;
}

[RequireComponent(typeof(AudioSource))]
public class NetworkVoice : MonoBehaviour
{
    public NetworkView myview;
    public AudioSource Salida;
    public GameObject iconoTalk;
    public bool TestMyAudio;
    public bool TestLocal;
    //A boolean that flags whether there's a connected microphone  
    private bool micConnected = false;
    private dataAudio myDataAudio, recibido;
    //The maximum and minimum available recording frequencies  
    private int minFreq;
    private int maxFreq;

    bool initialized;
    bool canRecord;
    AudioClip myRecord;

    float timerVoice = 0;
    float[] dataAudio;
    int diferencia;

    //new method
    int lastSample;
    int micPos;
    AudioClip c;
    int FREQUENCY = 48000;

    public string dataToSendTwo;
    byte[] testByte;

    string dataToSend;
    int DataLength, DataLengthTwo;
    float fps = 0;
    bool isSending;

    JSONObject dataVoice;

    ChatManager chatManager_;
    public bool micIsActive{
        get{
            return isActiveMic;
        }
    }

    bool isActiveMic;
    string dataRecibida;
    // Start is called before the first frame update
    void Start()
    {
        Salida = this.GetComponent<AudioSource>();

        if (myview.isMine)
            initialize();

        if(!Application.isEditor)
            TestLocal=false;
    }

    //initialize microphone
    public void initialize()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        if(!initialized)
        {
             Microphone.Init();
             Microphone.QueryAudioInput();
             initialized = true;
        }    
        #else
        initialized = true;
        // Check if there is at least one microphone connected
        if (Microphone.devices.Length <= 0)
        {
            //Throw a warning message at the console if there isn't  
            Debug.LogWarning("Microphone not connected!");
        }
        else //At least one microphone is present  
        {
            //Set 'micConnected' to true  
            micConnected = true;

            //Get the default microphone recording capabilities  
            Microphone.GetDeviceCaps(null, out minFreq, out maxFreq);
            myRecord = Microphone.Start(null, true, 100, minFreq);
            c = myRecord;
            canRecord = true;

            while (Microphone.GetPosition(null) < 0) { } // HACK from Riro

            //According to the documentation, if minFreq and maxFreq are zero, the microphone supports any frequency...  
            if (minFreq == 0 && maxFreq == 0)
            {
                //...meaning 44100 Hz can be used as the recording sampling rate  
                maxFreq = 44100;
            }
        }
        #endif
    }

    // Update is called once per frame
    void Update()
    {
        if (!initialized)
        {
            if (myview.isReady)
            {
                if (myview.isMine)
                {
                    initialize();
                }
                else
                {
                    Salida.loop = true;
                    canRecord = false;
                }
            }
        }

        if(Application.platform != RuntimePlatform.Android)
        {
            if(chatManager_ == null)
            {
                chatManager_ = FindObjectOfType<ChatManager>();
            }

            bool canWalk = true;
            if(chatManager_)
            {
                if(chatManager_.isChatMode)
                {
                    canWalk=false;
                }
            }
            if(canWalk && Application.isFocused)
            {
                if(Input.GetKeyDown(KeyCode.Space))
                {
                    isActiveMic=true;
                    if(chatManager_!=null)
                        chatManager_.ChangeStatusMic(true);
                }
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                isActiveMic = false;
                if (chatManager_ != null)
                    chatManager_.ChangeStatusMic(false);

                myview.CallRPC("Callarme", "" , NetworkView.TipoRPC.all);
            }

        }

        newRecordAudio();
    }

    public void Callarme()
    {
        Salida.volume = 0;
        Salida.Stop();

        if(iconoTalk)
        {
            if(iconoTalk.activeSelf)
            {
                iconoTalk.SetActive(false);
            }
        }
    }

    public void ChangeStatusVoice()
    {
        isActiveMic = !isActiveMic;
    }
    void newRecordAudio()
    {
        fps = 1f / Time.deltaTime;

        if (micConnected && canRecord && myview.isReady && myview.isMine && fps > 20)
        {
            if(isActiveMic)
            {
                timerVoice+=Time.deltaTime;
                if(timerVoice>0.5f)
                {
                    timerVoice = 0;
                    int pos = Microphone.GetPosition(null);
                    micPos = pos;

                    int diff = pos - lastSample;
                    diferencia = diff;
                    if (diff != 0)
                    {
                        if (c != null)
                        {
                            if (myDataAudio == null)
                                myDataAudio = new dataAudio();

                            float[] samples = new float[diff * c.channels];
                            c.GetData(samples, lastSample);
                            byte[] ba = ToByteArray(samples);
                            myDataAudio.canal = c.channels;
                            myDataAudio.bits = ba;

                            dataToSendTwo = System.Convert.ToBase64String(ba);
                            DataLengthTwo = dataToSendTwo.Length;
                            
                            if(dataVoice == null)
                            {
                                dataVoice = new JSONObject();
                                dataVoice.AddField("canal", c.channels);
                                dataVoice.AddField("samples", dataToSendTwo);
                                dataVoice.AddField("frecuencia", c.frequency);
                            }else{
                                dataVoice.Clear();
                                dataVoice.AddField("canal", c.channels);
                                dataVoice.AddField("samples", dataToSendTwo);
                                dataVoice.AddField("frecuencia", c.frequency);
                            }

                            if(TestLocal)
                            {
                                myview.CallRPC("getAudioNew", dataVoice.ToString(), NetworkView.TipoRPC.all);
                            }else{
                                myview.CallRPC("getAudioNew", dataVoice.ToString(), NetworkView.TipoRPC.other);
                            }
                            
                            

                            if (TestMyAudio)
                            {
                                Salida.clip = myRecord;
                                
                                if(!Salida.isPlaying)
                                    Salida.Play();
                            }
                            //ConnectionServer.instance.Emit("SendAudio", myDataAudio);
                        }
                    }
                    lastSample = pos;
                }
            }
            else
            {
                int pos = Microphone.GetPosition(null);
                lastSample = pos;
            }
        }
    }

    void RecordWebGL()
    {
        
    }

    public void getAudioNew(string data)
    {
        if(iconoTalk)
        {
            if(!iconoTalk.activeSelf)
            {
                iconoTalk.SetActive(true);
            }
        }

        Salida.volume = 1;

        //Debug.Log(data);
        JSONObject received = new JSONObject(data);
        string sampleText_ = received["samples"].str; //EncryptPlugin.Base64Decode(received["samples"].str);
        byte[] samples_ = System.Convert.FromBase64String(sampleText_); // System.Text.Encoding.ASCII.GetBytes(sampleText_);
        int canal_ = (int)received["canal"].f;
        float[] f = ToFloatArray(samples_);
        int frecuencia = (int)received["frecuencia"].f;

        if(recibido == null)
            recibido = new dataAudio();

        if(recibido.bits != samples_ && data != dataRecibida)
        {
            dataRecibida = data;

            recibido.bits = samples_;
            recibido.canal = canal_;

            Salida.clip = AudioClip.Create("RecibidoTemp", f.Length, canal_, frecuencia, false);
            Salida.clip.SetData(f, 0);
            
            Salida.loop = false;
            if (!Salida.isPlaying) Salida.Play();
        }
    }

    public void GetAudio(string data)
    {
        Debug.Log("recibiendo audio");
        myDataAudio = JsonUtility.FromJson<dataAudio>(data);
        Salida.clip = myRecord;
        Salida.Play();
    }


    // Used to convert the audio clip float array to bytes
    public byte[] ToByteArray(float[] floatArray)
    {
        var byteArray = new byte[floatArray.Length * 4];
        System.Buffer.BlockCopy(floatArray, 0, byteArray, 0, byteArray.Length);
        return byteArray;
    }

    public float[] ToFloatArray(byte[] byteArray)
    {
        var floatArray2 = new float[byteArray.Length / 4];
        System.Buffer.BlockCopy(byteArray, 0, floatArray2, 0, byteArray.Length);
        return floatArray2;
    }
}

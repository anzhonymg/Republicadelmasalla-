using UnityEngine;

public class ToastMessage : MonoBehaviour
{
    string toastString;
    string input;
    AndroidJavaObject currentActivity;
    AndroidJavaClass UnityPlayer;
    AndroidJavaObject context;

    public static ToastMessage instancia = null;

    private void Awake()
    {
        if (instancia != null)
        {
            Destroy(this.gameObject);
        }
    }

    void Start()
    {
        instancia = this;
        DontDestroyOnLoad(this.gameObject);

        if (Application.platform == RuntimePlatform.Android)
        {
            UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            currentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
        }
    }


    public void showToastOnUiThread(string toastString)
    {
        if(!Application.isEditor)
        {
            this.toastString = toastString;
            currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(showToast));
        }
    }

    void showToast()
    {
        Debug.Log(this + ": Running on UI thread");

        AndroidJavaClass Toast = new AndroidJavaClass("android.widget.Toast");
        AndroidJavaObject javaString = new AndroidJavaObject("java.lang.String", toastString);
        AndroidJavaObject toast = Toast.CallStatic<AndroidJavaObject>("makeText", context, javaString, Toast.GetStatic<int>("LENGTH_SHORT"));
        toast.Call("show");
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VfxManager : MonoBehaviour
{
    public Light Sun;
    public Gradient ColorRayo;

    Color normalColorSun, normalColorSky;

    bool isLigthing;
    Color colorCurrent;
    float timeCurrent;
    int timeWait = 4;
    // Start is called before the first frame update
    void Start()
    {
        normalColorSun = Sun.color;
        normalColorSky = RenderSettings.skybox.GetColor("_Tint");
    }

    private void OnDisable()
    {
        colorCurrent = ColorRayo.Evaluate(0);
        RenderSettings.skybox.SetColor("_Tint", colorCurrent);
    }
    private void OnDestroy()
    {
        colorCurrent = ColorRayo.Evaluate(0);
        RenderSettings.skybox.SetColor("_Tint", colorCurrent);
    }

    // Update is called once per frame
    void Update()
    {
        timeCurrent = Mathf.PingPong(Time.time, timeWait) / timeWait;
        colorCurrent = ColorRayo.Evaluate(timeCurrent);
        Sun.color = colorCurrent;
        RenderSettings.skybox.SetColor("_Tint", colorCurrent);

        if(timeCurrent >= 0.95f)
        {
            timeCurrent = 0;
            timeWait = Random.Range(1, 5);
        }
    }
}

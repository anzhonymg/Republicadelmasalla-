using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class gifViewer : MonoBehaviour
{
    public Sprite[] frames;
    public int fps;

    public bool isImage;

    SpriteRenderer myRender;
    Image myImage;

    int currentFps;
    float clock_;
    // Start is called before the first frame update
    void Start()
    {
        myRender = GetComponent<SpriteRenderer>();
        myImage = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        clock_ += Time.deltaTime;
        if(clock_ >= (float)(1f/(float)fps))
        {
            clock_ = 0;
            currentFps++;
            if (currentFps >= frames.Length)
                currentFps = 0;

            if(isImage)
            {
                myImage.sprite = frames[currentFps];
            }
            else
            {
                myRender.sprite = frames[currentFps];
            }
        }
    }
}

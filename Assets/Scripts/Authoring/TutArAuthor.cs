#if (UNITY_EDITOR) 

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.IO;

[System.Serializable]
public struct HandKeyframe
{
    public long frame;

    public float leftX;
    public float leftY;
    public float leftZ;

    public float rightX;
    public float rightY;
    public float rightZ;

    public HandKeyframe(long f, Vector3 hl, Vector3 hr)
    {
        frame = f;
        leftX = hl.x;
        leftY = hl.y;
        leftZ = hl.z;

        rightX = hr.x;
        rightY = hr.y;
        rightZ = hr.z;

    }
}

public class TutArAuthor : MonoBehaviour
{

    public GameObject videoPlayerContainer;
    public Slider slider;
    public Button playButton;
    public Button addButton;
    public Button finishButton;
    public Button perspectiveButton;

    public Camera bodyCam;
    public Toggle toggleLeftHand;
    public Toggle toggleRightHand;

    public VideoPlayer videoPlayer;
    public VideoClip clip;

    public GameObject leftHand;
    public GameObject rightHand;

    public List<HandKeyframe> userInputPosition;
    public List<HandKeyframe> userInputRotation;

    public GameObject tutARPlayer;

    private bool isPlaying = false;

    private Vector3 oLeftHand;
    private Vector3 oRightHand;

    private TutArPlayer mTutArPlayer;

    private Animation anim;
    private bool topview;

    // Initializes the GUI.
    void Start()
    {
        mTutArPlayer = tutARPlayer.GetComponent<TutArPlayer>();

        topview = true;

        anim = bodyCam.GetComponent<Animation>();

        userInputPosition = new List<HandKeyframe>();
        userInputRotation = new List<HandKeyframe>();

        oLeftHand = leftHand.transform.position;
        oRightHand = rightHand.transform.position;

        videoPlayer = (VideoPlayer)videoPlayerContainer.GetComponent(typeof(VideoPlayer));

        // Initliazize UI elements.
        clip = videoPlayer.clip;
        slider.maxValue = clip.frameCount;
        playButton.onClick.AddListener(PausePlay);
        addButton.onClick.AddListener(AddKeyframe);
        finishButton.onClick.AddListener(FinishReconstruction);
        perspectiveButton.onClick.AddListener(changePerspective);
        toggleRightHand.onValueChanged.AddListener(delegate
        {
            rightHandToggleChanged(toggleRightHand);
        });
        toggleLeftHand.onValueChanged.AddListener(delegate
        {
            leftHandToggleChanged(toggleLeftHand);
        });

        slider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });

        videoPlayer.Pause();
    }
    void leftHandToggleChanged(Toggle change)
    {
        leftHand.SetActive(change.isOn);
    }
    void rightHandToggleChanged(Toggle change)
    {
        rightHand.SetActive(change.isOn);
    }

    //Changes camera positio
    public void changePerspective()
    {
        if (topview)
        {
            anim.Play("TopToSide");
            topview = false;
        }
        else
        {
            anim.Play("SideToTop");
            topview = true;
        }

    }
    // Saves the added keyframes into a textfile.
    public void FinishReconstruction()
    {
        string pathPos = "Assets/Resources/" + mTutArPlayer.loadFrom + "pos.txt";
        StreamWriter writerPos = new StreamWriter(pathPos, false);
        foreach (var keyframe in userInputPosition)
        {
            writerPos.WriteLine(keyframe.frame / videoPlayer.frameRate + "," + keyframe.leftX + "," + keyframe.leftY + "," + keyframe.leftZ + "," + keyframe.rightX + "," + keyframe.rightY + "," + keyframe.rightZ);
        }
        writerPos.Close();

        string pathRot = "Assets/Resources/" + mTutArPlayer.loadFrom + "rot.txt";
        StreamWriter writerRot = new StreamWriter(pathRot, false);
        foreach (var keyframe in userInputRotation)
        {
            writerRot.WriteLine(keyframe.frame / videoPlayer.frameRate + "," + keyframe.leftX + "," + keyframe.leftY + "," + keyframe.leftZ + "," + keyframe.rightX + "," + keyframe.rightY + "," + keyframe.rightZ);
        }
        writerRot.Close();

    }
    // Slider controls.
    public void ValueChangeCheck()
    {
        videoPlayer.Pause();
        videoPlayer.frame = (int)slider.value;
        if (isPlaying)
        {
            videoPlayer.Play();
        }
    }
    // Adds a keyframe 
    void AddKeyframe()
    {
        HandKeyframe newDataRot;
        HandKeyframe newDataPos;

        if (!rightHand.activeSelf)
        {
            newDataPos = new HandKeyframe(videoPlayer.frame, leftHand.transform.position, new Vector3(0, 0, 0));
            newDataRot = new HandKeyframe(videoPlayer.frame, leftHand.transform.rotation.eulerAngles, new Vector3(0, 0, 0));

        }
        else if (!leftHand.activeSelf)
        {
            newDataPos = new HandKeyframe(videoPlayer.frame, new Vector3(0, 0, 0), rightHand.transform.position);
            newDataRot = new HandKeyframe(videoPlayer.frame, new Vector3(0, 0, 0), rightHand.transform.rotation.eulerAngles);

        }
        else
        {
            newDataPos = new HandKeyframe(videoPlayer.frame, leftHand.transform.position, rightHand.transform.position);
            newDataRot = new HandKeyframe(videoPlayer.frame, leftHand.transform.rotation.eulerAngles, rightHand.transform.rotation.eulerAngles);

        }
        userInputPosition.Add(newDataPos);
        userInputRotation.Add(newDataRot);
    }
    void PausePlay()
    {
        if (isPlaying)
        {
            videoPlayer.Pause();
            isPlaying = false;
        }
        else
        {
            videoPlayer.Play();
            isPlaying = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlaying)
            slider.value = videoPlayer.frame;
    }
}
#endif
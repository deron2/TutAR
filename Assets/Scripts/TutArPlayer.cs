using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using Kalman;
using HoloToolkit.Examples.InteractiveElements;
using System.IO;

public class TutArPlayer : MonoBehaviour
{
    public Vector3 offsetOfLeftHandPosition;
    public Vector3 offsetOfRightHandPosition;

    [UnityEngine.Range(0, 10)]
    public float openposeScale;
    [UnityEngine.Range(0, 10)]
    public float hand3dScale;

    public bool isPlaying = false;
    public bool useAuthorInput = false;
    public bool useAutomaticScaling = false;

    public string loadFrom;
    public float distancePalmToIndexInMeter = 0.1f;
    [UnityEngine.Range(0, 100)]
    public float translationsSmoothness = 10;
    public GameObject videoPlayer;
    public VideoClip videoClip;
    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject coordinateSystem;
    //public GameObject videoSlider;

    [SerializeField] private bool useKalmanFilter = false;
    //private SliderGestureControl mVideoSlider;
    private TutArXmlParser xmlParser;
    private IKalmanWrapper[] kalman;
    private int lastFrame;
    private VideoPlayer mvideoPlayer;
    private Vector3[] jointPositions;
    private Vector3[] oldJointPositions;
    private AnimationCurve[] animationCurve;
    private int numberOfJoints = 21;
    private int numberOfHands = 2;
    private GameObject[] bones;
    private GameObject[] hands;

    private float scale;
    private GameObject appBar;

    public Vector3[] GetJointPositions()
    {
        return jointPositions;
    }
    // Use this for initialization
    public void PauseTutAR()
    {
        mvideoPlayer.Pause();
        if (appBar != null)
        {
            appBar.SetActive(true);
        }

    }
    public void ResetTutAR()
    {
        mvideoPlayer.frame = 0;
        mvideoPlayer.Pause();
        leftHand.SetActive(false);
        rightHand.SetActive(false);
        if (coordinateSystem != null)
        {
            coordinateSystem.SetActive(true);
        }
    }
    public void SkipToFrame()
    {
        // mvideoPlayer.frame = (int)mVideoSlider.SliderValue;
    }
    public void StartTutAR()
    {
        videoPlayer.SetActive(true);
        isPlaying = true;
        mvideoPlayer.Play();
        rightHand.SetActive(true);
        leftHand.SetActive(true);
        mvideoPlayer.Play();
        if (coordinateSystem != null)
        {
            coordinateSystem.SetActive(false);
        }
        if(appBar != null)
        {
            appBar.SetActive(false);
        }
    }
    public void StopTutAR()
    {
        isPlaying = false;
        mvideoPlayer.Stop();
        videoPlayer.SetActive(false);
        leftHand.SetActive(false);
        rightHand.SetActive(false);

        if (coordinateSystem != null)
        {
            coordinateSystem.SetActive(true);
        }
        if (appBar != null)
        {
            appBar.SetActive(true);
        }
    }
    private void OnDestroy()
    {

    }
    void Awake()
    {
        InitArrays();
        if (useAuthorInput)
        {
            ReadAuthorInput();
        }
        videoPlayer.transform.parent = Camera.main.transform;
        xmlParser = new TutArXmlParser(loadFrom);

        if (videoPlayer != null)
        {
            mvideoPlayer = videoPlayer.GetComponent(typeof(VideoPlayer)) as VideoPlayer;
            lastFrame = (int)mvideoPlayer.frame;
            if (videoClip != null)
            {
                mvideoPlayer.clip = videoClip;

            }
            else
            {
                Debug.Log("No video clip defined.");
            }
        }

        //if (videoSlider != null)
        //{
        //    mVideoSlider = videoSlider.GetComponent(typeof(SliderGestureControl)) as SliderGestureControl;
        //    mVideoSlider.SetSpan(0f, (float)mvideoPlayer.clip.frameCount);
        //}

        kalman = new MatrixKalmanWrapper[numberOfJoints * numberOfHands];
        for (int i = 0; i < numberOfJoints * numberOfHands; i++)
        {
            kalman[i] = new MatrixKalmanWrapper();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (appBar ==  null)
        {
        appBar = GameObject.Find("AppBar(Clone)");
        }

        if (isPlaying && videoPlayer != null && mvideoPlayer.frame != lastFrame)
        {
            //mvideoPlayer.Play();
            LoadJointPositions();
            lastFrame = (int)mvideoPlayer.frame;
            //RenderBones();
            oldJointPositions = jointPositions;
        }
    }

    //Loads the joint positions from TutArXmlParser and scales them. 
    private void LoadJointPositions()
    {
        jointPositions = xmlParser.GetJointPositions((int)mvideoPlayer.frame);
        CalculateScale();

        for (int i = 0; i < jointPositions.Length; i++)
        {
            if (useAutomaticScaling)
            {
                jointPositions[i] *= scale;
            }
            else
            {
                jointPositions[i].x /= openposeScale;
                jointPositions[i].y /= openposeScale;
                jointPositions[i].z /= hand3dScale;
            }

            if (useAuthorInput)
            {
                float time = mvideoPlayer.frame / mvideoPlayer.frameRate;
                if (i < 21)
                {
                    Vector3 authorPosition;
                    authorPosition.x = animationCurve[3].Evaluate(time);
                    authorPosition.y = animationCurve[4].Evaluate(time);
                    authorPosition.z = animationCurve[5].Evaluate(time);
                    //jointPositions[i] += authorPosition;
                    rightHand.transform.localPosition = authorPosition;
                    rightHand.transform.rotation = Quaternion.Euler(animationCurve[9].Evaluate(time), animationCurve[10].Evaluate(time), animationCurve[11].Evaluate(time));
                }
                else if (i > 20)
                {
                    Vector3 authorPosition;
                    authorPosition.x = animationCurve[0].Evaluate(time);
                    authorPosition.y = animationCurve[1].Evaluate(time);
                    authorPosition.z = animationCurve[2].Evaluate(time);
                    //jointPositions[i] += authorPosition;
                    leftHand.transform.localPosition = authorPosition;
                    leftHand.transform.rotation = Quaternion.Euler(animationCurve[6].Evaluate(time), animationCurve[7].Evaluate(time), animationCurve[8].Evaluate(time));
                }
            }

            if (oldJointPositions != null)
            {
                jointPositions[i] = Vector3.Lerp(oldJointPositions[i], jointPositions[i], Time.deltaTime * translationsSmoothness);
            }

            if (useKalmanFilter)
            {
                jointPositions[i] = kalman[i].Update(jointPositions[i]);
            }

            if (i < 21)
            {
                jointPositions[i] += offsetOfRightHandPosition;
                //rightHand.transform.position += offsetOfRightHandPosition;
            }
            else
            {
                jointPositions[i] += offsetOfLeftHandPosition;
                //leftHand.transform.position += offsetOfRightHandPosition;

            }
        }
    }

    private void ReadAuthorInput()
    {
        try
        {
            TextAsset authorInputPos = (TextAsset)Resources.Load(loadFrom + "pos", typeof(TextAsset));
            string[] linesPos = authorInputPos.text.Split(System.Environment.NewLine[0]);

            foreach (string csvLine in linesPos)
            {
                string[] line = csvLine.Split(',');
                Debug.Log(line);
                if (line.Length == 7)
                {
                    animationCurve[0].AddKey(new Keyframe(float.Parse(line[0]), float.Parse(line[1])));
                    animationCurve[1].AddKey(new Keyframe(float.Parse(line[0]), float.Parse(line[2])));
                    animationCurve[2].AddKey(new Keyframe(float.Parse(line[0]), float.Parse(line[3])));

                    animationCurve[3].AddKey(new Keyframe(float.Parse(line[0]), float.Parse(line[4])));
                    animationCurve[4].AddKey(new Keyframe(float.Parse(line[0]), float.Parse(line[5])));
                    animationCurve[5].AddKey(new Keyframe(float.Parse(line[0]), float.Parse(line[6])));
                }
            }

            TextAsset authorInputRot = (TextAsset)Resources.Load(loadFrom + "rot", typeof(TextAsset));
            string[] linesRot = authorInputRot.text.Split(System.Environment.NewLine[0]);

            foreach (string csvLine in linesRot)
            {
                string[] line = csvLine.Split(',');
                if (line.Length == 7)
                {
                    animationCurve[6].AddKey(new Keyframe(float.Parse(line[0]), float.Parse(line[1])));
                    animationCurve[7].AddKey(new Keyframe(float.Parse(line[0]), float.Parse(line[2])));
                    animationCurve[8].AddKey(new Keyframe(float.Parse(line[0]), float.Parse(line[3])));

                    animationCurve[9].AddKey(new Keyframe(float.Parse(line[0]), float.Parse(line[4])));
                    animationCurve[10].AddKey(new Keyframe(float.Parse(line[0]), float.Parse(line[5])));
                    animationCurve[11].AddKey(new Keyframe(float.Parse(line[0]), float.Parse(line[6])));
                }
            }
        }
        catch (System.Exception)
        {
            Debug.Log("Error while parsing author input");
        }
    }
    private void CalculateScale()
    {
        float distancePalmIndex = Vector3.Distance(jointPositions[(int)OpenPoseHand.HandJoints.palm], jointPositions[(int)OpenPoseHand.HandJoints.palm2]);
        scale = distancePalmIndex * distancePalmToIndexInMeter;
    }

    private void InitArrays()
    {
        animationCurve = new AnimationCurve[12];

        for (int i = 0; i < 12; i++)
        {
            animationCurve[i] = new AnimationCurve();
        }
    }
}

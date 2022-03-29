using UnityEngine;

public class TutArHand : MonoBehaviour
{
    public bool isLeftHand;
    public bool isRightHand;

    public GameObject HandRoot;
    public GameObject tutARPlayer;

    public GameObject indexTip;
    public GameObject middleTip;
    public GameObject ringTip;
    public GameObject thumbTip;
    public GameObject pinkyTip;

    public GameObject container;
    private Vector3[] jointPositions;
    private TutArPlayer mTutArPlayer;
    private Vector3 palm;
    private Vector3 indexmeta;
    private Vector3 pinkymeta;

    private Quaternion oldRotation;
    private int offset;

    void Start()
    {
        // Offset to address left or right hand


        mTutArPlayer = tutARPlayer.GetComponent<TutArPlayer>();

    }
    // Update is called once per frame.
    void Update()
    {
        if (isLeftHand)
        {
            offset = 20;
        }
        else
        {
            offset = 0;
        }

        if (mTutArPlayer.isPlaying)
        {
            jointPositions = mTutArPlayer.GetJointPositions();
            HandRoot.SetActive(true);

            if (jointPositions != null)
            {
                // Moves the target for the hand for IK
                indexTip.transform.localPosition = jointPositions[(int)OpenPoseHand.HandJoints.index3 + offset];
                middleTip.transform.localPosition = jointPositions[(int)OpenPoseHand.HandJoints.middle3 + offset];
                ringTip.transform.localPosition = jointPositions[(int)OpenPoseHand.HandJoints.ring3 + offset];
                pinkyTip.transform.localPosition = jointPositions[(int)OpenPoseHand.HandJoints.pinky2 + offset];
                thumbTip.transform.localPosition = jointPositions[(int)OpenPoseHand.HandJoints.thumb3 + offset];
                if (jointPositions.Length != 0 && HandRoot.activeSelf)
                {
                    indexmeta = jointPositions[(int)OpenPoseHand.HandJoints.palm2 + offset];
                    palm = jointPositions[(int)OpenPoseHand.HandJoints.palm + offset];
                    pinkymeta = jointPositions[(int)OpenPoseHand.HandJoints.palm5 + offset];

                    Vector3 cross;
                    Quaternion rightToForward;
                    Quaternion position = Quaternion.Euler(0, 0, 0);

                    // Rotates hand according to the joint positions.
                    if (isLeftHand)
                    {
                        rightToForward = Quaternion.Euler(90f, 0, 0f);
                        position = Quaternion.Euler(0f, 180f, 0f);
                        cross = -Vector3.Cross(indexmeta - palm, pinkymeta - palm);
                    }
                    else
                    {
                        rightToForward = Quaternion.Euler(0f, -90f, 0f);
                        cross = Vector3.Cross(indexmeta - palm, pinkymeta - palm);
                    }
                    HandRoot.transform.localPosition = palm;


                    if (cross != Vector3.zero)
                    {
                        Quaternion rotation = Quaternion.LookRotation(indexmeta - palm, cross) * rightToForward * position;
                        HandRoot.transform.localRotation = Quaternion.Lerp(oldRotation, rotation, Time.deltaTime * 100); ;
                        oldRotation = rotation;
                    }
                }
            }
        }

    }
}

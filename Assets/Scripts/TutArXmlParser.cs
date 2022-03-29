using Kalman;
//using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Xml;
using UnityEngine;

public class TutArXmlParser : ITutArParser
{

    // folder name, where to load the xml files from Assets/
    public string loadFrom;

    private int numOfJoints = 21;
    private int numOfHands = 2;

    private float[] maxMinLeft = new float[] { 0, 0, 0, 0 };
    private float[] maxMinRight = new float[] { 0, 0, 0, 0 };
    private float[] maxMinLeftHand3D = new float[] { 0, 0 };
    private float[] maxMinRightHand3D = new float[] { 0, 0 };

    private IKalmanWrapper[] kalman;

    private System.Object[] xmlFiles;

    private Dictionary<int, string[]> OpenPoseHandLeftXMLFiles;
    private Dictionary<int, string[]> OpenPoseHandRightXMLFiles;
    private Dictionary<int, string[]> Hand3DHandRightXMLFile;
    private Dictionary<int, string[]> Hand3DHandLeftXMLFile;

    private Dictionary<int, XmlDocument> documents;

    private Vector3[] jointPositions;

    public TutArXmlParser(string loadFrom)
    {
        this.loadFrom = loadFrom;
        Load();
        InitArrays();
        NormalizeJointPoisition();

    }

    public Vector3[] GetJointPositions(int frame)
    {
        ParseData(frame);
        return jointPositions;
    }

    private void Load()
    {
        OpenPoseHandLeftXMLFiles = new Dictionary<int, string[]>();
        OpenPoseHandRightXMLFiles = new Dictionary<int, string[]>();
        Hand3DHandRightXMLFile = new Dictionary<int, string[]>();
        Hand3DHandLeftXMLFile = new Dictionary<int, string[]>();

        documents = new Dictionary<int, XmlDocument>();
        // Load all XML files from "Resources/foldername" ... OpenPose and Hand3D creates a new file for each frame when exporting it as xml. 
        xmlFiles = Resources.LoadAll(loadFrom, typeof(TextAsset));

        int indexl = 0;
        int indexr = 0;
        int indexdr = 0;
        int indexdl = 0;

        for (int i = 0; i < xmlFiles.Length; i++)
        {
            documents.Add(i, new XmlDocument());
            documents[i].LoadXml(((TextAsset)xmlFiles[i]).text);

            //OpenPose
            XmlNodeList left = documents[i].GetElementsByTagName("hand_left_0");
            XmlNodeList right = documents[i].GetElementsByTagName("hand_right_0");
            //Hand3D
            XmlNodeList depthright = documents[i].GetElementsByTagName("hand_right");
            XmlNodeList depthleft = documents[i].GetElementsByTagName("hand_left");

            if (right.Count > 0)
            {
                var strArray = right[0].LastChild.InnerText.Split(new string[] { "\r\n", "\n", " " },
                        StringSplitOptions.RemoveEmptyEntries);
                OpenPoseHandRightXMLFiles.Add(indexr, strArray);
                indexr++;
            }
            if (left.Count > 0)
            {
                var strArray = left[0].LastChild.InnerText.Split(new string[] { "\r\n", "\n", " " },
                        StringSplitOptions.RemoveEmptyEntries);
                OpenPoseHandLeftXMLFiles.Add(indexl, strArray);
                indexl++;
            }
            if (depthright.Count > 0)
            {
                var strArray = depthright[0].LastChild.InnerText.Split(new string[] { "\r\n", "\n", " " },
                    StringSplitOptions.RemoveEmptyEntries);
                Hand3DHandRightXMLFile.Add(indexdr, strArray);
                indexdr++;
            }
            if (depthleft.Count > 0)
            {
                var strArray = depthleft[0].LastChild.InnerText.Split(new string[] { "\r\n", "\n", " " },
                    StringSplitOptions.RemoveEmptyEntries);
                Hand3DHandLeftXMLFile.Add(indexdl, strArray);
                indexdl++;
            }
        }
    }

    private void InitArrays()
    {
        kalman = new MatrixKalmanWrapper[numOfJoints * numOfHands];
        jointPositions = new Vector3[numOfJoints * numOfHands];
        for (int i = 0; i < numOfJoints * numOfHands; i++)
        {
            kalman[i] = new MatrixKalmanWrapper();
        }
    }

    public void ParseData(int frame)
    {
        Vector3 newPosition;
        int index = 0;
        for (int i = 0; i < (numOfJoints) * numOfHands; i++)
        {

            if (index == 20)
            {
                index = 0;
            }
            try
            {
                //right hand
                if (i < 21)
                {
                    newPosition.x = float.Parse(OpenPoseHandRightXMLFiles[frame][index * 3], System.Globalization.NumberStyles.Float);
                    newPosition.y = float.Parse(OpenPoseHandRightXMLFiles[frame][index * 3 + 1], System.Globalization.NumberStyles.Float) * -1;
                    newPosition.z = float.Parse(Hand3DHandRightXMLFile[frame][index * 3 + 2], System.Globalization.NumberStyles.Float);

                    jointPositions[i].x = (newPosition.x - maxMinRight[0]) / (maxMinRight[2] - maxMinRight[0]);
                    jointPositions[i].y = (newPosition.y - maxMinRight[1]) / (maxMinRight[3] - maxMinRight[1]);
                    jointPositions[i].z = (newPosition.z - maxMinRightHand3D[0]) / (maxMinRightHand3D[1] - maxMinRightHand3D[0]);
                }
                //left hand
                else
                {
                    newPosition.x = float.Parse(OpenPoseHandLeftXMLFiles[frame][index * 3], System.Globalization.NumberStyles.Float);
                    newPosition.y = float.Parse(OpenPoseHandLeftXMLFiles[frame][index * 3 + 1], System.Globalization.NumberStyles.Float) * -1;
                    newPosition.z = float.Parse(Hand3DHandLeftXMLFile[frame][index * 3 + 2], System.Globalization.NumberStyles.Float);

                    jointPositions[i].x = (newPosition.x - maxMinLeft[0]) / (maxMinLeft[2] - maxMinLeft[0]);
                    jointPositions[i].y = (newPosition.y - maxMinLeft[1]) / (maxMinLeft[3] - maxMinLeft[1]);
                    jointPositions[i].z = (newPosition.z - maxMinLeftHand3D[0]) / (maxMinLeftHand3D[1] - maxMinLeftHand3D[0]);
                }
            }
            catch (KeyNotFoundException)
            {
                Debug.Log("XML file missing");
            }
            index++;
        }
    }

    private void NormalizeJointPoisition()
    {
        int frame = 0;
        maxMinLeft[0] = float.Parse(OpenPoseHandLeftXMLFiles[frame][0]);
        maxMinLeft[1] = float.Parse(OpenPoseHandLeftXMLFiles[frame][1]) * -1;
        maxMinLeft[2] = float.Parse(OpenPoseHandLeftXMLFiles[frame][0]);
        maxMinLeft[3] = float.Parse(OpenPoseHandLeftXMLFiles[frame][1]) * -1;

        maxMinRight[0] = float.Parse(OpenPoseHandRightXMLFiles[frame][0]);
        maxMinRight[1] = float.Parse(OpenPoseHandRightXMLFiles[frame][1]) * -1;
        maxMinRight[2] = float.Parse(OpenPoseHandRightXMLFiles[frame][0]);
        maxMinRight[3] = float.Parse(OpenPoseHandRightXMLFiles[frame][1]) * -1;

        for (int f = 0; f < OpenPoseHandLeftXMLFiles.Count; f++)
        {

            for (int index = 0; index < (numOfJoints); index++)
            {
                float xRight = float.Parse(OpenPoseHandRightXMLFiles[f][index * 3]);
                float yRight = float.Parse(OpenPoseHandRightXMLFiles[f][index * 3 + 1]) * -1;
                float xLeft = float.Parse(OpenPoseHandLeftXMLFiles[f][index * 3]);
                float yLeft = float.Parse(OpenPoseHandLeftXMLFiles[f][index * 3 + 1]) * -1;
                if (xRight < maxMinRight[0] && xRight != 0)
                {
                    maxMinRight[0] = xRight;
                }
                if (yRight < maxMinRight[1] && yRight != 0)
                {
                    maxMinRight[1] = yRight;
                }
                if (xRight > maxMinRight[2] && xRight != 0)
                {
                    maxMinRight[2] = xRight;
                }
                if (yRight > maxMinRight[3] && yRight != 0)
                {
                    maxMinRight[3] = yRight;
                }
                if (xLeft < maxMinLeft[0] && xLeft != 0)
                {
                    maxMinLeft[0] = xLeft;
                }
                if (yLeft < maxMinLeft[1] && yLeft != 0)
                {
                    maxMinLeft[1] = yLeft;
                }
                if (xLeft > maxMinLeft[2] && xLeft != 0)
                {
                    maxMinLeft[2] = xLeft;
                }
                if (yLeft > maxMinLeft[3] && yLeft != 0)
                {
                    maxMinLeft[3] = yLeft;
                }
            }

        }
        maxMinLeftHand3D[0] = float.Parse(Hand3DHandLeftXMLFile[0][2]);
        maxMinLeftHand3D[1] = float.Parse(Hand3DHandLeftXMLFile[0][2]);


        maxMinRightHand3D[0] = float.Parse(Hand3DHandRightXMLFile[0][2]);
        maxMinRightHand3D[1] = float.Parse(Hand3DHandRightXMLFile[0][2]);


        for (int f = 0; f < Hand3DHandRightXMLFile.Count; f++)
        {

            for (int index = 0; index < (numOfJoints); index++)
            {
                float zLeft = float.Parse(Hand3DHandLeftXMLFile[f][index * 3 + 2]);
                float zRight = float.Parse(Hand3DHandRightXMLFile[f][index * 3 + 2]);

                if (zRight < maxMinRightHand3D[0] && zRight != 0)
                {
                    maxMinRightHand3D[0] = zRight;
                }
                if (zRight > maxMinRightHand3D[1] && zRight != 0)
                {
                    maxMinRightHand3D[1] = zRight;
                }
                if (zLeft < maxMinLeftHand3D[0] && zLeft != 0)
                {
                    maxMinLeftHand3D[0] = zLeft;
                }
                if (zLeft > maxMinLeftHand3D[1] && zLeft != 0)
                {
                    maxMinLeftHand3D[1] = zLeft;
                }

            }
        }
    }
}

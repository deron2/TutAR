using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITutArParser
{
    void ParseData(int frame);
    Vector3[] GetJointPositions(int frame);

}

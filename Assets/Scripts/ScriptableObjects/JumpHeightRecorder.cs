using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/JumpHeightRecorder", order = 1)]
public class JumpHeightRecorder : ScriptableObject
{
    public List<float> yPositions = new List<float>();

    public void AddPos(float pos)
    {
        yPositions.Add(pos);
    }
}

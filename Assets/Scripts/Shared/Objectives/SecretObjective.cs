using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SecretObjective", menuName = "Scriptable Objects/SecretObjective")]
public class SecretObjective : ScriptableObject
{
    public string RoleName;
    [TextArea] public string Description;

    [Header("Objectives")]
    public bool Greedy = false; // get's twice the VP per supply

    public List<TrackType> PositiveTracks;
    public List<LevelVP> PositiveTable;

    public List<TrackType> NegativeTracks;
    public List<LevelVP> NegativeTable;
}

[System.Serializable]
public class LevelVP
{
    public int Level;
    public int Points;
}
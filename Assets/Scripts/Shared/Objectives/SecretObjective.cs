using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SecretObjective", menuName = "Scriptable Objects/SecretObjective")]
public class SecretObjective : ScriptableObject
{
    public string RoleName;
    [TextArea] public string Description;

    [Header("Objectives")]
    public bool Greedy = false; // get's twice the VP per supply

    public TrackType PositiveTrack;
    public List<LevelVP> PositiveTable;

    public TrackType NegativeTrack;
    public List<LevelVP> NegativeTable;
}

[System.Serializable]
public class LevelVP
{
    public int Level;
    public int Points;
}
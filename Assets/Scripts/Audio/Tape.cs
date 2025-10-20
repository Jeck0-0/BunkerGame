using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Tape", fileName = "NewVHSTape")]
public class Tape : ScriptableObject
{
    [Header("Identity")]
    public string TapeName;
    public string Artist;
    public Sprite CoverArt;

    [Header("Audio")]
    public AudioClip[] Tracks;
}

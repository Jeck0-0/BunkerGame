using UnityEngine;

namespace Client
{
    public enum EmergencyType { Crisis, Dilemma }
    
    public abstract class Emergency : ScriptableObject
    {
        public abstract EmergencyType Type { get; }
        
        public string Title;
        [TextArea(3, 10)] public string Description;

        [Tooltip("How much time players have to resolve the crisis(in seconds)")]
        public float TimeToResolve = 120f;

        [Tooltip("If true players commits are hidden")]
        public bool HiddenContributions = true;

        public bool Repeatable = false;
    }
}
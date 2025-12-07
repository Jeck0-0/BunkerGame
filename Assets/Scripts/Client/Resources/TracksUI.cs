using Client;
using System.Collections.Generic;
using UnityEngine;

public class TracksUI : Singleton<TracksUI>
{
    [SerializeField] List<TrackUI> trackReferences;

    public void SetObjective(SecretObjective objective)
    {
        foreach (var track in trackReferences)
        {
            int obj = 
                objective.NegativeTracks.Contains(track.type) ? 0 : 
                objective.NeutralTracks.Contains(track.type)  ? 1 : 
                objective.PositiveTracks.Contains(track.type) ? 2 : -1;
                
            if(obj != -1)
                track.SetObjective(obj);
        }
    }

    public void UpdateAllTracks()
    {
        foreach (var track in trackReferences)
            track.UpdateValue();
    }
}
using System;

namespace Client
{
    [Serializable]
    public struct CrisisStartDTO
    {
        public string CrisisId;
        public double CrisisEndTimeUtc;
    }

    [Serializable]
    public struct PlayerPublicCommitDTO
    {
        public string PlayerId;
        public int TotalCommitted;
        public int SuccessPoints;
    }

    [Serializable]
    public struct CrisisPublicResultDTO
    {
        public string CrisisId;
        public bool Success;
        public string HighestBidderId;
        public string LowestBidderId;
        public int TotalSuccessPoints;
        public int BunkerIntegrityAfter;
        public PlayerPublicCommitDTO[] PublicCommits; // empty if hidden
    }

    [Serializable]
    public struct PlayerResolveDTO
    {
        public string PlayerId;
        public bool GlobalSuccess;
        public int PlayerTotalCommitted;
        public int PlayerSuccessPoints;
        public bool WasHighestBidder;
        public bool WasLowestBidder;
        public ResourceAmount[] ResourceDeltas; // amounts to apply to this player
    }
}
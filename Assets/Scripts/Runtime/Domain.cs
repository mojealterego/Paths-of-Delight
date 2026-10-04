using System;
using System.Collections.Generic;

namespace PathsOfDelight
{
    public enum Edition { Play, Adult }
    public enum AnswerChoice { No, Maybe, Yes }
    public enum ConsentChoice { Decline, Accept }
    public enum AppScreen { Welcome, ProfileA, ProfileB, Lobby, Board, PrivateAnswerA, PrivateAnswerB, MatchResult, ConsentA, ConsentB, Activity, Paused, Summary, Privacy, CustomContent }

    [Serializable]
    public sealed class PlayerProfile
    {
        public string displayName = "Partner";
        public int age = 18;
        public int intensityCeiling = 1;
        public List<string> blockedTags = new List<string>();
    }

    [Serializable]
    public sealed class ContentCard
    {
        public string id;
        public string prompt;
        public string activity;
        public int intensity;
        public string edition;
        public string[] tags;
        public int rewardPoints;
    }

    [Serializable]
    public sealed class CardCatalog
    {
        public ContentCard[] cards;
    }

    [Serializable]
    public sealed class SessionState
    {
        public string sessionId;
        public int seed;
        public int turn;
        public int boardPosition;
        public int sharedPoints;
        public bool paused;
        public string currentCardId;
        public List<string> completedCardIds = new List<string>();
    }

    [Serializable]
    public sealed class LocalSave
    {
        public PlayerProfile playerA = new PlayerProfile { displayName = "Partner A" };
        public PlayerProfile playerB = new PlayerProfile { displayName = "Partner B" };
        public SessionState session = new SessionState();
        public List<ContentCard> customCards = new List<ContentCard>();
        public bool privacyAccepted;
    }
}

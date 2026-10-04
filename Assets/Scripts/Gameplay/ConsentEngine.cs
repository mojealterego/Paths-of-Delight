namespace PathsOfDelight
{
    public static class ConsentEngine
    {
        public static bool IsBlindMatch(AnswerChoice a, AnswerChoice b) => a == AnswerChoice.Yes && b == AnswerChoice.Yes;
        public static bool CanStartActivity(ConsentChoice a, ConsentChoice b) => a == ConsentChoice.Accept && b == ConsentChoice.Accept;
        public static int SharedReward(bool completedVoluntarily, int configuredReward) => completedVoluntarily ? System.Math.Max(0, configuredReward) : 0;
    }
}

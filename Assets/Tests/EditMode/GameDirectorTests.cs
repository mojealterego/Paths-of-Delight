using System.Collections.Generic;
using NUnit.Framework;

namespace PathsOfDelight.Tests
{
    public sealed class GameDirectorTests
    {
        [Test]
        public void BlindMatch_RevealsOnlyMutualYes()
        {
            Assert.IsTrue(ConsentEngine.IsBlindMatch(AnswerChoice.Yes, AnswerChoice.Yes));
            Assert.IsFalse(ConsentEngine.IsBlindMatch(AnswerChoice.Yes, AnswerChoice.No));
            Assert.IsFalse(ConsentEngine.IsBlindMatch(AnswerChoice.Yes, AnswerChoice.Maybe));
        }

        [Test]
        public void Consent_RequiresBothAndNeverRewardsDecline()
        {
            Assert.IsTrue(ConsentEngine.CanStartActivity(ConsentChoice.Accept, ConsentChoice.Accept));
            Assert.IsFalse(ConsentEngine.CanStartActivity(ConsentChoice.Accept, ConsentChoice.Decline));
            Assert.AreEqual(0, ConsentEngine.SharedReward(false, 5));
        }

        [Test]
        public void Director_RespectsLowerIntensityAndBlockedTags()
        {
            var cards = new List<ContentCard>
            {
                new ContentCard { id = "a", intensity = 1, edition = "play", tags = new[] { "connection" } },
                new ContentCard { id = "b", intensity = 2, edition = "play", tags = new[] { "roleplay" } },
                new ContentCard { id = "c", intensity = 3, edition = "adult", tags = new[] { "props" } }
            };
            var a = new PlayerProfile { intensityCeiling = 3 };
            var b = new PlayerProfile { intensityCeiling = 1 };
            b.blockedTags.Add("roleplay");
            var state = new SessionState { seed = 1, turn = 0, boardPosition = 0 };
            var selected = new GameDirector().SelectNext(cards, a, b, state, Edition.Play);
            Assert.NotNull(selected);
            Assert.AreEqual("a", selected.id);
        }

        [Test]
        public void PlayEdition_NeverSelectsAdultContent()
        {
            var cards = new List<ContentCard>
            {
                new ContentCard { id = "play", intensity = 1, edition = "play", tags = new string[0] },
                new ContentCard { id = "adult", intensity = 1, edition = "adult", tags = new string[0] }
            };
            var profile = new PlayerProfile { intensityCeiling = 3 };
            var state = new SessionState { seed = 7 };
            for (var i = 0; i < 10; i++)
            {
                state.turn = i;
                var selected = new GameDirector().SelectNext(cards, profile, profile, state, Edition.Play);
                Assert.AreEqual("play", selected.id);
            }
        }
    }
}

using System.Collections.Generic;
using NUnit.Framework;
using ReferenceDeletion.Core;
using System.Linq;

namespace ReferenceDeletion.Tests
{
    [TestFixture]
    public sealed class ReferenceIndexTests
    {
        [Test]
        public void ReverseIndex_AddLink_MakesTargetQueryable()
        {
            ReverseReferenceIndex index = new ReverseReferenceIndex();

            index.AddLink("sword-guid", "player-guid");
            index.AddLink("sword-guid", "enemy-guid");

            IReadOnlyCollection<string> referencers = index.GetReferencingGuids("sword-guid");

            Assert.AreEqual(2, referencers.Count);
            Assert.IsTrue(referencers.Contains("player-guid"));
            Assert.IsTrue(referencers.Contains("enemy-guid"));
        }

        [Test]
        public void ReverseIndex_RemoveLink_ClearsEmptySets()
        {
            ReverseReferenceIndex index = new ReverseReferenceIndex();
            index.AddLink("sword-guid", "player-guid");

            index.RemoveLink("sword-guid", "player-guid");

            Assert.AreEqual(0, index.GetReferencingGuids("sword-guid").Count);
        }

        [Test]
        public void ReverseIndex_RemoveAllLinksFrom_RemovesOnlyThatSource()
        {
            ReverseReferenceIndex index = new ReverseReferenceIndex();
            index.AddLink("sword-guid", "player-guid");
            index.AddLink("sword-guid", "enemy-guid");

            index.RemoveAllLinksFrom("player-guid", new[] { "sword-guid" });

            IReadOnlyCollection<string> referencers = index.GetReferencingGuids("sword-guid");
            Assert.AreEqual(1, referencers.Count);
            Assert.IsTrue(referencers.Contains("enemy-guid"));
        }

        [Test]
        public void ForwardIndex_Set_And_Get_RoundTrips()
        {
            ForwardReferenceIndex index = new ForwardReferenceIndex();
            HashSet<string> refs = new HashSet<string> { "a", "b", "c" };

            index.Set("player-guid", refs);
            HashSet<string> retrieved = index.Get("player-guid");

            Assert.AreEqual(3, retrieved.Count);
        }

        [Test]
        public void ForwardIndex_Get_UnknownGuid_ReturnsEmptySet()
        {
            ForwardReferenceIndex index = new ForwardReferenceIndex();
            Assert.AreEqual(0, index.Get("unknown").Count);
        }
    }
}

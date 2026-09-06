using System;
using System.Collections.Generic;
using NUnit.Framework;
using ReferenceDeletion.Utils;

namespace ReferenceDeletion.Tests
{
    [TestFixture]
    public sealed class GuidParserTests
    {
        [Test]
        public void ExtractGuids_FindsSingleGuid()
        {
            string yaml = "m_Material: {fileID: 2100000, guid: 3f2a9c1b7e6d4a4a8b0e1c2d3e4f5061, type: 2}";
            HashSet<string> result = new HashSet<string>();

            GuidParser.ExtractGuids(yaml.AsSpan(), result);

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.Contains("3f2a9c1b7e6d4a4a8b0e1c2d3e4f5061"));
        }

        [Test]
        public void ExtractGuids_FindsMultipleUniqueGuids()
        {
            string yaml =
                "guid: 3f2a9c1b7e6d4a4a8b0e1c2d3e4f5061\n" +
                "guid: aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\n" +
                "guid: 3f2a9c1b7e6d4a4a8b0e1c2d3e4f5061\n"; // duplicate
            HashSet<string> result = new HashSet<string>();

            GuidParser.ExtractGuids(yaml.AsSpan(), result);

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void ExtractGuids_ExcludesSelfGuid()
        {
            string selfGuid = "3f2a9c1b7e6d4a4a8b0e1c2d3e4f5061";
            string yaml = $"guid: {selfGuid}\nguid: aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\n";
            HashSet<string> result = new HashSet<string>();

            GuidParser.ExtractGuids(yaml.AsSpan(), result, selfGuid);

            Assert.AreEqual(1, result.Count);
            Assert.IsFalse(result.Contains(selfGuid));
        }

        [Test]
        public void ExtractGuids_IgnoresMalformedShortGuid()
        {
            string yaml = "guid: deadbeef\n";
            HashSet<string> result = new HashSet<string>();

            GuidParser.ExtractGuids(yaml.AsSpan(), result);

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void ExtractGuids_EmptyText_ReturnsNoResults()
        {
            HashSet<string> result = new HashSet<string>();
            GuidParser.ExtractGuids(string.Empty.AsSpan(), result);
            Assert.AreEqual(0, result.Count);
        }
    }
}

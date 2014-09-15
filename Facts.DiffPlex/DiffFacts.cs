﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiffPlex;
using Xunit;

namespace Facts.DiffPlex
{
    public class DiffFacts
    {
        private IList<string> baselineContent;

        public DiffFacts()
        {
            this.baselineContent = new List<string> {
                "abc",
                "def",
                "ghi",
            };
        }

        [Fact]
        public void CompareLines_Unchanged()
        {
            var result = Diff.CompareLines(this.baselineContent, this.baselineContent);
            Assert.NotNull(result);
            Assert.Equal(3, result.Inline.Count);
            Assert.True(result.Inline.All(l => l.Type == ChangeType.Unchanged));
            Assert.Equal(this.baselineContent, result.Inline.Select(l => l.Text));
        }

        [Fact]
        public void CompareLines_AddedTopLine()
        {
            var after = this.baselineContent.ToList();
            after.Insert(0, "foo");
            var result = Diff.CompareLines(this.baselineContent, after);
            Assert.NotNull(result);
            Assert.Equal(4, result.Inline.Count);
            Assert.Equal(ChangeType.Inserted, result.Inline[0].Type);
            Assert.Equal(after[0], result.Inline[0].Text);
            Assert.True(result.Inline.Skip(1).All(l => l.Type == ChangeType.Unchanged));
            Assert.Equal(this.baselineContent, result.Inline.Skip(1).Select(l => l.Text));
        }

        [Fact]
        public void CompareLines_SingleString_ExactlyEqual()
        {
            string[] lineEndingSequences = new string[] { "\n", "\r", "\r\n" };
            foreach (string lineEndingSequence in lineEndingSequences)
            {
                string baseline = string.Join(lineEndingSequence, this.baselineContent);
                var result = Diff.CompareLines(baseline, baseline);
                Assert.Equal(this.baselineContent.Count, result.Inline.Count);
                Assert.True(result.Inline.All(l => l.Type == ChangeType.Unchanged));
            }
        }

        [Fact]
        public void CompareLines_SingleString_VaryingLineEndings()
        {
            string[] lineEndingSequences = new string[] { "\n", "\r" };
            foreach (string lineEndingSequence in lineEndingSequences)
            {
                string baseline = string.Join(lineEndingSequence, this.baselineContent);
                string changed = string.Join("\r\n", this.baselineContent);

                var result = Diff.CompareLines(baseline, changed);
                Assert.Equal(5, result.Inline.Count);
                Assert.True(result.Inline.Take(2).All(l => l.Type == ChangeType.Deleted));
                Assert.True(result.Inline.Skip(2).Take(2).All(l => l.Type == ChangeType.Inserted));
                Assert.True(result.Inline.Skip(5).All(l => l.Type == ChangeType.Unchanged));

                result = Diff.CompareLines(baseline, changed, Diff.Options.IgnoreWhitespace);
                Assert.Equal(this.baselineContent.Count, result.Inline.Count);
                Assert.True(result.Inline.All(l => l.Type == ChangeType.Unchanged));
            }
        }
    }
}

﻿using NUnit.Framework;
using System;
using System.Linq;

namespace ParseDiff.Tests
{
    [TestFixture]
    public class TestDiff
    {
        [Test]
        public void ShouldParseNull() =>
            Assert.AreEqual(0, Diff.Parse(null).Count());

        [Test]
        public void ShouldParseEmptyString() =>
            Assert.AreEqual(0, Diff.Parse(string.Empty).Count());

        [Test]
        public void ShouldParseWhitespace() =>
            Assert.AreEqual(0, Diff.Parse(" ").Count());

        [Test]
        public void ShouldParseSimpleGitLikeDiff()
        {
            var diff = @"
diff --git a/file b/file
index 123..456 789
--- a/file
+++ b/file
@@ -1,2 +1,2 @@
- line1
+ line2";
            var files = Diff.Parse(diff, Environment.NewLine).ToArray();
            Assert.AreEqual(1, files.Length);
            var file = files[0];
            Assert.AreEqual("file", file.From);
            Assert.AreEqual("file", file.To);
            Assert.AreEqual(1, file.Chunks.Count());
            var chunk = file.Chunks.First();
            Assert.AreEqual("@@ -1,2 +1,2 @@", chunk.Content);
            var changes = chunk.Changes.ToArray();
            Assert.AreEqual(2, changes.Count());
            Assert.AreEqual("- line1", changes[0].Content);
            Assert.AreEqual("+ line2", changes[1].Content);
        }

        [Test]
        public void ShouldParseDiffWIthDeletedFileModeLine()
        {
            var diff = @"
diff --git a/test b/test
deleted file mode 100644
index db81be4..0000000
--- b/test
+++ /dev/null
@@ -1,2 +0,0 @@
-line1
-line2
";
            var files = Diff.Parse(diff, Environment.NewLine).ToArray();
            Assert.AreEqual(1, files.Length);
            var file = files[0];
            Assert.AreEqual(FileChangeType.Delete, file.Type);
            Assert.AreEqual("test", file.From);
            Assert.AreEqual("/dev/null", file.To);
            var chunk = file.Chunks.First();
            Assert.AreEqual("@@ -1,2 +0,0 @@", chunk.Content);
            Assert.AreEqual(2, chunk.Changes.Count());
            var changes = chunk.Changes.ToArray();
            Assert.AreEqual("-line1", changes[0].Content);
            Assert.AreEqual("-line2", changes[1].Content);
        }

        [Test]
        public void ShouldParseDiffWithNewFileModeLine()
        {
            var diff = @"
diff --git a/test b/test
new file mode 100644
index 0000000..db81be4
--- /dev/null
+++ b/test
@@ -0,0 +1,2 @@
+line1
+line2
";
            var files = Diff.Parse(diff, Environment.NewLine).ToArray();
            Assert.AreEqual(1, files.Length);
            var file = files[0];
            Assert.AreEqual(true, file.Type == FileChangeType.Add);
            Assert.AreEqual("/dev/null", file.From);
            Assert.AreEqual("test", file.To);
            Assert.AreEqual("@@ -0,0 +1,2 @@", file.Chunks.ElementAt(0).Content);
            Assert.AreEqual(2, file.Chunks.ElementAt(0).Changes.Count());
            Assert.AreEqual("+line1", file.Chunks.ElementAt(0).Changes.ElementAt(0).Content);
            Assert.AreEqual("+line2", file.Chunks.ElementAt(0).Changes.ElementAt(1).Content);
        }

        [Test]
        public void ShouldParseDiffWithDeletedFileModeLine()
        {
            var diff = @"
diff --git a/test b/test
deleted file mode 100644
index db81be4..0000000
--- b/test
+++ /dev/null
@@ -1,2 +0,0 @@
-line1
-line2
";
            var files = Diff.Parse(diff, Environment.NewLine).ToArray();
            Assert.AreEqual(1, files.Length);
            var file = files[0];
            Assert.AreEqual(true, file.Type == FileChangeType.Delete);
            Assert.AreEqual("test", file.From);
            Assert.AreEqual("/dev/null", file.To);
            Assert.AreEqual("@@ -1,2 +0,0 @@", file.Chunks.ElementAt(0).Content);
            Assert.AreEqual(2, file.Chunks.ElementAt(0).Changes.Count());
            Assert.AreEqual("-line1", file.Chunks.ElementAt(0).Changes.ElementAt(0).Content);
            Assert.AreEqual("-line2", file.Chunks.ElementAt(0).Changes.ElementAt(1).Content);
        }

        [Test]
        public void ShouldParseDiffWithSingleLineFiles()
        {
            var diff = @"
diff --git a/file1 b/file1
deleted file mode 100644
index db81be4..0000000
--- b/file1
+++ /dev/null
@@ -1 +0,0 @@
-line1
diff --git a/file2 b/file2
new file mode 100644
index 0000000..db81be4
--- /dev/null
+++ b/file2
@@ -0,0 +1 @@
+line1
";
            var files = Diff.Parse(diff, Environment.NewLine).ToArray();
            Assert.AreEqual(2, files.Length);
            var file = files[0];
            Assert.AreEqual(true, file.Deleted);
            Assert.AreEqual("file1", file.From);
            Assert.AreEqual("/dev/null", file.To);
            Assert.AreEqual("@@ -1 +0,0 @@", file.Chunks.ElementAt(0).Content);
            Assert.AreEqual(1, file.Chunks.ElementAt(0).Changes.Count());
            Assert.AreEqual("-line1", file.Chunks.ElementAt(0).Changes.ElementAt(0).Content);
            Assert.AreEqual(LineChangeType.Delete, file.Chunks.ElementAt(0).Changes.ElementAt(0).ChangeType);
            file = files[1];
            Assert.AreEqual(true, file.Add);
            Assert.AreEqual("/dev/null", file.From);
            Assert.AreEqual("file2", file.To);
            Assert.AreEqual("@@ -0,0 +1 @@", file.Chunks.ElementAt(0).Content);
            Assert.AreEqual(0, file.Chunks.ElementAt(0).NewLines);
            Assert.AreEqual(1, file.Chunks.ElementAt(0).Changes.Count());
            Assert.AreEqual("+line1", file.Chunks.ElementAt(0).Changes.ElementAt(0).Content);
            Assert.AreEqual(LineChangeType.Add, file.Chunks.ElementAt(0).Changes.ElementAt(0).ChangeType);
        }

        [Test]
        public void ShouldParseMultipleFilesInDiff()
        {
            var diff = @"
diff --git a/file1 b/file1
index 123..456 789
--- a/file1
+++ b/file1
@@ -1,2 +1,2 @@
- line1
+ line2
diff --git a/file2 b/file2
index 123..456 789
--- a/file2
+++ b/file2
@@ -1,3 +1,3 @@
- line1
+ line2
";
            var files = Diff.Parse(diff, Environment.NewLine).ToArray();
            Assert.AreEqual(2, files.Length);
            var file = files[0];
            Assert.AreEqual("file1", file.From);
            Assert.AreEqual("file1", file.To);
            Assert.AreEqual("@@ -1,2 +1,2 @@", file.Chunks.ElementAt(0).Content);
            Assert.AreEqual(2, file.Chunks.ElementAt(0).Changes.Count());
            Assert.AreEqual("- line1", file.Chunks.ElementAt(0).Changes.ElementAt(0).Content);
            Assert.AreEqual("+ line2", file.Chunks.ElementAt(0).Changes.ElementAt(1).Content);
            file = files[1];
            Assert.AreEqual("file2", file.From);
            Assert.AreEqual("file2", file.To);
            Assert.AreEqual("@@ -1,3 +1,3 @@", file.Chunks.ElementAt(0).Content);
            Assert.AreEqual(2, file.Chunks.ElementAt(0).Changes.Count());
            Assert.AreEqual("- line1", file.Chunks.ElementAt(0).Changes.ElementAt(0).Content);
            Assert.AreEqual("+ line2", file.Chunks.ElementAt(0).Changes.ElementAt(1).Content);
        }

        [Test]
        public void ShouldParseGnuSampleDiff()
        {
            var diff = @"
--- lao	2002-02-21 23:30:39.942229878 -0800
+++ tzu	2002-02-21 23:30:50.442260588 -0800
@@ -1,7 +1,6 @@
-The Way that can be told of is not the eternal Way;
-The name that can be named is not the eternal name.
The Nameless is the origin of Heaven and Earth;
-The Named is the mother of all things.
+The named is the mother of all things.
+
Therefore let there always be non-being,
	so we may see their subtlety,
And let there always be being,
@@ -9,3 +8,6 @@
The two are the same,
But after they are produced,
	they have different names.
+They both may be called deep and profound.
+Deeper and more profound,
+The door of all subtleties!
";
            var files = Diff.Parse(diff, Environment.NewLine).ToArray();
            Assert.AreEqual(1, files.Length);
            var file = files[0];
            Assert.AreEqual("lao", file.From);
            Assert.AreEqual("tzu", file.To);
            Assert.AreEqual(2, file.Chunks.Count());
            var chunk0 = file.Chunks.ElementAt(0);
    
            Assert.AreEqual(1, chunk0.OldStart);
            Assert.AreEqual(7, chunk0.OldLines);
            Assert.AreEqual(1, chunk0.NewStart);
            Assert.AreEqual(6, chunk0.NewLines);
            var chunk1 = file.Chunks.ElementAt(1);
    
            Assert.AreEqual(9, chunk1.OldStart);
            Assert.AreEqual(3, chunk1.OldLines);
            Assert.AreEqual(8, chunk1.NewStart);
            Assert.AreEqual(6, chunk1.NewLines);
        }

        [Test]
        public void ShouldPArseHgDiffOutput()
        {
            var diff = @"
diff -r 514fc757521e lib/parsers.coffee
--- a/lib/parsers.coffee	Thu Jul 09 00:56:36 2015 +0200
+++ b/lib/parsers.coffee	Fri Jul 10 16:23:43 2015 +0200
@@ -43,6 +43,9 @@
             files[file] = { added: added, deleted: deleted }
         files
+    diff: (out) ->
+        files = {}
+
 module.exports = Parsers
 module.exports.version = (out) ->
";
            var files = Diff.Parse(diff, Environment.NewLine).ToArray();
            Assert.AreEqual(1, files.Length);
            var file = files[0];
            Assert.AreEqual("@@ -43,6 +43,9 @@", file.Chunks.ElementAt(0).Content);
            Assert.AreEqual("lib/parsers.coffee", file.From);
            Assert.AreEqual("lib/parsers.coffee", file.To);
        }

        [Test]
        public void ShouldParseFileNamesForNNewEmptyFile()
        {
            var diff = @"
diff --git a/newFile.txt b/newFile.txt
new file mode 100644
index 0000000..e6a2e28
";
            var files = Diff.Parse(diff, Environment.NewLine).ToArray();
            Assert.AreEqual(1, files.Length);
            var file = files[0];
            Assert.AreEqual("/dev/null", file.From);
            Assert.AreEqual("newFile.txt", file.To);
        }

        [Test]
        public void ShouldParseFileNamesForADeletedFile()
        {
            var diff = @"
diff --git a/deletedFile.txt b/deletedFile.txt
deleted file mode 100644
index e6a2e28..0000000
";
            var files = Diff.Parse(diff, Environment.NewLine).ToArray();
            Assert.AreEqual(1, files.Length);
            var file = files[0];
            Assert.AreEqual("deletedFile.txt", file.From);
            Assert.AreEqual("/dev/null", file.To);
        }
    }
}
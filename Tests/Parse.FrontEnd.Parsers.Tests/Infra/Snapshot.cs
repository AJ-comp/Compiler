using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;

namespace Parse.FrontEnd.Parsers.Tests.Infra;

/// <summary>
/// Minimal golden-master (approval) helper. On the first run for a given name it
/// captures the current output as the approved baseline; on later runs it asserts the
/// output is unchanged. This is exactly the safety net we want before refactoring:
/// it pins whatever the engine produces TODAY, then screams if a refactor alters it.
///
/// Approved baselines live next to this file under <c>__snapshots__/</c> and are meant
/// to be committed. When a change is intentional, replace the *.approved.txt with the
/// emitted *.received.txt.
/// </summary>
internal static class Snapshot
{
    private static string SnapshotDir([CallerFilePath] string thisFilePath = "")
        => Path.Combine(Path.GetDirectoryName(thisFilePath), "__snapshots__");

    public static void Match(string name, string actual)
    {
        var dir = SnapshotDir();
        Directory.CreateDirectory(dir);

        actual = Normalize(actual);
        var approvedPath = Path.Combine(dir, name + ".approved.txt");

        if (!File.Exists(approvedPath))
        {
            // First run: capture current behavior as the baseline and pass.
            File.WriteAllText(approvedPath, actual, new UTF8Encoding(false));
            return;
        }

        var expected = Normalize(File.ReadAllText(approvedPath));
        if (expected == actual) return;

        var receivedPath = Path.Combine(dir, name + ".received.txt");
        File.WriteAllText(receivedPath, actual, new UTF8Encoding(false));

        Assert.Fail(
            "Snapshot '" + name + "' differs from the approved baseline.\n" +
            "  approved: " + approvedPath + "\n" +
            "  received: " + receivedPath + "\n" +
            "If this change is intended, overwrite the .approved.txt with the .received.txt.");
    }

    private static string Normalize(string s)
    {
        s = s.Replace("\r\n", "\n").Replace("\r", "\n");
        var lines = s.Split('\n');
        for (int i = 0; i < lines.Length; i++) lines[i] = lines[i].TrimEnd();
        return string.Join("\n", lines).TrimEnd();
    }
}

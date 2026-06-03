namespace PassphraseGenerator.Tests.TestFixtures;

/// <summary>Small in-memory word list for use in tests (no HTTP calls).</summary>
public static class TestWordlist
{
    /// <summary>A minimal but valid Diceware word list for testing.</summary>
    public static readonly Dictionary<string, string> English = new(StringComparer.Ordinal)
    {
        ["11111"] = "aardvark",
        ["11112"] = "abacus",
        ["11113"] = "abbey",
        ["11114"] = "abbot",
        ["11115"] = "abduct",
        ["11116"] = "abeam",
        ["11121"] = "abets",
        ["11122"] = "abhor",
        ["11123"] = "abide",
        ["11124"] = "abject",
        ["11125"] = "ablaze",
        ["11126"] = "ablest",
        ["11131"] = "absurd",
        ["11132"] = "accent",
        ["11133"] = "access",
        ["11134"] = "accord",
        ["11135"] = "accrue",
        ["11136"] = "accuse",
        ["11141"] = "achieve",
        ["11142"] = "acidly",
        ["11143"] = "acking",
        ["11144"] = "acorn",
        ["11145"] = "across",
        ["11146"] = "acting",
        ["11151"] = "active",
        ["11152"] = "actual",
        ["11153"] = "acumen",
        ["11154"] = "adding",
        ["11155"] = "adhere",
        ["11156"] = "admire",
    };

    /// <summary>A minimal Dutch word list for testing.</summary>
    public static readonly Dictionary<string, string> Dutch = new(StringComparer.Ordinal)
    {
        ["11111"] = "aarde",
        ["11112"] = "aardei",
        ["11113"] = "aardgas",
        ["11114"] = "aardige",
        ["11115"] = "aardman",
        ["11116"] = "aardpeer",
        ["11121"] = "aardrijks",
        ["11122"] = "aardvark",
        ["11123"] = "aardvlo",
        ["11124"] = "aardworm",
        ["11125"] = "aarzel",
        ["11126"] = "abacus",
        ["11131"] = "aband",
        ["11132"] = "abonnee",
        ["11133"] = "abrikoos",
        ["11134"] = "absoluut",
        ["11135"] = "abstract",
        ["11136"] = "accenten",
        ["11141"] = "accordeon",
        ["11142"] = "achter",
        ["11143"] = "achtbaan",
        ["11144"] = "acteur",
        ["11145"] = "actief",
        ["11146"] = "acties",
        ["11151"] = "adam",
        ["11152"] = "adder",
        ["11153"] = "ademen",
        ["11154"] = "agenda",
        ["11155"] = "agent",
        ["11156"] = "alarm",
    };

    public static string ToJson(Dictionary<string, string> wordlist)
    {
        var entries = wordlist.Select(kv => $@"""{kv.Key}"":""{kv.Value}""");
        return "{" + string.Join(",", entries) + "}";
    }
}

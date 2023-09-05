namespace InfiniteDecimal.CodeGenerator;

public static class BigDecConstructor
{
    public static async Task Generate()
    {
        var operators = new (string oper1, string oper2)[]
        {
            ("==", "!="),
            (">", "<="),
            ("<", ">="),
        };

        var types = new[]
        {
            "BigDec",
            "BigInteger",
            "long",
            "ulong",
            "int",
            "uint",
            "short",
            "ushort",
            "sbyte",
            "byte",
            "float",
            "double",
            "decimal",
        };

        var template = await File.ReadAllTextAsync("template.txt");

        foreach (var type in types)
        {
            var text = "";
            foreach (var (oper1, oper2) in operators)
            {
                text += Generate(type, oper1, oper2);
                text += "\n\n";
                text += Generate(type, oper2, oper1);
                text += "\n\n";
            }

            while (true)
            {
                var s = text.Replace("\n\n\n", "\n\n");
                if (s == text)
                {
                    break;
                }

                text = s;
            }

            text = string.Format(template, text);
            await File.WriteAllTextAsync($"./BigDec.Operators_{type.ToLowerInvariant()}.cs", text);
        }

        Console.WriteLine(Environment.CurrentDirectory);
    }

    private static string OperatorTemplate1 =
        "[MethodImpl(MethodImplOptions.AggressiveInlining)]\n" +
        "public static bool operator {0}(BigDec a, {1} b)\n" +
        "{{\n" +
        "    return {2}({3});\n" +
        "}}\n";

    private static string OperatorTemplate2 =
        "[MethodImpl(MethodImplOptions.AggressiveInlining)]\n" +
        "public static bool operator {0}({1} b, BigDec a)\n" +
        "{{\n" +
        "    return {2}({3});\n" +
        "}}\n";

    public static string Generate(string type, string oper1, string oper2)
    {
        if (type != "BigDec")
        {
            var isCommutative = (oper1 is "==" or "!=");
            var text1 = string.Format(
                OperatorTemplate1,
                oper1,
                type,
                string.Empty,
                $"a {oper1} new BigDec(b)"
            );
            var text2 = string.Format(
                OperatorTemplate2,
                oper1,
                type,
                string.Empty,
                isCommutative ? $"a {oper1} new BigDec(b)" : $"new BigDec(b) {oper1} a"
            );

            return text1 + "\n\n" + text2;
        }

        if (oper1 is ">" or "==")
        {
            return string.Empty;
        }

        // BigDec
        var expr = $"a {oper2} b";
        var text = string.Format(
            OperatorTemplate1,
            oper1,
            type,
            "!",
            expr
        );
        return text;
    }
}
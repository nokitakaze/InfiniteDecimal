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
            var operatorText = "";
            foreach (var (oper1, oper2) in operators)
            {
                operatorText += GenerateOperator(type, oper1, oper2);
                operatorText += "\n\n";
                operatorText += GenerateOperator(type, oper2, oper1);
                operatorText += "\n\n";
            }

            var mathOperations = "";
            if (type != "BigDec")
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var oper in new string[] { "+", "-", "/", "*" })
                {
                    mathOperations += GenerateMathOperation(type, oper);
                }
            }

            var text = string.Format(
                template,
                operatorText,
                mathOperations
            );

            while (true)
            {
                var s = text.Replace("\n\n\n", "\n\n");
                if (s == text)
                {
                    break;
                }

                text = s;
            }

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

    public static string GenerateOperator(string type, string oper1, string oper2)
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

    private static string MathOperationTemplate1 =
        "[MethodImpl(MethodImplOptions.AggressiveInlining)]\n" +
        "public static BigDec operator {0}(BigDec a, {1} b)\n" +
        "{{\n" +
        "    return a {0} new BigDec(b);\n" +
        "}}\n";

    private static string MathOperationTemplate2 =
        "[MethodImpl(MethodImplOptions.AggressiveInlining)]\n" +
        "public static BigDec operator {0}({1} b, BigDec a)\n" +
        "{{\n" +
        "    return new BigDec(b) {0} a;\n" +
        "}}\n";

    public static string GenerateMathOperation(string type, string oper)
    {
        if (type == "BigDec")
        {
            return string.Empty;
        }

        var text1 = string.Empty;
        if ((type == "BigInteger") && (oper is "+" or "*"))
        {
        }
        else
        {
            text1 = string.Format(
                MathOperationTemplate1,
                oper,
                type
            );
        }

        var text2 = string.Format(
            MathOperationTemplate2,
            oper,
            type
        );

        return text1 + "\n\n" + text2 + "\n\n";
    }
}
namespace Janglim.Mcp;

public static class RequestInputValidator
{
    public static WorkerResponse? Validate(WorkerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Grammar))
            return Invalid("A non-empty EBNF grammar is required.");
        if (request.Grammar.Length > McpLimits.MaxGrammarChars)
            return Invalid($"Grammar exceeds {McpLimits.MaxGrammarChars} UTF-16 code units.");

        switch (request.Operation)
        {
            case "validate_grammar":
                break;
            case "parse_input":
                if (request.Input is null) return Invalid("Input is required; use an empty string to test empty input.");
                if (request.Input.Length > McpLimits.MaxInputChars)
                    return Invalid($"Input exceeds {McpLimits.MaxInputChars} UTF-16 code units.");
                break;
            case "run_tests":
                if (request.Tests is null || request.Tests.Length is 0 or > McpLimits.MaxTests)
                    return Invalid($"Provide 1 to {McpLimits.MaxTests} test cases.");
                var total = 0;
                foreach (var test in request.Tests)
                {
                    if (test is null || string.IsNullOrWhiteSpace(test.Name) || test.Name.Length > 128 || test.Input is null)
                        return Invalid("Every case needs a name (1–128 characters), input, and expectSuccess.");
                    if (test.Input.Length > McpLimits.MaxInputChars)
                        return Invalid($"A test input exceeds {McpLimits.MaxInputChars} UTF-16 code units.");
                    total += test.Input.Length;
                }
                if (total > McpLimits.MaxTestInputChars)
                    return Invalid($"Combined test inputs exceed {McpLimits.MaxTestInputChars} UTF-16 code units.");
                break;
            default:
                return Invalid("Unknown operation.");
        }
        return null;
    }

    private static WorkerResponse Invalid(string message) => new("invalid_request", message);
}

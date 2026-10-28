$ErrorActionPreference = 'Stop'
$bin = (Resolve-Path -LiteralPath 'artifacts/typescript/lookahead-fix/test-bin').Path
foreach ($name in @('AJ.Common.dll', 'Parse.dll', 'Parse.FrontEnd.dll', 'Parse.FrontEnd.Grammars.dll', 'Parse.FrontEnd.Grammars.Ebnf.dll', 'Parse.FrontEnd.Parsers.dll')) {
    [void][Reflection.Assembly]::LoadFrom((Join-Path $bin $name))
}
$language = [Janglim.FrontEnd.Grammars.TypeScript.TypeScriptGrammar]::new(
    [Janglim.FrontEnd.Grammars.TypeScript.TypeScriptInputKind]::Type,
    [Janglim.FrontEnd.Grammars.JavaScript.JavaScriptMode]::NonStrict)
$parser = [Janglim.FrontEnd.Parsers.LR.LALRParser]::new($language.Grammar, $true)
Write-Output ('STATES ' + $parser.Canonical.IndexStateDic.Count + '; CONFLICTS ' + $parser.Conflicts.Count)
$contextProperty = [Janglim.FrontEnd.Parsers.Datas.ParsingUnit].GetProperty('PredicateContext', [Reflection.BindingFlags]'Instance,NonPublic')
$inputs = @('(value: unknown) => asserts value', '(value: unknown) => asserts value is string', '(value: unknown) => value is string', 'asserts', 'asserts value', 'asserts this')
foreach ($inputText in $inputs) {
    Write-Output ('INPUT ' + $inputText)
    $parsed = $parser.Parsing($language.CreateTokenSource($inputText))
    Write-Output ('SUCCESS ' + $parsed.Success + '; EXCEPTION ' + $parser.FiredException)
    foreach ($block in $parsed) {
        foreach ($unit in $block.Units) {
            $top = $unit.BeforeStack.Stack.Peek()
            if ($top -isnot [int]) { $top = 'goto' }
            $action = $unit.Action
            $destination = if ($action.Dest -is [Janglim.FrontEnd.RegularGrammar.NonTerminalSingle]) { $action.Dest.ToGrammarString() } else { [string]$action.Dest }
            $context = $contextProperty.GetValue($unit)
            $previous = if ($null -ne $context.PreviousToken) { $context.PreviousToken.Input } else { '<none>' }
            Write-Output ('  I' + $top + ' token=' + $unit.InputValue.Input + ' [' + $unit.InputValue.Kind.Caption + '] prev=' + $previous + ' :: ' + $action.Direction + ' ' + $destination + ' :: ' + $unit.ErrorMessage)
            if ($top -is [int]) {
                $cell = $parser.ParsingTable[$top].MatchedValueSet[$unit.InputValue.Kind]
                if ($cell.Count -gt 1 -or $unit.IsError) {
                    foreach ($candidate in $cell) {
                        $target = if ($candidate.Dest -is [Janglim.FrontEnd.RegularGrammar.NonTerminalSingle]) { $candidate.Dest.ToGrammarString() } else { [string]$candidate.Dest }
                        $predicate = $candidate.Dest.Predicate
                        $decision = if ($null -ne $predicate) { ' guard=' + $predicate.Name + ' value=' + $predicate.Evaluate($context) } else { '' }
                        Write-Output ('    CANDIDATE ' + $candidate.Direction + ' ' + $target + $decision)
                    }
                }
            }
        }
    }
}

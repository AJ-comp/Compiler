# Diagnostic replay only. Original source, parser table and LR items remain unchanged.
$probeDir = Join-Path (Get-Location) 'artifacts/typescript/lookahead-repro/bin/Release/net8.0'
foreach ($name in @('AJ.Common','Parse','Parse.FrontEnd','Parse.FrontEnd.Grammars','Parse.FrontEnd.Grammars.Ebnf','Parse.FrontEnd.Parsers')) {
    [void][System.Reflection.Assembly]::LoadFrom((Join-Path $probeDir ($name + '.dll')))
}
$grammar = @'
S : {Always()}? C ;
C : P {Always()}? | P {Never()}? 'e' 'x' '?' S ':' S ;
P : R {Always()}? ;
R : id {Always()}? | id {Never()}? 'i' S ;
id := "[a-z]+" ;
'@
$options=[Janglim.FrontEnd.Grammars.Ebnf.EbnfReaderOptions]::new()
[void]$options.RegisterPredicate('Always',[System.Func[Janglim.FrontEnd.RegularGrammar.PredicateContext,bool]]{ param($context) return $true })
[void]$options.RegisterPredicate('Never',[System.Func[Janglim.FrontEnd.RegularGrammar.PredicateContext,bool]]{ param($context) return $false })
$read=[Janglim.FrontEnd.Grammars.Ebnf.EbnfGrammarReader]::Read($grammar,$options)
$parser=[Janglim.FrontEnd.Parsers.LR.LALRParser]::new($read.Grammar,$false)
$canonical=$parser.Canonical
$flags=[System.Reflection.BindingFlags]'Instance,NonPublic'
$predMethod=$canonical.GetType().GetMethod('PredEx',$flags)
$getReached=$canonical.GetType().GetMethod('GetStateSetHasReachedHandle',$flags)
$ff=$canonical.GetType().GetField('_ffAnalyzer',$flags).GetValue($canonical)
$virtualStart=$canonical.GetType().GetField('virtualStartSymbol',$flags).GetValue($canonical)
$eof=[Janglim.FrontEnd.RegularGrammar.EndMarker]::new()
$entries=[System.Collections.Generic.List[object]]::new()
foreach($state in $getReached.Invoke($canonical,@())) {
    foreach($item in $state.ReachedHandleItem) {
        $entries.Add([pscustomobject]@{State=$state;Item=$item;Order=$entries.Count})
    }
}
$cache=@{}
function ReplayLookAhead($state,$item) {
    $key=([string]$state.StateNumber)+':'+([string]$item.GetHashCode())
    if ($cache.ContainsKey($key)) { return ,$cache[$key] }
    $cache[$key]=[Janglim.FrontEnd.RegularGrammar.TerminalSet]::new()
    $result=[Janglim.FrontEnd.RegularGrammar.TerminalSet]::new()
    $target=$item.SingleNT.ToNonTerminal()
    if ([Janglim.FrontEnd.RegularGrammar.Symbol]::op_Equality($target,$virtualStart)) {
        [void]$result.Add($eof)
        $cache[$key]=$result
        return ,$result
    }
    $predecessors=$predMethod.Invoke($canonical,[object[]]@($state,$item))
    foreach($predecessor in $predecessors) { foreach($parentItem in $predecessor) {
        if (-not [Janglim.FrontEnd.RegularGrammar.Symbol]::op_Equality($parentItem.MarkSymbol,$target)) { continue }
        $alpha=$ff.First($parentItem.SymbolListAfterMarkSymbol)
        if($alpha.Contains([Janglim.FrontEnd.RegularGrammar.Epsilon]::new()) -or $alpha.Count -eq 0) {
            $dependencyKey=([string]$predecessor.StateNumber)+':'+([string]$parentItem.GetHashCode())
            if($cache.ContainsKey($dependencyKey)) { $la=$cache[$dependencyKey] }
            else { $la=ReplayLookAhead $predecessor $parentItem }
            [void]$result.UnionWith($alpha.RingSum($la))
        } else { [void]$result.UnionWith($alpha) }
    } }
    $cache[$key]=$result
    return ,$result
}
$targetItem=$canonical.IndexStateDic[6] | Where-Object { $_.SingleNT.Predicate.Name -eq 'Always' }
$targetKey='6:'+([string]$targetItem.GetHashCode())
$orders=[System.Collections.Generic.List[object]]::new()
$orders.Add([pscustomobject]@{Name='original';Entries=$entries.ToArray()})
$reverse=$entries.ToArray();[array]::Reverse($reverse)
$orders.Add([pscustomobject]@{Name='reverse';Entries=$reverse})
$ascending=@($entries.ToArray() | Sort-Object @{Expression={$_.State.StateNumber}},@{Expression={$_.Item.GetHashCode()}})
$orders.Add([pscustomobject]@{Name='state-ascending';Entries=$ascending})
$targetFirst=@($entries.ToArray() | Sort-Object @{Expression={if($_.State.StateNumber -eq 6){0}else{1}}},Order)
$orders.Add([pscustomobject]@{Name='I6-first';Entries=$targetFirst})
for($seed=1;$seed -le 10;$seed++) {
    $random=[System.Random]::new($seed)
    $shuffled=@($entries.ToArray() | ForEach-Object { [pscustomobject]@{Entry=$_;Random=$random.Next()} } | Sort-Object Random | ForEach-Object {$_.Entry})
    $orders.Add([pscustomobject]@{Name=('random-'+$seed);Entries=$shuffled})
}
$rows=[System.Collections.Generic.List[object]]::new()
foreach($order in $orders) {
    $cache=@{}
    foreach($entry in $order.Entries) { $null=ReplayLookAhead $entry.State $entry.Item }
    $differences=0
    foreach($key in $canonical.LookAheadTable.Keys) {
        $modelKey=([string]$key.Item1)+':'+([string]$key.Item2.GetHashCode())
        if(-not $cache.ContainsKey($modelKey) -or -not $cache[$modelKey].SetEquals($canonical.LookAheadTable[$key])) { $differences++ }
    }
    $rows.Add([pscustomobject]@{Order=$order.Name;TargetI6HasEof=$cache[$targetKey].Contains($eof);TargetI6Set=$cache[$targetKey].ToString();DifferentCacheEntriesFromOriginal=$differences})
}
$rows | ConvertTo-Json

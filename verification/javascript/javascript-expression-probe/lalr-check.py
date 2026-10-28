from pathlib import Path
from collections import defaultdict, deque
import re

text = Path('grammars/javascript/ecmascript-2026-expressions.ebnf').read_text(encoding='utf-8')
text = '\n'.join(line.split('#', 1)[0] for line in text.splitlines())
rules = []
for rule in text.split(';'):
    if not rule.strip() or ':=' in rule: continue
    lhs, rhs = rule.split(':', 1)
    lhs = lhs.strip()
    tokens = re.findall(r"'[^']*'|\{NoLineBreak\(\)\}\?|[A-Za-z][A-Za-z0-9]*|\|", rhs)
    alts=[[]]
    for tok in tokens:
        if tok=='|': alts.append([])
        elif tok=='{NoLineBreak()}?': pass
        else: alts[-1].append(tok)
    rules.extend((lhs,tuple(alt)) for alt in alts)
rules.insert(0,('$start',('Expression',)))
nts={lhs for lhs,_ in rules}
by_lhs=defaultdict(list)
for idx,(lhs,rhs) in enumerate(rules): by_lhs[lhs].append(idx)
first=defaultdict(set); nullable=set()
changed=True
while changed:
    changed=False
    for lhs,rhs in rules:
        before=len(first[lhs])
        for s in rhs:
            first[lhs].update(first[s] if s in nts else {s})
            if s not in nullable: break
        else:
            if lhs not in nullable: nullable.add(lhs); changed=True
        changed |= before!=len(first[lhs])
def closure(items):
    result=set(items); queue=list(items)
    while queue:
        p,d=queue.pop(); rhs=rules[p][1]
        if d<len(rhs) and rhs[d] in nts:
            for q in by_lhs[rhs[d]]:
                item=(q,0)
                if item not in result: result.add(item); queue.append(item)
    return frozenset(result)
states=[closure({(0,0)})]; index={states[0]:0}; transitions={}
queue=deque([0])
while queue:
    i=queue.popleft(); groups=defaultdict(set)
    for p,d in states[i]:
        rhs=rules[p][1]
        if d<len(rhs): groups[rhs[d]].add((p,d+1))
    for sym,items in groups.items():
        target=closure(items)
        if target not in index: index[target]=len(states); states.append(target); queue.append(index[target])
        transitions[i,sym]=index[target]
las=[{item:set() for item in st} for st in states]
las[0][(0,0)].add('$')
changed=True
rounds=0
while changed:
    changed=False; rounds+=1
    for i,st in enumerate(states):
        for p,d in st:
            la=las[i][(p,d)]
            if not la: continue
            rhs=rules[p][1]
            if d>=len(rhs): continue
            sym=rhs[d]
            dest=las[transitions[i,sym]][p,d+1]
            prev=len(dest); dest.update(la); changed |= prev!=len(dest)
            if sym not in nts: continue
            look=set()
            for s in rhs[d+1:]:
                look.update(first[s] if s in nts else {s})
                if s not in nullable: break
            else: look.update(la)
            for q in by_lhs[sym]:
                dest=las[i][q,0]; prev=len(dest); dest.update(look); changed |= prev!=len(dest)
actions=defaultdict(set)
for (state,sym),dest in transitions.items():
    if sym not in nts: actions[state,sym].add(('shift',dest))
for i,st in enumerate(states):
    for p,d in st:
        if d==len(rules[p][1]):
            for la in las[i][p,d]: actions[i,la].add(('accept',0) if p==0 else ('reduce',p))
conflicts={key:val for key,val in actions.items() if len(val)>1}
print('productions',len(rules),'LR0 states',len(states),'lookahead propagation rounds',rounds,'LALR conflicts',len(conflicts))
for key,val in conflicts.items():
    print(key, val)
    for action,p in val:
        if action=='reduce': print('  reduce',rules[p])
for input in ['a', 'a + a * a', 'a ** a ** a', "'new' a '(' ')' '(' ')'", "'[' ',' a ',' ',' ']'", "'{' a ':' a ',' '}'"]:
    pass

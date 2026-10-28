from pathlib import Path
import json

repo = Path.cwd()
output = Path('C:/Users/AhjLv/.codex/visualizations/2026/10/10/01a1259b-dbb8-7e10-9e0a-4959920bf8da/lookahead-worklist-code.html')
source_lines = (repo / 'src/FrontEnd/Parse.FrontEnd.Parsers/Collections/CanonicalRelation.cs').read_text(encoding='utf-8-sig').splitlines()
trace = json.loads((repo / 'artifacts/typescript/lookahead-fix/trace-probe/trace.json').read_text(encoding='utf-8-sig'))
again = json.loads((repo / 'artifacts/typescript/lookahead-fix/trace-probe/trace3.json').read_text(encoding='utf-8-sig'))
rounds = {r['round']: r for r in trace['rounds']}
late = {r['round']: r for r in again['rounds']}
START = 'I0 · Accept → • S'
S0 = 'I0 · S → • G C'
S2 = 'I2 · S → G • C'
C2 = 'I2 · C → • R'
R2 = "I2 · R → • 'a'"
R5 = "I5 · R → 'a' •"
G0 = 'I0 · G → •'
MID = "I3 · A → 'a' • 'z'"
DONE = "I6 · A → 'a' 'z' •"
steps = []

def add(phase, title, detail, lines, diagram, queue=None, facts=(), active=None):
    step = dict(phase=phase, title=title, detail=detail, lines=lines, diagram=diagram, facts=list(facts))
    if queue is not None: step['queue'] = queue
    if active is not None: step['active'] = active
    steps.append(step)

def table(*rows):
    return dict(kind='table', rows=[dict(item=item, set=value, updated=active) for item, value, active in rows])

def single(item, value, label='', updated=False):
    return dict(kind='single', item=item, set=value, label=label, updated=updated)

def flow(r, u, edge, edge2=None, after=True):
    update = r['updates'][u]
    answer = dict(kind='flow', **{'from': r['source']}, fromSet=r['lookahead'], to=update['target'],
                  toSet=update['after' if after else 'before'], updated=after and update['changed'], edge=edge)
    if edge2: answer['edge2'] = edge2
    return answer

add('1 · 문법과 저장 단위', '이전 설명과 같은 문법을 사용합니다.',
    '입력 a를 정상적으로 처리할 파싱 테이블을 만드는 과정을 보겠습니다. epsilon은 빈 생성 규칙입니다.', [],
    dict(kind='grammar', lines=["S : G C ;", "C : R | 'e' S ;", "R : 'a' | 'b' S ;", 'G : epsilon ;']))
add('1 · 문법과 저장 단위', '한 상태 안에도 LR 항목이 여러 개 있습니다.',
    '따라서 룩어헤드는 상태 번호와 LR 항목을 함께 묶은 키로 저장합니다.', [],
    table((S2, '{}', False), (C2, '{}', False)))
add('1 · 문법과 저장 단위', '각 키의 값은 하나의 TerminalSet 집합입니다.',
    '예를 들어 C → • R 항목의 룩어헤드는 아래 키로 찾습니다.', [295, 296],
    table((C2, '{}', False)), facts=['키: (I2, C → • R)', '값: TerminalSet 객체'])
add('1 · 문법과 저장 단위', '시작 규칙 전체의 완료를 확인할 규칙을 하나 추가합니다.',
    '파서는 시작 규칙 S를 감싸는 Accept → S를 추가합니다. 이 규칙에서 출발하는 항목이 확장 시작 항목입니다.', [],
    dict(kind='grammar', lines=['Accept → S', '', 'I0 · Accept → • S']))
add('2 · 집합과 전달 관계 준비', '먼저 저장소를 비우고, 전달 관계를 준비합니다.',
    '각 항목이 정보를 넘길 대상 목록이 필요합니다. 코드에서는 이 목록을 dependencies라고 부릅니다.', [242, 243],
    dict(kind='grammar', lines=['LookAheadTable: 항목별 룩어헤드', 'dependencies: 항목별 전달 대상']))
add('2 · 집합과 전달 관계 준비', '시작 항목의 빈 집합을 먼저 만듭니다.',
    'BuildLookAheadDependencies의 첫 반복에서 확장 시작 항목에 TerminalSet을 준비합니다.', [295, 296],
    table((START, '{}', False)))
add('2 · 집합과 전달 관계 준비', '확장 시작 항목에 EOF를 넣습니다.',
    '시작 규칙 전체를 읽고 나면 입력이 끝나야 하므로, 이 항목의 룩어헤드에 EOF를 넣습니다.', [298, 299],
    table((START, '{EOF}', True)))
add('2 · 집합과 전달 관계 준비', '나머지 항목에도 각각 빈 집합을 준비합니다.',
    '그림에는 EOF를 전달받을 주요 항목을 골라 표시했습니다. 각 항목의 집합은 아직 비어 있습니다.', [291, 293, 295, 296],
    table((START, '{EOF}', False), (S2, '{}', False), (C2, '{}', False), (R5, '{}', False)))
add('2 · 집합과 전달 관계 준비', '이제 정보를 넘길 두 항목을 연결합니다.',
    'S → G • C는 C 규칙을 이어 읽는 항목이므로, 같은 상태의 C → • R에 정보를 넘깁니다.', [338, 339],
    dict(kind='flow', **{'from': S2}, fromSet='{}', to=C2, toSet='{}',
         edge='dependencies에 전달 관계 등록'))
add('2 · 집합과 전달 관계 준비', '뒤쪽에 다른 기호가 없으면 부모의 집합을 물려줍니다.',
    'S → G • C에서 C 뒤가 비어 있으므로, 이 전달 관계는 Inherit = true로 기록합니다.', [332, 339, 368],
    dict(kind='flow', **{'from': S2}, fromSet='{}', to=C2, toSet='{}',
         edge='Inherit = true'))
add('2 · 집합과 전달 관계 준비', 'G 뒤에는 C가 있으므로, C가 시작할 토큰을 구합니다.',
    'C는 a·b·e로 시작합니다. 이 토큰들을 직접 넘길 First에 기록하고, 부모 집합은 물려주지 않습니다.', [332, 339],
    dict(kind='flow', **{'from': S0}, fromSet='{}', to=G0, toSet='{}',
         edge="First = {'a', 'b', 'e'}", edge2='Inherit = false'))
add('3 · 큐에서 항목을 꺼내 전파', '전달할 작업을 순서대로 보관할 큐를 만듭니다.',
    'pending은 앞으로 다른 항목에 룩어헤드를 전달할 항목들을 보관합니다.', [244],
    single(START, '{EOF}', '첫 전달 작업의 후보'), queue=[])
add('3 · 큐에서 항목을 꺼내 전파', '큐에 이미 들어간 항목을 별도로 표시합니다.',
    '같은 항목을 큐에 중복 등록하지 않도록, 등록 여부를 queued 집합에 보관합니다.', [245],
    single(START, '{EOF}'), queue=[], facts=['queued = { }'])
add('3 · 큐에서 항목을 꺼내 전파', '처음에는 EOF를 가진 시작 항목만 큐에 넣습니다.',
    '집합이 비어 있는 항목은 건너뜁니다. 시작 항목은 pending과 queued에 함께 등록합니다.', [247, 249, 250, 251],
    single(START, '{EOF}', '초기 등록', True), queue=[START], facts=['queued에 시작 항목 등록'])
add('3 · 큐에서 항목을 꺼내 전파', '큐에서 꺼낸 항목을 source에 담습니다.',
    'source는 이번 전달 작업을 식별하는 상태 번호와 LR 항목의 묶음입니다. 집합은 이 키로 저장소에서 찾습니다.', [254, 256],
    single(START, '{EOF}', 'source'), queue=[])
add('3 · 큐에서 항목을 꺼내 전파', '꺼낸 항목의 등록 표시를 지웁니다.',
    '나중에 이 항목의 집합이 더 커지면 큐에 다시 넣을 수 있도록 queued에서 제거합니다.', [258],
    single(START, '{EOF}', 'source'), queue=[], facts=['queued에서 시작 항목 제거'])
add('3 · 큐에서 항목을 꺼내 전파', '시작 항목의 EOF가 첫 S 항목으로 전달됩니다.',
    '전달받은 집합이 커진 항목들을 큐에 넣습니다. 아래는 첫 source를 처리한 뒤의 실제 큐입니다.', [260, 269, 270, 272, 273],
    flow(rounds[1], 1, 'EOF 전달'), queue=rounds[1]['queue'])
add('3 · 큐에서 항목을 꺼내 전파', '3번째 source는 S → • G C입니다.',
    '그 전에 큐에 있던 완료된 Accept 항목을 처리했습니다. 이제 S 항목의 EOF를 점이 이동한 항목으로 넘깁니다.', [256, 270, 272, 273],
    flow(rounds[3], 0, 'Goto 뒤 항목에도 EOF 전달'), queue=[S2])
add('3 · 큐에서 항목을 꺼내 전파', '같은 source에서 G 항목에는 First 토큰을 전달합니다.',
    '이 관계는 Inherit = false이므로, G 항목에는 a·b·e를 넣습니다.', [269, 270, 272, 273],
    flow(rounds[3], 1, "First = {'a', 'b', 'e'}", 'Inherit = false'), queue=rounds[3]['queue'])
add('4 · 이전에 누락된 C 항목 갱신', '4번째 source는 먼저 S 규칙의 완료 항목에 EOF를 전달합니다.',
    'S → G • C의 첫 전달 대상은 S → G C •입니다. 이 완료 항목을 먼저 큐에 등록합니다.', [256, 258, 270, 272, 273],
    flow(rounds[4], 0, 'S 규칙의 완료 항목으로 EOF 전달'), queue=[G0, 'I3 · S → G C •'])
add('4 · 이전에 누락된 C 항목 갱신', '같은 source에서 다음 전달 대상 C → • R을 선택합니다.',
    'S 규칙의 완료 항목을 등록했으므로, 이제 dependencies에 기록된 다음 대상 C 항목을 처리합니다.', [260, 262],
    flow(rounds[4], 1, '다음 전달 대상 선택', after=False), queue=[G0, 'I3 · S → G C •'])
add('4 · 이전에 누락된 C 항목 갱신', '받을 집합을 targetSet 참조에 담습니다.',
    'targetSet과 LookAheadTable의 해당 값은 같은 TerminalSet 객체를 가리킵니다.', [263],
    dict(kind='alias', set='{}'), queue=[G0, 'I3 · S → G C •'])
add('4 · 이전에 누락된 C 항목 갱신', '추가하기 전 집합 크기를 before에 기록합니다.',
    'C 항목의 집합은 아직 비어 있으므로 before는 0입니다.', [264],
    dict(kind='alias', set='{}'), queue=[G0, 'I3 · S → G C •'], facts=['before = 0'])
add('4 · 이전에 누락된 C 항목 갱신', 'targetSet에 EOF를 합치면 저장소의 집합도 바뀝니다.',
    'Inherit = true이므로 source 집합의 EOF를 합칩니다. 같은 객체를 사용하므로 저장소에도 바로 반영됩니다.', [269, 270],
    dict(kind='alias', set='{EOF}', updated=True), queue=[G0, 'I3 · S → G C •'], facts=['before = 0', 'targetSet.Count = 1'])
add('4 · 이전에 누락된 C 항목 갱신', '집합이 커졌고 대기 중인 항목도 아니므로, C를 큐에 넣습니다.',
    'Count는 0에서 1이 됐고 queued.Add(target)도 true입니다. 이 두 조건을 만족하면 전달 작업을 등록합니다.', [272, 273],
    single(C2, '{EOF}', '새로 등록한 전달 작업', True),
    queue=[G0, 'I3 · S → G C •', C2], facts=['1 > 0: true', 'queued.Add(target): true'])
add('4 · 이전에 누락된 C 항목 갱신', 'C의 다른 생성 규칙도 EOF를 받아 큐에 등록됩니다.',
    '같은 source의 나머지 대상 C → • e S도 처리합니다. 그러면 4번째 source의 전달 작업이 끝납니다.', [260, 270, 272, 273],
    flow(rounds[4], 2, 'C의 다른 생성 규칙으로 EOF 전달'), queue=rounds[4]['queue'])
add('4 · 이전에 누락된 C 항목 갱신', '7번째 source가 된 C 항목은 EOF를 R 항목에 전달합니다.',
    '그 사이 앞선 대기 항목들을 처리했습니다. 이제 C가 저장소에서 자신의 EOF를 읽어 다음 항목으로 넘깁니다.', [256, 258, 270, 272, 273],
    flow(rounds[7], 1, 'C 항목에서 R 항목으로 EOF 전달'), queue=rounds[7]['queue'])
add('4 · 이전에 누락된 C 항목 갱신', '10번째 source가 된 R 항목은 완료 항목으로 EOF를 넘깁니다.',
    'a를 읽은 뒤의 R → a • 항목도 EOF를 받습니다. 이제 이 항목의 Reduce를 EOF 열에 넣을 수 있습니다.', [256, 270, 272, 273],
    flow(rounds[10], 0, 'Goto 뒤 완료 항목으로 EOF 전달'), queue=rounds[10]['queue'])
add('4 · 이전에 누락된 C 항목 갱신', '같은 EOF가 다시 와도 집합 크기는 늘어나지 않습니다.',
    '16번째 source가 이미 EOF를 가진 S 항목에 EOF를 다시 전달합니다. 크기가 그대로이므로 이 대상을 큐에 추가하지 않습니다.', [270, 272, 273],
    flow(rounds[16], 0, 'EOF를 다시 합쳐도 {EOF}', after=True),
    queue=rounds[15]['queue'][1:], facts=['before = 1', 'targetSet.Count = 1', '1 > 1: false'], active=[270, 272])
add('4 · 이전에 누락된 C 항목 갱신', '큐에 남은 항목까지 처리하면 계산이 끝납니다.',
    '이 예제에서는 20개 항목을 처리한 뒤 pending이 비어 while 반복을 끝냅니다.', [254, 275, 276],
    table((C2, '{EOF}', False), (R5, '{EOF}', True)), queue=[], facts=['pending.Count = 0'])
add('4 · 이전에 누락된 C 항목 갱신', '완성된 룩어헤드로 EOF 열의 Reduce를 만들 수 있습니다.',
    '입력 a를 읽고 I5에 도착하면, EOF를 보고 R → a를 Reduce한 뒤 정상적으로 파싱을 마칩니다.', [],
    dict(kind='actions', item=R5, set='{EOF}', action=['ACTION[I5, EOF]', '= Reduce(R → a)'], result='실제 입력 a → 파싱 성공'))

add('5 · 이미 처리한 항목에 새 토큰이 도착', '이번에는 두 경로가 같은 A 규칙을 사용하는 문법을 보겠습니다.',
    '큐에 다시 등록하는 이유를 확인하기 위해, A 뒤의 토큰이 서로 다른 문법을 사용합니다.', [],
    dict(kind='grammar', lines=["S : A '!'", "  | 'b' 'c' 'd' A '?' ;", "A : 'a' 'z' ;"]))
add('5 · 이미 처리한 항목에 새 토큰이 도착', '첫 경로에서는 A 뒤에 !가 옵니다.',
    '이 정보가 전달된 뒤, 6번째 source는 중간 A 항목에도 !를 넘깁니다.', [270, 272, 273],
    flow(late[6], 0, "'!' 전달"), queue=late[6]['queue'])
add('5 · 이미 처리한 항목에 새 토큰이 도착', '9번째 처리에서 중간 A 항목이 !를 완료 항목으로 넘깁니다.',
    '이 시점에는 중간 항목과 완료 항목이 모두 !를 알고 있습니다.', [256, 258, 270, 272, 273],
    flow(late[9], 0, "'!' 전달"), queue=late[9]['queue'])
add('5 · 이미 처리한 항목에 새 토큰이 도착', '이후 두 번째 경로에서 ?가 도착합니다.',
    '14번째 source가 같은 중간 A 항목에 ?를 추가합니다. 이미 한 번 처리한 항목의 집합이 다시 커집니다.', [270],
    flow(late[14], 0, "새 토큰 '?' 전달"), queue=late[14]['queueBefore'][1:], facts=['추가 전: ! 하나', '추가 후: !와 ? 두 개'])
add('5 · 이미 처리한 항목에 새 토큰이 도착', '이미 처리한 중간 항목을 큐에 다시 넣습니다.',
    '첫 처리에서 queued 표시를 지웠으므로 다시 등록할 수 있습니다. 2 > 1도 참이므로 pending에 추가합니다.', [272, 273],
    single(MID, "{'!', '?'}", '다시 등록한 전달 작업', True), queue=late[14]['queue'], facts=['before = 1', 'targetSet.Count = 2', 'queued.Add(target): true'])
add('5 · 이미 처리한 항목에 새 토큰이 도착', '16번째 처리에서 중간 항목이 새 정보를 완료 항목까지 넘깁니다.',
    '앞에 있던 완료 S 항목을 처리한 뒤, 중간 A 항목을 다시 꺼냅니다. 이 항목이 완료 A 항목에도 ?를 넘깁니다.', [256, 258, 270, 272, 273],
    flow(late[16], 0, "이번에는 '?'도 함께 전달"), queue=late[16]['queue'])
add('5 · 이미 처리한 항목에 새 토큰이 도착', '완료 항목은 !와 ?에서 모두 Reduce할 수 있습니다.',
    '17번째 처리까지 끝나면 큐가 비고, 실제 입력 az!와 bcdaz?를 모두 정상적으로 파싱합니다.', [254, 275, 276],
    dict(kind='actions', item=DONE, set="{'!', '?'}", action=['ACTION[I6, !] / ACTION[I6, ?]', '= Reduce(A → a z)'], result='az! · bcdaz? → 모두 파싱 성공'), queue=[])

used_lines = sorted({line for step in steps for line in step['lines']})
source = {line: source_lines[line - 1].strip() for line in used_lines}
for step in steps:
    assert all(line in source and source[line] for line in step['lines'])
    assert step['diagram']['kind'] in ('grammar', 'table', 'flow', 'alias', 'single', 'actions')

html = output.read_text(encoding='utf-8')
start = html.index('    const source = ')
end = html.index("    const ns = ", start)
html = html[:start] + '    const source = ' + json.dumps(source, ensure_ascii=False) + ';\n' + \
    '    const steps = ' + json.dumps(steps, ensure_ascii=False) + ';\n' + html[end:]
output.write_text(html, encoding='utf-8')
print(json.dumps({'path':str(output), 'steps':len(steps), 'sourceLines':len(source), 'bytes':output.stat().st_size}, ensure_ascii=True))

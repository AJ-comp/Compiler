"""Draw every Entry and dependency in the exact trace3 example.

The manifest comes from the real worklist trace; layout is explicit to keep all
15 references visible without crossings.  No dependency is inferred from a
picture or collapsed into a synthetic edge.
"""
from pathlib import Path
from html import escape
import json

OUT = Path('docs/images')
TRACE = Path('artifacts/typescript/lookahead-fix/trace-probe/trace3.json')

NODES = [
    ('accept0', 0, 'Accept → • S', 50, 190),
    ('s0a', 0, "S → • A 'e'", 50, 340),
    ('a0', 0, "A → • 'a' 'z'", 50, 490),
    ('s0b', 0, "S → • 'b' 'c' 'd' A 'f'", 50, 740),
    ('accept1', 1, 'Accept → S •', 550, 190),
    ('s2', 2, "S → A • 'e'", 550, 340),
    ('s5', 5, "S → A 'e' •", 1050, 340),
    ('a3', 3, "A → 'a' • 'z'", 550, 490),
    ('a6', 6, "A → 'a' 'z' •", 1050, 490),
    ('s4', 4, "S → 'b' • 'c' 'd' A 'f'", 300, 740),
    ('s7', 7, "S → 'b' 'c' • 'd' A 'f'", 550, 740),
    ('s8', 8, "S → 'b' 'c' 'd' • A 'f'", 800, 740),
    ('a8', 8, "A → • 'a' 'z'", 800, 610),
    ('s9', 9, "S → 'b' 'c' 'd' A • 'f'", 1050, 740),
    ('s10', 10, "S → 'b' 'c' 'd' A 'f' •", 1300, 740),
]
NODE_MAP = {n[0]: n for n in NODES}
KEYS = {f'I{state} · {item}': key for key, state, item, _, _ in NODES}

# Keyed by source/target IDs.  Each path begins/ends on the corresponding
# Entry boundary, including the two separate A starts that share I3.
PATHS = {
    ('accept0', 'accept1'): ('M 270 228 H 550', 'S', 410, 214),
    ('accept0', 's0a'): ('M 50 217 H 36 V 378 H 50', None, 70, 299),
    ('accept0', 's0b'): ('M 50 247 H 22 V 778 H 50', None, 70, 692),
    ('s0a', 's2'): ('M 270 378 H 550', 'A', 410, 364),
    ('s0a', 'a0'): ('M 250 416 V 490', None, 68, 448),
    ('s0b', 's4'): ('M 270 778 H 300', 'b', 285, 760),
    ('s2', 's5'): ('M 770 378 H 1050', 'e', 910, 364),
    ('a0', 'a3'): ('M 270 528 H 550', 'a', 410, 514),
    ('s4', 's7'): ('M 520 778 H 550', 'c', 535, 760),
    ('a3', 'a6'): ('M 770 528 H 1050', 'z', 910, 514),
    ('s7', 's8'): ('M 770 778 H 800', 'd', 785, 760),
    ('s8', 's9'): ('M 1020 778 H 1050', 'A', 1035, 760),
    ('s8', 'a8'): ('M 1000 740 V 686', None, 818, 710),
    ('s9', 's10'): ('M 1270 778 H 1300', 'f', 1285, 760),
    ('a8', 'a3'): ('M 800 648 H 660 V 566', 'a', 734, 634),
}

STYLE = '''
:root{--bg:#fff;--ink:#1b2b3f;--muted:#506278;--line:#ccd6e3;
 --state:#f7f9fc;--node:#fff;--blue:#285ab7;--orange:#a85c16;
 --highlight:#edf4ff;--seed:#e8f5ef;--seedink:#226346}
@media(prefers-color-scheme:dark){:root{--bg:#18222f;--ink:#edf3fb;
 --muted:#bccbdd;--line:#4b607b;--state:#202d3e;--node:#243449;
 --blue:#a3c4ff;--orange:#f2ba7f;--highlight:#283f61;
 --seed:#244a3c;--seedink:#b7ead1}}
text{font-family:'Malgun Gothic','Noto Sans KR',sans-serif;fill:var(--ink)}
.mono{font-family:Consolas,'Malgun Gothic',monospace;font-variant-ligatures:none}
.muted{fill:var(--muted)}.blue{fill:var(--blue)}.orange{fill:var(--orange)}
.goto{stroke:var(--blue);stroke-width:2.2;fill:none;stroke-linejoin:round;stroke-linecap:round}
.closure{stroke:var(--orange);stroke-width:2.1;fill:none;stroke-dasharray:6 5;stroke-linejoin:round;stroke-linecap:round}
.state{fill:var(--state);stroke:var(--line);stroke-width:1.25}
.entry{fill:var(--node);stroke:var(--line);stroke-width:1.5}
.entry.shared{fill:var(--highlight);stroke:var(--blue)}
'''


def readable_trace(value):
    """Keep the diagnostic trace intact and use e/f terminals in the document."""
    if isinstance(value, str):
        return value.replace(chr(33), 'e').replace(chr(63), 'f')
    if isinstance(value, list):
        return [readable_trace(item) for item in value]
    if isinstance(value, dict):
        return {key: readable_trace(item) for key, item in value.items()}
    return value


def build():
    trace = readable_trace(json.loads(TRACE.read_text(encoding='utf-8')))
    records = {}
    for round_ in trace['rounds']:
        source = KEYS[round_['source']]
        for update in round_['updates']:
            target = KEYS[update['target']]
            value = {'source': source, 'target': target, 'first': update['first'],
                     'inherit': update['inherit'],
                     'kind': 'goto' if update['first'] is None else 'closure'}
            previous = records.setdefault((source, target), value)
            assert previous == value
    assert set(records) == set(PATHS)
    assert len(NODES) == 15 and len(records) == 15
    assert len(set(n[1] for n in NODES)) == 11

    parts = [
        '<svg xmlns="http://www.w3.org/2000/svg" width="1540" height="1040" viewBox="0 0 1540 1040" role="img" aria-labelledby="title desc">',
        '<title id="title">BuildLookAheadEntries가 준비하는 전체 전달 관계</title>',
        '<desc id="desc">예제 문법의 모든 상태 11개, LookAheadEntry 15개, Dependencies 연결 15개를 빠짐없이 표시했습니다. 파란 실선은 Goto로 이동한 항목에 집합을 상속하는 연결 11개이며 First null, Inherit true입니다. 주황 점선은 Closure 전달 4개이며 각각 First와 Inherit 값을 표기했습니다. 초기 Accept Entry에서 두 S 시작 Entry로, 첫 S 시작 Entry에서 초기 A 시작 Entry로, I8의 S 항목에서 같은 상태 A 시작 Entry로 Closure 전달이 연결됩니다. 서로 다른 두 A 시작 Entry의 a 전이는 같은 I3 Entry로 연결되고, I3은 I6 완료 Entry로 연결됩니다. 그림은 큐 처리 전이므로 초기 Accept Entry만 EOF를 가지고 나머지는 빈 집합입니다.</desc>',
        f'<style>{STYLE}</style>',
        '<defs><marker id="goto-head" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="7" markerHeight="7" orient="auto-start-reverse"><path d="M 1 1 L 9 5 L 1 9" fill="none" stroke="var(--blue)" stroke-width="1.6"/></marker><marker id="closure-head" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="7" markerHeight="7" orient="auto-start-reverse"><path d="M 1 1 L 9 5 L 1 9" fill="none" stroke="var(--orange)" stroke-width="1.6"/></marker></defs>',
        '<rect width="1540" height="1040" fill="var(--bg)"/>',
    ]

    def text(x, y, value, size=18, cls='', weight=400, anchor='start'):
        parts.append(f'<text x="{x}" y="{y}" font-size="{size}" class="{cls}" font-weight="{weight}" text-anchor="{anchor}">{escape(value)}</text>')

    text(24, 42, '전체 전달 관계', 28, weight=700)
    text(245, 42, 'BuildLookAheadEntries()', 24, 'mono muted')
    text(1516, 40, '11개 상태  ·  15개 Entry  ·  15개 연결', 18, 'muted', anchor='end')
    text(24, 82, '상자 하나 = LookAheadEntry   /   화살표 하나 = 출발 Entry의 Dependencies[i].Target 참조', 18)
    text(24, 115, "S : A 'e' | 'b' 'c' 'd' A 'f' ;     A : 'a' 'z' ;", 20, 'mono muted')

    # All states, including the two multi-Entry states, are shown as groups.
    states = [
        (0, 10, 145, 270, 694),
        (1, 540, 150, 240, 130),
        (2, 540, 300, 240, 130),
        (5, 1040, 300, 240, 130),
        (3, 540, 450, 240, 130),
        (6, 1040, 450, 240, 130),
        (4, 290, 700, 240, 139),
        (7, 540, 700, 240, 139),
        (8, 790, 568, 240, 271),
        (9, 1040, 700, 240, 139),
        (10, 1290, 700, 240, 139),
    ]
    for state, x, y, w, h in states:
        parts.append(f'<g data-state="I{state}"><rect x="{x}" y="{y}" width="{w}" height="{h}" rx="10" class="state"/></g>')
        text(x+14, y+24, f'I{state}' + (' · 초기 상태' if state==0 else ''), 18, 'mono', 700)

    for key, state, item, x, y in NODES:
        shared = key in ('a3', 'a6')
        parts.append(f'<g id="entry-{key}" data-entry="{key}" data-state="I{state}" data-lr-item="{escape(item, quote=True)}">')
        parts.append(f'<rect x="{x}" y="{y}" width="220" height="76" rx="7" class="entry{" shared" if shared else ""}"/>')
        text(x+12, y+19, 'LookAheadEntry', 14, 'mono muted')
        display = item.replace("'", '')
        text(x+12, y+50, display, 21, 'mono blue' if shared else 'mono', 600)
        parts.append('</g>')

    # Draw only in gaps/borders; no reference goes across another Entry's text.
    for key, edge in records.items():
        route, symbol, lx, ly = PATHS[key]
        kind = edge['kind']
        attrs = ' '.join(f'data-{k}="{escape(str(v), quote=True)}"' for k, v in edge.items())
        parts.append(f'<g {attrs}><path d="{route}" class="{kind}" marker-end="url(#{kind}-head)"/></g>')
        if symbol is not None:
            parts.append(f'<rect x="{lx-13}" y="{ly-20}" width="26" height="25" rx="4" fill="var(--bg)"/>')
            text(lx, ly, symbol, 21, 'mono blue', 600, 'middle')
        else:
            # Values belong to this exact dependency, not to the Target Entry.
            first = edge['first'].replace("'", '')
            text(lx, ly, f'First = {first}', 15, 'mono orange', 600)
            text(lx, ly+23, f'Inherit = {str(edge["inherit"]).lower()}', 15, 'mono orange')

    # A single seed is present when BuildLookAheadEntries returns.
    parts.append('<rect x="320" y="154" width="186" height="34" rx="6" fill="var(--seed)"/>')
    text(413, 177, 'LookAhead = {EOF}', 17, 'mono', 600, 'middle')
    parts.append('<path d="M 320 171 H 296 V 207 H 270" fill="none" stroke="var(--seedink)" stroke-width="1.5"/>')

    parts.append('<line x1="24" y1="872" x2="1516" y2="872" stroke="var(--line)"/>')
    parts.append('<path d="M 24 908 H 80" class="goto" marker-end="url(#goto-head)"/>')
    text(98, 914, 'Goto(기호) · 11개', 18, 'blue', 600)
    text(320, 914, 'First = null,  Inherit = true', 18, 'mono')
    text(800, 914, '출발 Entry의 룩어헤드 집합을 Target에 합칩니다.', 18)
    parts.append('<path d="M 24 947 H 80" class="closure" marker-end="url(#closure-head)"/>')
    text(98, 953, 'Closure · 4개', 18, 'orange', 600)
    text(320, 953, '각 연결 옆에 First · Inherit 값을 표시했습니다.', 18)
    text(24, 997, '준비 직후: Accept 시작 Entry만 LookAhead = {EOF}, 나머지 14개는 {}. 모든 IsQueued = false.', 18)
    text(24, 1025, '화살표가 나가지 않는 완료 Entry 4개는 Dependencies = []입니다.', 16, 'muted')
    parts.append('</svg>')
    OUT.joinpath('lalr-lookahead-full-graph.svg').write_text('\n'.join(parts)+'\n', encoding='utf-8')
    manifest = {'states': 11, 'nodes': [dict(key=k, state=s, item=i, x=x, y=y) for k,s,i,x,y in NODES],
                'dependencies': list(records.values()), 'seed': ['accept0'], 'snapshot': 'BuildLookAheadEntries return, before queue'}
    Path(__file__).with_name('full-graph-manifest.json').write_text(json.dumps(manifest, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')


if __name__ == '__main__':
    build()

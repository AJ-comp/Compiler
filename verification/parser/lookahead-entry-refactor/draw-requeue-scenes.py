"""Static, source-accurate snapshots of the delayed lookahead propagation.

Snapshots correspond to trace3.json rounds 14--17.  The graph is fixed; only
LookAhead sets, IsQueued flags, source/target roles, and queue references change.
"""
from pathlib import Path
from html import escape
import importlib.util
import json

spec = importlib.util.spec_from_file_location(
    'lookahead_diagrams', Path(__file__).with_name('draw-lookahead-images.py'))
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)
Diagram = module.Diagram

SCENES = [
    dict(number='01', title='둘째 경로의 A Entry를 꺼냅니다.',
         subtitle='source = pending.Dequeue(); source.IsQueued = false;',
         mobile_title=['둘째 경로의 A Entry를', '큐에서 꺼냅니다.'],
         source=0, target=1, middle_two=False, complete_two=False,
         queued=[False, False, False], pending=['s'],
         description='둘째 경로의 A 시작 Entry를 꺼낸 직후입니다. 이 Entry의 LookAhead에는 f가 있고, 중간과 완료 Entry에는 아직 e만 있습니다. 세 Entry의 IsQueued는 false이며 pending에는 S 완료 Entry 참조 하나가 남아 있습니다.'),
    dict(number='02', title='f를 중간 Entry에 합칩니다.',
         subtitle='target.LookAhead.UnionWith(source.LookAhead);',
         mobile_title=['f를 중간 Entry의', 'LookAhead에 합칩니다.'],
         source=0, target=1, middle_two=True, complete_two=False,
         queued=[False, False, False], pending=['s'],
         description='둘째 경로 Entry의 f를 중간 Entry의 LookAhead에 합쳤습니다. 중간 집합의 크기는 1에서 2로 늘었지만 IsQueued는 아직 false입니다. 완료 Entry는 e만 가지고 있으며 pending에는 S 완료 Entry 참조 하나만 있습니다.'),
    dict(number='03', title='중간 Entry를 큐에 다시 넣습니다.',
         subtitle='target.IsQueued = true; pending.Enqueue(target);',
         mobile_title=['중간 Entry를', '큐에 다시 넣습니다.'],
         source=0, target=1, middle_two=True, complete_two=False,
         queued=[False, True, False], pending=['s', 'middle'],
         description='집합이 증가했고 큐 밖에 있던 중간 Entry의 IsQueued를 true로 바꾸고 pending 뒤에 등록했습니다. pending은 S 완료 Entry 참조, 중간 Entry 참조 순서입니다. 중간 Entry 참조는 위에 그려진 동일한 객체를 가리키며 완료 집합은 아직 e뿐입니다.'),
    dict(number='04', title='중간 Entry를 다시 꺼냅니다.',
         subtitle='source = pending.Dequeue(); source.IsQueued = false;',
         mobile_title=['중간 Entry를', '다시 꺼냅니다.'],
         source=1, target=2, middle_two=True, complete_two=False,
         queued=[False, False, False], pending=[],
         description='먼저 앞에 있던 S 완료 Entry를 처리한 다음 중간 Entry를 꺼낸 시점입니다. source는 중간 Entry, target은 완료 Entry이며 세 Entry의 IsQueued는 false입니다. pending은 비었지만 source의 전달 작업은 진행 중입니다.'),
    dict(number='05', title='완료 Entry도 갱신하고 등록합니다.',
         subtitle='집합을 합친 뒤, 같은 큐 등록 조건을 적용합니다.',
         mobile_title=['완료 Entry도 갱신하고', '큐에 등록합니다.'],
         source=1, target=2, middle_two=True, complete_two=True,
         queued=[False, False, True], pending=['complete'],
         description='중간 Entry의 집합을 완료 Entry에 합치면 완료 Entry에도 f가 추가됩니다. 같은 큐 등록 조건에 따라 완료 Entry의 IsQueued는 true이고 pending에는 이 완료 Entry를 가리키는 참조 하나가 있습니다. 중간 Entry는 현재 처리 중이므로 IsQueued가 false입니다.'),
    dict(number='06', title='완료 Entry까지 처리하면 끝납니다.',
         subtitle='완료 Entry의 Dependencies가 비어 있어 추가 전달은 없습니다.',
         mobile_title=['완료 Entry까지', '처리하면 끝납니다.'],
         source=2, target=None, middle_two=True, complete_two=True,
         queued=[False, False, False], pending=[],
         description='완료 Entry를 큐에서 꺼내 IsQueued를 false로 바꿨습니다. 이 Entry의 Dependencies는 빈 목록이어서 추가 전달은 없습니다. 현재 Entry 처리가 끝났고 pending도 비었으므로 계산이 종료됩니다. 완료 Entry의 LookAhead에는 e와 f가 모두 있습니다.'),
]

RULES = ['A → •a z', 'A → a•z', 'A → az•']
ROLES = ['둘째 경로의 A 항목', '중간 항목', '완료 항목']
QUEUE_LABELS = dict(s='S 완료 Entry 참조', middle='중간 Entry 참조', complete='완료 Entry 참조')
QUEUE_RULES = dict(s='S → b c d A f•', middle=RULES[1], complete=RULES[2])


def frame(d, x, y, w, h, label, focus=False, font=24):
    fill = 'var(--focus)' if focus else 'var(--card)'
    d.parts.append(f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="13" fill="{fill}" stroke="var(--line)" stroke-width="2"/>')
    gap = min(w-30, len(label)*font*.56+20)
    d.parts.append(f'<rect x="{x+14}" y="{y-18}" width="{gap}" height="34" fill="var(--canvas)"/>')
    d.text(x+24, y+8, label, font, 'code', 700)


def set_line(d, x, y, value, size=25):
    prefix = 'LookAhead = '
    if value == 'f':
        text = escape(prefix) + '<tspan class="new">{f}</tspan>'
    elif value == 'e,f':
        text = escape(prefix) + '{e<tspan class="new">, f</tspan>}'
    else:
        text = escape(prefix + '{e}')
    d.parts.append(f'<text x="{x}" y="{y}" class="code" font-size="{size}">{text}</text>')


def head(d, x, y, direction, cls=''):
    if direction == 'up':
        d.path(f'M {x-6} {y+9} L {x} {y} L {x+6} {y+9}', cls)
    elif direction == 'right':
        d.path(f'M {x-9} {y-6} L {x} {y} L {x-9} {y+6}', cls)
    else:
        d.path(f'M {x+9} {y-6} L {x} {y} L {x+9} {y+6}', cls)


def values(scene):
    return ['f', 'e,f' if scene['middle_two'] else 'e',
            'e,f' if scene['complete_two'] else 'e']


def entry(d, scene, i, x, y, w, mobile=False):
    h = 298 if mobile else 390
    frame(d, x, y, w, h, 'LookAheadEntry', i == 1, 25 if mobile else 24)
    d.text(x+20, y+59, RULES[i], 33 if mobile else 32, 'code', 700)
    set_line(d, x+20, y+109, values(scene)[i], 26 if mobile else 25)
    d.text(x+20, y+154, f'IsQueued = {str(scene["queued"][i]).lower()}',
           26 if mobile else 25, 'code', 700 if scene['queued'][i] else 400)
    if i == 2:
        d.text(x+20, y+212, 'Dependencies = []', 25 if mobile else 23, 'code')
        d.text(x+20, y+258, '추가 전달 대상 없음', 24 if mobile else 22, 'sub')
    else:
        # Dependencies is a list; Target belongs to its [0] dependency.
        ny = y+190
        nh = 87 if mobile else 176
        d.parts.append(f'<rect x="{x+14}" y="{ny}" width="{w-28}" height="{nh}" rx="9" fill="var(--canvas)" stroke="var(--line)" stroke-width="1.5"/>')
        d.text(x+26, ny+29, 'Dependencies[0]', 24 if mobile else 23, 'code', 700)
        if mobile:
            d.text(x+26, ny+67, 'Target', 25, 'code')
        else:
            d.text(x+26, ny+70, 'Target', 24, 'code')
            d.text(x+26, ny+114, 'First = null', 23, 'code')
            d.text(x+26, ny+153, 'Inherit = true', 23, 'code')


def queue_chip(d, x, y, w, label, rule, index, mobile=False):
    d.box(x, y, w, 82 if mobile else 96)
    d.text(x+18, y+31, f'{index}. {label}', 23 if mobile else 22, 'sub', 600)
    d.text(x+18, y+65 if mobile else y+74, rule, 25 if mobile else 27, 'code')


def desktop(scene):
    d = Diagram(1000, 820, scene['title'], scene['description'])
    d.text(30, 48, scene['number'], 29, 'new code', 700)
    d.text(90, 48, scene['title'], 30, weight=700)
    d.text(30, 97, scene['subtitle'], 24, 'code sub' if '=' in scene['subtitle'] or '.Dependencies' in scene['subtitle'] else 'sub')
    xs = [30, 354, 678]
    for i, x in enumerate(xs):
        d.text(x, 146, '둘째 경로 · A' if i == 0 else ROLES[i], 25, 'sub')
        if i == scene['source']:
            d.text(x+289, 146, 'source', 21, 'new code', 700, 'end')
        elif i == scene['target']:
            d.text(x+289, 146, 'target', 21, 'new code', 700, 'end')
        entry(d, scene, i, x, 181, 292)
    for i in [0, 1]:
        # The arrow starts at Target *inside* Dependencies[0].
        active = scene['number'] in ['02', '05'] and i == scene['source']
        cls = 'active' if active else ''
        d.arrow(xs[i]+123, 434, xs[i+1], 434, cls)
    d.text(30, 627, f'pending · 대기 {len(scene["pending"])}개', 26, 'code', 700)
    if scene['pending']:
        for n, key in enumerate(scene['pending'], 1):
            i = dict(s=0, middle=1, complete=2)[key]
            x = xs[i]
            queue_chip(d, x, 672, 292, QUEUE_LABELS[key], QUEUE_RULES[key], n)
            if key != 's':
                cx = x+146
                d.path(f'M {cx} 672 L {cx} 571', 'waiting')
                head(d, cx, 571, 'up', 'waiting')
    else:
        d.box(30, 661, 940, 113)
        d.text(52, 709, '[]', 33, 'code', 700)
        msg = ('지금 꺼낸 중간 Entry의 전달 작업은 아직 진행 중입니다.'
               if scene['number'] == '04' else '현재 Entry 처리도 끝났습니다. 새로 등록할 작업이 없습니다.')
        d.text(130, 711, msg, 25)
        d.text(52, 751, 'source 처리 완료와 빈 큐를 함께 확인합니다.' if scene['number']=='06'
               else '큐에서 꺼냈으므로 중간 Entry의 IsQueued는 false입니다.', 23, 'sub')
    d.save(f'lalr-lookahead-requeue-{scene["number"]}.svg')


def mobile(scene):
    d = Diagram(400, 1535, scene['title'], scene['description'])
    d.text(24, 40, scene['number'], 27, 'new code', 700)
    for n, line in enumerate(scene['mobile_title']):
        d.text(24, 89+n*40, line, 29, weight=700)
    ys = [202, 589, 976]
    for i, y in enumerate(ys):
        d.text(24, y-29, ROLES[i], 26, 'sub')
        if i == scene['source']:
            d.text(352, y-29, 'source', 22, 'new code', 700, 'end')
        elif i == scene['target']:
            d.text(352, y-29, 'target', 22, 'new code', 700, 'end')
        entry(d, scene, i, 24, y, 328, True)
    for i in [0, 1]:
        active = scene['number'] in ['02', '05'] and i == scene['source']
        cls = 'active' if active else ''
        y = ys[i]+251
        ny = ys[i+1]+60
        d.path(f'M 136 {y} L 380 {y} L 380 {ny} L 352 {ny}', cls)
        head(d, 352, ny, 'left', cls)
    d.text(24, 1331, f'pending · 대기 {len(scene["pending"])}개', 26, 'code', 700)
    if scene['pending']:
        for n, key in enumerate(scene['pending']):
            y = 1356+n*94
            queue_chip(d, 24, y, 328, QUEUE_LABELS[key], QUEUE_RULES[key], n+1, True)
            if key != 's':
                target_y = ys[dict(middle=1, complete=2)[key]]+150
                d.path(f'M 24 {y+40} L 9 {y+40} L 9 {target_y} L 24 {target_y}', 'waiting')
                head(d, 24, target_y, 'right', 'waiting')
    else:
        d.box(24, 1356, 328, 144)
        d.text(44, 1401, '[]', 32, 'code', 700)
        lines = ['지금 꺼낸 중간 Entry의', '전달 작업은 아직 진행 중입니다.'] if scene['number']=='04' else ['현재 Entry 처리도 끝났고,', '새로 등록할 작업도 없습니다.']
        for n, line in enumerate(lines):
            d.text(44, 1441+n*36, line, 23)
    d.save(f'lalr-lookahead-requeue-{scene["number"]}-mobile.svg')


if __name__ == '__main__':
    for scene in SCENES:
        desktop(scene)
        mobile(scene)
    Path(__file__).with_name('requeue-scene-snapshots.json').write_text(
        json.dumps(SCENES, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')

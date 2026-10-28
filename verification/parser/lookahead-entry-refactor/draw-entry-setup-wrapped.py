from pathlib import Path
import importlib.util

spec = importlib.util.spec_from_file_location('lookahead_diagrams', Path(__file__).with_name('draw-lookahead-images.py'))
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)
Diagram = module.Diagram


def frame(d,x,y,w,h,label,focus=False,font=26):
    fill='var(--focus)' if focus else 'var(--card)'
    d.parts.append(f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="12" fill="{fill}" stroke="var(--line)" stroke-width="2"/>')
    gap=min(w-32,len(label)*font*.56+24)
    d.parts.append(f'<rect x="{x+16}" y="{y-18}" width="{gap}" height="34" fill="var(--canvas)"/>')
    d.text(x+26,y+9,label,font,'code',weight=700)


def desktop():
    d=Diagram(1000,1325,'LR 항목을 LookAheadEntry로 감싸고 내부 프로퍼티를 펼친 구조',
      '위쪽은 초기 상태의 각 LR 항목을 LookAheadEntry 네모로 감싸 표시합니다. 아래쪽은 초기 A 항목 객체를 큐 처리 전에 펼친 값입니다. LookAhead는 빈 집합, IsQueued는 false이며, Dependencies 목록의 첫 LookAheadDependency에는 Target, First null, Inherit true가 있습니다. Target은 a 전이 뒤 중간 항목의 LookAheadEntry 객체를 가리킵니다.')
    d.text(36,42,'LR 항목마다 LookAheadEntry 하나',32,weight=700)
    d.text(36,93,'초기 상태 · 관련 항목',26,'sub')
    for i,(rule,focus) in enumerate([('S → •A !',False),('S → •b c d A ?',False),('A → •a z',True)]):
        y=132+i*138
        frame(d,36,y,928,104,'LookAheadEntry',focus)
        d.text(62,y+72,rule,34,'code new' if focus else 'code')
    d.parts.append('<line x1="36" x2="964" y1="557" y2="557" stroke="var(--line)"/>')
    d.text(36,619,'초기 A 항목의 객체를 펼치면',32,weight=700)
    d.text(36,673,'BuildLookAheadEntries 반환 직후 · 큐 처리 전',26,'sub')
    d.text(36,735,'초기 상태',26,'sub')
    x,y=36,780
    frame(d,x,y,540,490,'LookAheadEntry',True,28)
    d.text(x+26,y+65,'A → •a z',36,'code new')
    d.text(x+26,y+118,'LookAhead = {}',29,'code')
    d.text(x+26,y+168,'IsQueued = false',29,'code')
    frame(d,x+20,y+205,500,265,'Dependencies',False,27)
    d.parts.append(f'<rect x="{x+40}" y="{y+240}" width="460" height="210" rx="9" fill="var(--canvas)" stroke="var(--line)" stroke-width="1.5"/>')
    d.text(x+58,y+277,'[0] LookAheadDependency',25,'code',weight=700)
    d.text(x+58,y+327,'Target',28,'code')
    d.text(x+58,y+377,'First = null',28,'code')
    d.text(x+58,y+427,'Inherit = true',28,'code')
    d.text(700,947,'Target이 가리키는 객체',26,'sub')
    d.text(700,989,'a 전이 뒤의 중간 항목',25,'sub')
    frame(d,700,1030,264,145,'LookAheadEntry',True,25)
    d.text(724,1117,'A → a•z',33,'code')
    d.arrow(x+178,y+318,700,y+318,'active')
    d.save('lalr-lookahead-entry-setup.svg')


def mobile():
    d=Diagram(400,1660,'항목을 감싼 Entry와 내부 Dependencies의 실제 값',
      '각 LR 항목을 LookAheadEntry 네모로 감싸 표시했습니다. 초기 A 객체를 펼치면 LookAhead 빈 집합, IsQueued false, Dependencies 목록이 있습니다. 그 목록의 첫 LookAheadDependency 안에 Target, First null, Inherit true가 있으며 Target 화살표는 a 전이 뒤 중간 항목의 Entry를 가리킵니다.')
    d.text(24,38,'LR 항목마다 객체 하나',28,weight=700)
    d.text(24,88,'초기 상태 · 관련 항목',26,'sub')
    for i,(rule,focus) in enumerate([('S → •A !',False),('S → •b c d A ?',False),('A → •a z',True)]):
        y=132+i*138
        frame(d,24,y,352,104,'LookAheadEntry',focus,26)
        d.text(46,y+73,rule,29,'code new' if focus else 'code')
    d.parts.append('<line x1="24" x2="376" y1="556" y2="556" stroke="var(--line)"/>')
    d.text(24,605,'초기 A 항목의 실제 구조',27,weight=700)
    d.text(24,649,'큐 처리 전의 값',26,'sub')
    x,y=24,720
    frame(d,x,y,352,615,'LookAheadEntry',True,27)
    d.text(x+22,y+72,'A → •a z',32,'code new')
    d.text(x+22,y+128,'LookAhead = {}',26,'code')
    d.text(x+22,y+178,'IsQueued = false',26,'code')
    frame(d,x+12,y+226,328,354,'Dependencies',False,26)
    d.parts.append(f'<rect x="{x+24}" y="{y+265}" width="304" height="292" rx="9" fill="var(--canvas)" stroke="var(--line)" stroke-width="1.5"/>')
    d.text(x+40,y+300,'[0]',24,'code',weight=700)
    d.text(x+40,y+344,'LookAheadDependency',26,'code',weight=700)
    d.text(x+40,y+402,'Target',26,'code')
    d.text(x+40,y+460,'First = null',26,'code')
    d.text(x+40,y+518,'Inherit = true',26,'code')
    d.text(24,1400,'Target이 가리키는 객체',26,'sub')
    frame(d,24,1450,328,165,'LookAheadEntry',True,26)
    d.text(46,1510,'a 전이 뒤의 중간 항목',24,'sub')
    d.text(46,1570,'A → a•z',32,'code')
    d.path(f'M {x+148} {y+393} L 380 {y+393} L 380 1540')
    d.arrow(380,1540,352,1540,'active')
    d.save('lalr-lookahead-entry-setup-mobile.svg')


desktop();mobile()
print('Created wrapped entries with nested dependency properties and values.')

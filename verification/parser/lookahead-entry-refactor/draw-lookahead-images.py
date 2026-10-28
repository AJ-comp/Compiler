from pathlib import Path
from html import escape

OUT = Path('docs/images')

STYLE = '''
  :root{--canvas:#fbfcfe;--ink:#172b3a;--muted:#496072;--line:#c7d4df;--card:#f0f4f8;--focus:#eaf3ff;--blue:#1758b0;--edge:#718698}
  @media(prefers-color-scheme:dark){:root{--canvas:#18212b;--ink:#edf4fc;--muted:#b9ccdf;--line:#455c73;--card:#243343;--focus:#203d60;--blue:#9bc8ff;--edge:#9eb7ce}}
  text{fill:var(--ink);font-family:"Malgun Gothic","Noto Sans KR",sans-serif}
  .code{font-family:Consolas,"Malgun Gothic",monospace}
  .sub{fill:var(--muted)}.new{fill:var(--blue)}
  .edge{stroke:var(--edge);stroke-width:2.5;fill:none;stroke-linecap:round;stroke-linejoin:round}
  .active{stroke:var(--blue)}.waiting{stroke-dasharray:7 8}
'''


class Diagram:
    def __init__(self, width, height, title, description):
        self.width, self.height = width, height
        self.parts = [f'<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" viewBox="0 0 {width} {height}" role="img" aria-labelledby="title desc">',
                      f'<title id="title">{escape(title)}</title><desc id="desc">{escape(description)}</desc>',
                      f'<style>{STYLE}</style>',
                      f'<rect width="{width}" height="{height}" fill="var(--canvas)"/>']

    def text(self, x, y, value, size=26, cls='', weight=400, anchor='start'):
        self.parts.append(f'<text x="{x}" y="{y}" font-size="{size}" font-weight="{weight}" text-anchor="{anchor}" class="{cls}">{escape(value)}</text>')

    def box(self, x, y, w, h, focus=False):
        fill = 'var(--focus)' if focus else 'var(--card)'
        self.parts.append(f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="14" fill="{fill}"/>')

    def path(self, value, cls=''):
        self.parts.append(f'<path d="{value}" class="edge {cls}"/>')

    def arrow(self, x1, y1, x2, y2, cls=''):
        self.path(f'M {x1} {y1} L {x2} {y2}', cls)
        if y1 == y2:
            tail = x2-10 if x2>x1 else x2+10
            self.path(f'M {tail} {y2-7} L {x2} {y2} L {tail} {y2+7}', cls)
        else:
            self.path(f'M {x2-7} {y2-10} L {x2} {y2} L {x2+7} {y2-10}', cls)

    def tokens(self, x, y, two, size=45, anchor='start'):
        if two:
            self.parts.append(f'<text x="{x}" y="{y}" font-size="{size}" text-anchor="{anchor}" class="code">{{!<tspan class="new">, ?</tspan>}}</text>')
        else:
            self.text(x,y,'{!}',size,'code',anchor=anchor)

    def save(self, name):
        (OUT/name).write_text('\n'.join(self.parts+['</svg>'])+'\n', encoding='utf-8')


def connections(mobile=False):
    width,height = (400,1460) if mobile else (1000,1340)
    d=Diagram(width,height,'초기 상태에서 b, c, d 전이를 거쳐 두 번째 A 시작 항목이 생기는 과정',
              '초기 상태에는 S의 두 시작 항목과 첫 규칙에서 Closure로 들어온 A 시작 항목 하나가 있습니다. 두 번째 S 항목은 b, c, d로 Goto한 뒤 점이 A 앞에 오며 그 상태의 Closure에 A 시작 항목이 추가됩니다. 두 상태에서 a로 Goto하면 같은 중간 상태에 도달합니다. 설명에 필요한 상태와 항목을 표시했습니다.')
    if not mobile:
        d.text(36,36,'초기 상태와 각 Goto 전이',30,weight=700)
        d.box(36,64,550,246)
        d.text(60,105,'초기 상태 · 관련 항목',26,'sub')
        d.text(60,153,'S → •A !',31,'code')
        d.text(60,202,'S → •b c d A ?',31,'code')
        d.text(60,266,'A → •a z',33,'code new')
        d.path('M 586 260 L 822 260 L 822 930')
        d.path('M 815 920 L 822 930 L 829 920')
        d.text(664,237,'a',30,'code sub')
        d.arrow(280,310,280,405)
        d.text(309,365,'b',30,'code sub')
        d.box(36,405,550,128)
        d.text(60,446,'b 전이 뒤',26,'sub')
        d.text(60,500,'S → b•c d A ?',31,'code')
        d.arrow(280,533,280,625)
        d.text(309,588,'c',30,'code sub')
        d.box(36,625,550,128)
        d.text(60,666,'c 전이 뒤',26,'sub')
        d.text(60,720,'S → b c•d A ?',31,'code')
        d.arrow(280,753,280,845)
        d.text(309,808,'d',30,'code sub')
        d.box(36,845,550,216)
        d.text(60,886,'d 전이 뒤 · 점이 A 앞에 도착',26,'sub')
        d.text(60,932,'S → b c d•A ?',31,'code')
        d.arrow(78,944,78,977)
        d.text(104,968,'Closure',25,'code sub')
        d.text(60,1037,'A → •a z',33,'code new')
        d.arrow(586,1002,682,1002)
        d.text(633,979,'a',30,'code sub',anchor='middle')
        d.box(682,930,280,140,True)
        d.text(706,974,'같은 중간 상태',26,'sub')
        d.text(706,1031,'A → a•z',33,'code')
        d.arrow(822,1070,822,1164)
        d.text(850,1126,'z',30,'code sub')
        d.box(682,1164,280,140)
        d.text(706,1209,'완료 상태',26,'sub')
        d.text(706,1266,'A → az•',33,'code')
    else:
        d.text(200,32,'관련 LR(0) 상태와 전이',25,weight=700,anchor='middle')
        d.box(18,50,330,234)
        d.text(40,89,'초기 상태',26,'sub')
        d.text(40,132,'S → •A !',28,'code')
        d.text(40,180,'S → •b c d A ?',28,'code')
        d.text(40,246,'A → •a z',30,'code new')
        d.path('M 348 244 L 380 244 L 380 1150')
        d.arrow(380,1150,348,1150)
        d.text(365,224,'a',26,'code sub')
        d.arrow(183,284,183,360)
        d.text(211,330,'b',28,'code sub')
        d.box(18,360,330,120)
        d.text(40,400,'b 전이 뒤',26,'sub')
        d.text(40,448,'S → b•c d A ?',28,'code')
        d.arrow(183,480,183,555)
        d.text(211,525,'c',28,'code sub')
        d.box(18,555,330,120)
        d.text(40,595,'c 전이 뒤',26,'sub')
        d.text(40,643,'S → b c•d A ?',28,'code')
        d.arrow(183,675,183,750)
        d.text(211,720,'d',28,'code sub')
        d.box(18,750,330,228)
        d.text(40,790,'d 전이 뒤',26,'sub')
        d.text(40,838,'S → b c d•A ?',28,'code')
        d.arrow(78,853,78,899)
        d.text(104,885,'Closure',26,'code sub')
        d.text(40,950,'A → •a z',30,'code new')
        d.arrow(183,978,183,1080)
        d.text(211,1039,'a',28,'code sub')
        d.box(18,1080,330,140,True)
        d.text(40,1122,'같은 중간 상태',26,'sub')
        d.text(40,1180,'A → a•z',32,'code')
        d.arrow(183,1220,183,1315)
        d.text(211,1275,'z',28,'code sub')
        d.box(18,1315,330,120)
        d.text(40,1355,'완료 상태',26,'sub')
        d.text(40,1407,'A → az•',32,'code')
    d.save('lalr-lookahead-connections-mobile.svg' if mobile else 'lalr-lookahead-connections.svg')


def requeue(mobile=False):
    width,height=(400,1530) if mobile else (1000,940)
    d=Diagram(width,height,'늦게 추가된 물음표를 큐 재등록으로 완료 항목까지 전달',
              '첫 장면은 중간과 완료 집합이 모두 느낌표 하나입니다. 둘째 장면에서 중간 집합에 물음표가 추가되고 큐에 재등록되지만 완료 집합은 아직 느낌표 하나입니다. 셋째 장면에서 중간 객체를 다시 꺼내 전달해 완료 집합에도 물음표가 추가됩니다.')
    scenes=[('이미 !를 전달한 상태',False,False,False,'전달한 결과'),
            ('새 ? 도착 · 큐에 재등록',True,False,True,'전달 대기'),
            ('다시 꺼내 전달',True,True,False,'재전파')]
    for i,(title,left,right,queued,edge) in enumerate(scenes):
        top=i*(510 if mobile else 310)
        if i:
            d.parts.append(f'<line x1="24" x2="{width-24}" y1="{top+5}" y2="{top+5}" stroke="var(--line)"/>')
        if mobile:
            title = ['이미 전달한 상태','새 ? 도착 · 다시 등록','다시 꺼내 전달'][i]
            d.text(24,top+43,title,27,weight=700)
            d.box(24,top+69,352,184,True)
            d.text(46,top+104,'중간 항목',26,'sub')
            d.text(46,top+144,'A → a•z',31,'code')
            d.tokens(46,top+198,left,43)
            d.text(46,top+232,'IsQueued = '+str(queued).lower(),25,'code sub')
            d.arrow(200,top+253,200,top+328,'waiting' if i==1 else 'active' if i==2 else '')
            d.text(223,top+298,edge,26,'sub')
            d.box(24,top+328,352,154,focus=right)
            d.text(46,top+364,'완료 항목',26,'sub')
            d.text(46,top+403,'A → az•',31,'code')
            d.tokens(46,top+457,right,43)
        else:
            d.text(36,top+43,title,30,weight=700)
            d.box(36,top+68,388,196,True)
            d.text(60,top+105,'중간 항목',25,'sub')
            d.text(60,top+146,'A → a•z',33,'code')
            d.tokens(60,top+202,left,46)
            d.text(60,top+242,'IsQueued = '+str(queued).lower(),25,'code sub')
            d.arrow(424,top+171,576,top+171,'waiting' if i==1 else 'active' if i==2 else '')
            d.text(500,top+139,edge,23,'sub',anchor='middle')
            d.box(576,top+68,388,196,focus=right)
            d.text(600,top+105,'완료 항목',25,'sub')
            d.text(600,top+146,'A → az•',33,'code')
            d.tokens(600,top+202,right,46)
    d.save('lalr-lookahead-requeue-mobile.svg' if mobile else 'lalr-lookahead-requeue.svg')


if __name__ == '__main__':
    OUT.mkdir(exist_ok=True)
    connections();connections(True);requeue();requeue(True)
    print('Created four diagram images.')

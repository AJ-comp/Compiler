import json
import re
from html.parser import HTMLParser
from pathlib import Path
from urllib.parse import unquote, urlsplit
import xml.etree.ElementTree as ET

root = Path(__file__).resolve().parents[2]
doc = root / 'docs/ko/parser-driven-lexing.html'
site = root / 'artifacts/parser-driven-lexing/site/_site'
built = site / 'ko/parser-driven-lexing.html'
issues = []

class Document(HTMLParser):
    def __init__(self, text):
        super().__init__(convert_charrefs=True)
        self.ids = set()
        self.links = []
        self.stack = []
        self.feed(text)
        if self.stack:
            issues.append('Unclosed HTML tags: ' + repr(self.stack))

    def handle_starttag(self, tag, attrs):
        attrs = dict(attrs)
        if 'id' in attrs:
            if attrs['id'] in self.ids:
                issues.append('Duplicate id: ' + attrs['id'])
            self.ids.add(attrs['id'])
        if tag == 'a' and 'href' in attrs:
            self.links.append(attrs['href'])
        if tag not in {'area','base','br','col','embed','hr','img','input','link','meta','param','source','track','wbr'}:
            self.stack.append(tag)

    def handle_endtag(self, tag):
        if not self.stack or self.stack[-1] != tag:
            issues.append('Mismatched HTML closing tag: ' + tag)
        else:
            self.stack.pop()

text = doc.read_text(encoding='utf-8')
parsed = Document(text)
matches = built.exists() and doc.read_bytes() == built.read_bytes()
if not matches:
    issues.append('Built HTML differs from the source')
local_links = 0
for href in parsed.links:
    url = urlsplit(href)
    if url.scheme or url.netloc:
        continue
    local_links += 1
    target = (built.parent / unquote(url.path)).resolve() if url.path else built
    if not target.exists():
        issues.append('Missing local link: ' + href)
    elif url.fragment:
        target_ids = parsed.ids if target == built else Document(target.read_text(encoding='utf-8')).ids
        if unquote(url.fragment) not in target_ids:
            issues.append('Missing local fragment: ' + href)

summary_links = []
for source in (root / 'src').rglob('*.cs'):
    if 'obj' in source.parts or 'bin' in source.parts:
        continue
    for href in re.findall(r'href="([^"]*parser-driven-lexing\.html(?:#[^"]*)?)"', source.read_text(encoding='utf-8-sig')):
        path, _, fragment = href.partition('#')
        valid = (source.parent / path).resolve() == doc.resolve() and (not fragment or fragment in parsed.ids)
        summary_links.append({'source':str(source.relative_to(root)), 'href':href, 'valid':valid})
        if not valid:
            issues.append('Invalid summary link: ' + str(source))

author_preserved = '저자가 설계하고 작성했습니다' in text and 'Codex는' in text
if not author_preserved:
    issues.append('Author/designer attribution is missing')
required_anchors = {'exchange','goal-selection','lexer-role','parser-role','prediction-role'}
if not required_anchors <= parsed.ids:
    issues.append('Missing required class-summary anchors')

tests = {}
for name in ['policy-api', 'mcp-policy-api']:
    report = root / f'artifacts/parser-driven-lexing/tests/{name}.trx'
    tree = ET.parse(report)
    counters = next(node for node in tree.iter() if node.tag.endswith('Counters'))
    tests[name] = counters.attrib
    if counters.attrib.get('failed') != '0':
        issues.append('Test failure: ' + name)

report = {'html':str(doc.relative_to(root)), 'builtHtmlMatches':matches,
          'anchors':len(parsed.ids), 'localLinks':local_links,
          'summaryLinks':summary_links, 'authorAttributionPreserved':author_preserved,
          'tests':tests, 'issues':issues}
destination = root / 'artifacts/parser-driven-lexing/verification.json'
destination.write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({'anchors':len(parsed.ids),'summaryLinks':len(summary_links),
                  'authorAttributionPreserved':author_preserved,'issues':issues},ensure_ascii=False))
raise SystemExit(bool(issues))

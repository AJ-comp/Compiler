import type { Locale } from "./language";
import ru from "./locales/ru.ts";
import zhCN from "./locales/zh-CN.ts";
import zhTW from "./locales/zh-TW.ts";
import uk from "./locales/uk.ts";
import vi from "./locales/vi.ts";
import de from "./locales/de.ts";
import fr from "./locales/fr.ts";
import th from "./locales/th.ts";
import es from "./locales/es.ts";
import pt from "./locales/pt.ts";
import it from "./locales/it.ts";

export type MessageValues = Record<string, string | number>;

// Canonical English UI messages are also the fallback. Grammar symbols, input,
// parser actions, and engine diagnostics are intentionally supplied as raw values.
export const messages: Readonly<
  Record<string, readonly [ko: string, ja: string]>
> = {
  File: ["파일", "ファイル"],
  "Open file…": ["파일 열기…", "ファイルを開く…"],
  "Save file": ["파일 저장", "ファイルを保存"],
  "Grammar and input in one file": [
    "문법과 입력을 하나의 파일로",
    "文法と入力を一つのファイルに",
  ],
  "Saved {name}.": ["{name} 파일을 저장했습니다.", "{name} を保存しました。"],
  "Opened {name}.": ["{name} 파일을 열었습니다.", "{name} を開きました。"],
  "Unable to save the file. Please try again.": [
    "파일을 저장하지 못했습니다. 다시 시도해 주세요.",
    "ファイルを保存できませんでした。もう一度お試しください。",
  ],
  "Unable to read this file. Please try again.": [
    "이 파일을 읽지 못했습니다. 다시 시도해 주세요.",
    "このファイルを読み込めませんでした。もう一度お試しください。",
  ],
  "This is not a valid Janglim workspace file.": [
    "올바른 Janglim 작업 공간 파일이 아닙니다.",
    "有効な Janglim ワークスペースファイルではありません。",
  ],
  "This file uses an unsupported workspace version.": [
    "이 파일의 작업 공간 버전은 지원하지 않습니다.",
    "このファイルのワークスペースバージョンはサポートされていません。",
  ],
  "Choose a workspace file smaller than 2 MB.": [
    "2 MB보다 작은 작업 공간 파일을 선택해 주세요.",
    "2 MB 未満のワークスペースファイルを選択してください。",
  ],
  "Replace your current work?": [
    "현재 작업 내용을 바꾸시겠어요?",
    "現在の作業内容を置き換えますか？",
  ],
  "You have unsaved changes. Save your file before opening another workspace.":
    [
      "저장하지 않은 변경 사항이 있습니다. 다른 작업 공간을 열기 전에 파일을 저장해 주세요.",
      "保存されていない変更があります。別のワークスペースを開く前にファイルを保存してください。",
    ],
  "Replace workspace": ["작업 공간 바꾸기", "ワークスペースを置き換える"],
  Cancel: ["취소", "キャンセル"],
  "Dismiss notification": ["알림 닫기", "通知を閉じる"],
  Language: ["언어", "言語"],
  "Language: {language}": ["언어: {language}", "言語: {language}"],
  "Automatic (browser)": ["자동 · 브라우저 설정", "自動 · ブラウザー設定"],
  "Following browser language": [
    "브라우저 언어 사용 중",
    "ブラウザーの言語を使用中",
  ],
  "Janglim Playground": ["Janglim 플레이그라운드", "Janglim プレイグラウンド"],
  "Janglim Playground · Explore your grammar": [
    "Janglim 플레이그라운드 · 문법 탐색",
    "Janglim プレイグラウンド · 文法を探索",
  ],
  "An interactive EBNF and LALR parser playground. Visualize syntax trees and explore every parsing decision with Janglim.":
    [
      "EBNF 문법과 LALR 파싱을 실험하는 플레이그라운드입니다. Janglim으로 구문 트리를 시각화하고 파싱의 각 단계를 살펴보세요.",
      "EBNF 文法と LALR 解析を試せるプレイグラウンドです。Janglim で構文木を可視化し、解析の各ステップを確認できます。",
    ],
  "Preparing your grammar workspace…": [
    "문법 작업 공간을 준비하고 있어요…",
    "文法ワークスペースを準備しています…",
  ],
  "Loading Janglim Playground…": [
    "Janglim 플레이그라운드를 불러오고 있어요…",
    "Janglim プレイグラウンドを読み込んでいます…",
  ],
  "An unhandled error has occurred.": [
    "처리하지 못한 오류가 발생했습니다.",
    "処理できないエラーが発生しました。",
  ],
  Reload: ["새로고침", "再読み込み"],
  "Unable to load the playground: {message}": [
    "플레이그라운드를 불러오지 못했습니다: {message}",
    "プレイグラウンドを読み込めませんでした: {message}",
  ],
  "Unable to load the playground:": [
    "플레이그라운드를 불러오지 못했습니다:",
    "プレイグラウンドを読み込めませんでした:",
  ],
  "Workspace navigation": ["작업 공간 탐색", "ワークスペースのナビゲーション"],
  "Parse tree": ["구문 트리", "構文木"],
  "Parsing table": ["파싱 테이블", "解析表"],
  "Execution trace": ["실행 기록", "実行トレース"],
  "Janglim documentation": ["Janglim 문서", "Janglim ドキュメント"],
  "Switch to light theme": ["밝은 테마로 전환", "ライトテーマに切り替え"],
  "Switch to dark theme": ["어두운 테마로 전환", "ダークテーマに切り替え"],
  Playground: ["플레이그라운드", "プレイグラウンド"],
  Preview: ["미리보기", "プレビュー"],
  "LALR(1) engine": ["LALR(1) 엔진", "LALR(1) エンジン"],
  Documentation: ["문서", "ドキュメント"],
  "Source on GitHub": ["GitHub 소스 코드", "GitHub のソースコード"],
  "GRAMMAR WORKSPACE": ["문법 작업 공간", "文法ワークスペース"],
  "See your grammar take shape": [
    "문법이 트리가 되는 과정을 살펴보세요",
    "文法が構文木になる過程を見てみよう",
  ],
  "Write a grammar. Run an input. Explore every parsing decision.": [
    "문법을 작성하고 입력을 파싱한 뒤, 각 단계를 살펴보세요.",
    "文法を書き、入力を解析し、各ステップを確認できます。",
  ],
  Examples: ["예제", "サンプル"],
  "Explore a grammar": ["예제 문법 살펴보기", "サンプル文法を試す"],
  "Arithmetic expressions": ["산술식", "算術式"],
  "Precedence & nested expressions": [
    "연산자 우선순위와 중첩된 식",
    "演算子の優先順位と入れ子の式",
  ],
  "Parenthesized expression": ["괄호가 있는 식", "括弧付きの式"],
  "Explore a deeper syntax tree": [
    "더 깊은 구문 트리 살펴보기",
    "より深い構文木を確認",
  ],
  "Ambiguous grammar": ["모호한 문법", "曖昧な文法"],
  "Inspect shift/reduce conflicts": [
    "shift/reduce 충돌 살펴보기",
    "shift/reduce の競合を確認",
  ],
  "Rejected input": ["거부되는 입력", "受理されない入力"],
  "Follow the parser to an error": [
    "오류가 발생하는 단계까지 따라가기",
    "エラーが起きるステップまで確認",
  ],
  "Parsing…": ["파싱 중…", "解析中…"],
  "Run parser": ["파싱 실행", "解析を実行"],
  "Building your parse": [
    "파싱 결과를 만들고 있어요",
    "解析結果を生成しています",
  ],
  "Building your parse…": [
    "파싱 결과를 만들고 있어요…",
    "解析結果を生成しています…",
  ],
  "Edited · run to update": [
    "수정됨 · 실행하면 갱신됩니다",
    "変更あり · 実行して更新",
  ],
  "Grammar needs attention": ["문법을 확인해 주세요", "文法を確認してください"],
  "Input accepted": ["입력이 수락되었습니다", "入力が受理されました"],
  "Input rejected": ["입력이 거부되었습니다", "入力が受理されませんでした"],
  "Ready to parse": ["파싱할 준비가 되었어요", "解析の準備ができました"],
  "The input matches your grammar.": [
    "입력이 문법과 일치합니다.",
    "入力が文法に一致しています。",
  ],
  "Explore the trace to find the error.": [
    "실행 기록에서 오류가 발생한 단계를 확인하세요.",
    "実行トレースでエラーの箇所を確認してください。",
  ],
  tokens: ["토큰", "トークン"],
  states: ["상태", "状態"],
  steps: ["단계", "ステップ"],
  "{count} tokens": ["토큰 {count}개", "{count} 個のトークン"],
  "{count} states": ["상태 {count}개", "{count} 個の状態"],
  "{count} steps": ["{count}단계", "{count} ステップ"],
  "Unable to run this grammar": [
    "이 문법을 실행하지 못했습니다",
    "この文法を実行できませんでした",
  ],
  "The parser could not complete this run.": [
    "파서가 실행을 완료하지 못했습니다.",
    "パーサーが実行を完了できませんでした。",
  ],
  "{count} grammar conflict": ["문법 충돌 {count}개", "文法の競合 {count} 件"],
  "{count} grammar conflicts": ["문법 충돌 {count}개", "文法の競合 {count} 件"],
  "Grammar conflicts: {count}": [
    "문법 충돌: {count}개",
    "文法の競合: {count} 件",
  ],
  "Inspect ambiguity": ["모호성 살펴보기", "曖昧さを確認"],
  "This grammar is not LALR(1). The engine continues with its default action at each conflict.":
    [
      "이 문법은 LALR(1)이 아닙니다. 엔진은 각 충돌에서 기본 동작을 선택해 계속 진행합니다.",
      "この文法は LALR(1) ではありません。エンジンは競合ごとに既定の動作を選んで続行します。",
    ],
  Grammar: ["문법", "文法"],
  "Grammar copied": ["문법을 복사했습니다", "文法をコピーしました"],
  "Copy grammar": ["문법 복사", "文法をコピー"],
  "Reset default example": ["기본 예제로 초기화", "既定のサンプルに戻す"],
  "{count} lines": ["{count}줄", "{count} 行"],
  "Literals in single quotes": [
    "작은따옴표로 리터럴 작성",
    "リテラルは一重引用符で囲む",
  ],
  "Regex in double quotes": [
    "큰따옴표로 정규식 작성",
    "正規表現は二重引用符で囲む",
  ],
  "Input string": ["입력 문자열", "入力文字列"],
  "Plain text": ["일반 텍스트", "プレーンテキスト"],
  "{count} characters": ["{count}자", "{count} 文字"],
  "Enter to run": ["Enter로 실행", "Enter で実行"],
  "Your grammar runs locally in your browser.": [
    "문법은 브라우저 안에서 실행됩니다.",
    "文法の解析はブラウザー内で実行されます。",
  ],
  "Parser results": ["파싱 결과", "解析結果"],
  Trace: ["실행 기록", "トレース"],
  "Exit expanded view": ["확대 보기 종료", "拡大表示を終了"],
  "Expand results": ["결과 확대 보기", "結果を拡大表示"],
  "CONCRETE SYNTAX TREE": ["구체 구문 트리", "具象構文木"],
  Nonterminal: ["비단말", "非終端記号"],
  Terminal: ["단말", "終端記号"],
  "Zoom out": ["축소", "縮小"],
  "Fit tree width": ["트리 너비 맞추기", "構文木の幅に合わせる"],
  "Zoom in": ["확대", "拡大"],
  "Complete parse tree": ["완성된 구문 트리", "完成した構文木"],
  "Stack forest after the current action": [
    "현재 동작 이후 스택의 트리들",
    "現在の動作後のスタック内の構文木",
  ],
  "Current step": ["현재 단계", "現在のステップ"],
  "Parsing complete": ["파싱 완료", "解析完了"],
  "Run to begin": ["실행하면 시작됩니다", "実行して開始"],
  Lookahead: ["룩어헤드", "先読み"],
  "Stack depth": ["스택 깊이", "スタックの深さ"],
  "Selected symbol": ["선택한 심벌", "選択した記号"],
  "Clear symbol selection": ["심벌 선택 해제", "記号の選択を解除"],
  Token: ["토큰", "トークン"],
  Rule: ["규칙", "規則"],
  "Input · characters {start}–{end}": [
    "입력 · {start}–{end}번째 문자",
    "入力 · {start}～{end} 文字目",
  ],
  "No consumed input": ["소비한 입력 없음", "消費した入力なし"],
  "Grammar · {count} location": ["문법 · {count}곳", "文法 · {count} 箇所"],
  "Grammar · {count} locations": ["문법 · {count}곳", "文法 · {count} 箇所"],
  "Grammar locations · {count}": [
    "문법 위치 · {count}곳",
    "文法の箇所 · {count} 箇所",
  ],
  "Generated symbol · no direct grammar location": [
    "자동 생성 심벌 · 문법에 직접 대응하는 위치 없음",
    "生成された記号 · 文法内の直接の対応箇所なし",
  ],
  "Run again to link the tree to your edits.": [
    "다시 실행하면 트리가 수정한 내용과 연결됩니다.",
    "再実行すると、構文木が変更内容に対応します。",
  ],
  "Select a tree node to highlight its grammar and input.": [
    "트리 노드를 선택하면 대응하는 문법과 입력이 강조됩니다.",
    "ノードを選択すると、対応する文法と入力が強調表示されます。",
  ],
  "Parser stack": ["파서 스택", "パーサーのスタック"],
  TOP: ["상단", "先頭"],
  "The stack appears after parsing.": [
    "파싱을 실행하면 스택이 표시됩니다.",
    "解析を実行するとスタックが表示されます。",
  ],
  "LALR(1) · after action": ["LALR(1) · 동작 이후", "LALR(1) · 動作後"],
  "Every transition in the generated parsing table.": [
    "생성된 파싱 테이블의 모든 전이를 확인합니다.",
    "生成された解析表のすべての遷移を確認できます。",
  ],
  "Filter parsing table": ["파싱 테이블 필터", "解析表を絞り込む"],
  "Filter state, symbol or action…": [
    "상태, 심벌, 동작으로 필터…",
    "状態・記号・動作で絞り込み…",
  ],
  State: ["상태", "状態"],
  Symbol: ["심벌", "記号"],
  Action: ["동작", "動作"],
  "Target / production": ["대상 / 생성규칙", "遷移先 / 生成規則"],
  "No entries match your filter.": [
    "필터와 일치하는 항목이 없습니다.",
    "条件に一致する項目はありません。",
  ],
  "Run a valid grammar to generate its parsing table.": [
    "올바른 문법을 실행하면 파싱 테이블이 생성됩니다.",
    "有効な文法を実行すると解析表が生成されます。",
  ],
  "{count} of {total} entries": [
    "전체 {total}개 중 {count}개 항목",
    "全 {total} 件中 {count} 件の項目",
  ],
  "Select a step to inspect its tree and stack.": [
    "단계를 선택하면 해당 시점의 트리와 스택을 볼 수 있습니다.",
    "ステップを選択すると、その時点の構文木とスタックを確認できます。",
  ],
  Step: ["단계", "ステップ"],
  Stack: ["스택", "スタック"],
  "Go to step {count}": ["{count}단계로 이동", "ステップ {count} に移動"],
  "Inspect tree at step {count}": [
    "{count}단계의 트리 살펴보기",
    "ステップ {count} の構文木を確認",
  ],
  "Go to step {step}": ["{step}단계로 이동", "ステップ {step} に移動"],
  "Inspect tree at step {step}": [
    "{step}단계의 트리 살펴보기",
    "ステップ {step} の構文木を確認",
  ],
  "Run a grammar to capture the execution trace.": [
    "문법을 실행하면 실행 기록이 표시됩니다.",
    "文法を実行すると実行トレースが記録されます。",
  ],
  "Run a grammar to explore the execution trace.": [
    "문법을 실행하고 실행 기록을 살펴보세요.",
    "文法を実行し、実行トレースを確認できます。",
  ],
  "{count} decisions": ["파싱 결정 {count}개", "{count} 回の解析判断"],
  "Step {step} selected": ["{step}단계 선택됨", "ステップ {step} を選択中"],
  "First step": ["첫 단계", "最初のステップ"],
  "Previous step": ["이전 단계", "前のステップ"],
  "Pause playback": ["재생 일시정지", "再生を一時停止"],
  "Play trace": ["실행 기록 재생", "トレースを再生"],
  "Next step": ["다음 단계", "次のステップ"],
  "Last step": ["마지막 단계", "最後のステップ"],
  "Parsing step": ["파싱 단계", "解析ステップ"],
  "Playback speed": ["재생 속도", "再生速度"],
  "Janglim · an open source parser engine": [
    "Janglim · 오픈 소스 파서 엔진",
    "Janglim · オープンソースのパーサーエンジン",
  ],
  Lexer: ["렉서", "字句解析器"],
  "LALR parser": ["LALR 파서", "LALR パーサー"],
  "Syntax tree": ["구문 트리", "構文木"],
  "EBNF grammar editor": ["EBNF 문법 편집기", "EBNF 文法エディター"],
  "Highlighted grammar source for {symbol}.": [
    "{symbol}에 대응하는 문법을 강조했습니다.",
    "{symbol} に対応する文法を強調表示しました。",
  ],
  "Highlighted source for {symbol}.": [
    "{symbol}에 대응하는 입력을 강조했습니다.",
    "{symbol} に対応する入力を強調表示しました。",
  ],
  "Enter a string to parse…": [
    "파싱할 문자열을 입력하세요…",
    "解析する文字列を入力…",
  ],
  "Parsing your grammar…": ["문법을 파싱하고 있어요…", "文法を解析しています…"],
  TOKEN: ["토큰", "トークン"],
  RULE: ["규칙", "規則"],
  "Your parse tree starts here": [
    "여기에서 구문 트리가 시작됩니다",
    "ここから構文木が始まります",
  ],
  "Run a grammar to explore each shift and reduction.": [
    "문법을 실행하고 이동과 축약의 각 단계를 살펴보세요.",
    "文法を実行し、シフトと還元の各ステップを確認できます。",
  ],
  "{count} tree": ["트리 {count}개", "{count} 個の構文木"],
  "{count} trees": ["트리 {count}개", "{count} 個の構文木"],
  "{count} nodes": ["노드 {count}개", "{count} 個のノード"],
  "Concrete syntax tree forest": [
    "구체 구문 트리 모음",
    "具象構文木のフォレスト",
  ],
  "Parse tree roots and their children": [
    "구문 트리의 루트와 자식 노드",
    "構文木のルートと子ノード",
  ],
  "Stack state I{state}": ["스택 상태 I{state}", "スタックの状態 I{state}"],
  "State {state}": ["상태 {state}", "状態 {state}"],
  "Select a tree node to highlight its grammar and input source. Use Enter or Space to select a focused node, and Escape to clear selection. Token offsets identify repeated input tokens; the end offset is exclusive.":
    [
      "트리 노드를 선택하면 대응하는 문법과 입력이 강조됩니다. 포커스한 노드는 Enter 또는 Space로 선택하고, Escape로 선택을 해제하세요. 토큰 위치로 반복되는 입력을 구분하며, 끝 위치는 범위에 포함되지 않습니다.",
      "ノードを選択すると、対応する文法と入力が強調表示されます。フォーカス中のノードは Enter または Space で選択し、Escape で解除できます。トークンの位置で同じ入力の繰り返しを区別します。終了位置は範囲に含まれません。",
    ],
  "Concrete syntax tree forest with {count} root and {nodes} nodes": [
    "루트 {count}개와 노드 {nodes}개로 이루어진 구체 구문 트리 모음",
    "{count} 個のルートと {nodes} 個のノードからなる具象構文木のフォレスト",
  ],
  "Concrete syntax tree forest with {count} roots and {nodes} nodes": [
    "루트 {count}개와 노드 {nodes}개로 이루어진 구체 구문 트리 모음",
    "{count} 個のルートと {nodes} 個のノードからなる具象構文木のフォレスト",
  ],
  "{kind}{symbol}: {label}; node {id}": [
    "{kind}{symbol}: {label}; 노드 {id}",
    "{kind}{symbol}: {label}; ノード {id}",
  ],
  "{kind}{symbol}: {label}; input offsets {start} to {end}, end exclusive; node {id}":
    [
      "{kind}{symbol}: {label}; 입력 위치 {start}부터 {end}까지, 끝 위치 제외; 노드 {id}",
      "{kind}{symbol}: {label}; 入力位置 {start} から {end} まで、終了位置は含まない; ノード {id}",
    ],
};

export const catalogs: Readonly<
  Record<Exclude<Locale, "en">, Readonly<Record<string, string>>>
> = {
  ko: Object.fromEntries(
    Object.entries(messages).map(([key, pair]) => [key, pair[0]]),
  ),
  ja: Object.fromEntries(
    Object.entries(messages).map(([key, pair]) => [key, pair[1]]),
  ),
  ru,
  "zh-CN": zhCN,
  "zh-TW": zhTW,
  uk,
  vi,
  de,
  fr,
  th,
  es,
  pt,
  it,
};

export function translate(
  locale: Locale,
  message: string,
  values: MessageValues = {},
): string {
  const catalog = locale === "en" ? undefined : catalogs[locale];
  const text =
    catalog && Object.hasOwn(catalog, message) ? catalog[message] : message;
  return text.replace(
    /\{([a-zA-Z][a-zA-Z0-9_]*)\}/g,
    (placeholder, key: string) =>
      Object.hasOwn(values, key) ? String(values[key]) : placeholder,
  );
}

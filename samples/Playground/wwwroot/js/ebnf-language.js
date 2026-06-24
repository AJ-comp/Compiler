// Registers an "ebnf" language with Monaco so the grammar editor highlights the regions of our
// EBNF notation. Mirrors the engine's own tokenizer:
//   rule/token name -> 'type'      'literal' -> 'string'      "regex" -> 'regexp'
//   : := | ;        -> 'operators'  # comment -> 'comment'
// Polls until Monaco has loaded (editor.main.js may finish after this script runs).
(function register() {
    if (typeof monaco === 'undefined' || !monaco.languages || !monaco.editor) {
        setTimeout(register, 50);
        return;
    }

    monaco.languages.register({ id: 'ebnf' });

    monaco.languages.setMonarchTokensProvider('ebnf', {
        tokenizer: {
            root: [
                [/#.*/, 'comment'],
                [/'[^']*'/, 'string'],                 // 'literal' terminal
                [/"[^"]*"/, 'regexp'],                 // "regex" pattern
                [/:=/, 'operators'],
                [/[:|;]/, 'operators'],
                [/[A-Za-z_][A-Za-z0-9_]*/, 'type'],    // rule / token name
            ]
        }
    });

    // A theme that gives each region a clearly distinct color (independent of the default mapping).
    monaco.editor.defineTheme('ebnf-theme', {
        base: 'vs',
        inherit: true,
        rules: [
            { token: 'comment', foreground: '6A9955' },    // green
            { token: 'string', foreground: 'A31515' },     // dark red  (literals)
            { token: 'regexp', foreground: 'B36B00' },     // orange    (patterns)
            { token: 'operators', foreground: '808080' },  // grey      ( : := | ; )
            { token: 'type', foreground: '267F99' },       // teal      (rule names)
        ],
        colors: {}
    });
})();

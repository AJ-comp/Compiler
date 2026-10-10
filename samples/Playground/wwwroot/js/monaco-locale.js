// Use only NLS packs present in the bundled Monaco distribution.
// Configure its AMD loader before editor.main.js evaluates, using the language
// already resolved by the shared bootstrap (including an explicit saved choice).
(function () {
    var locale = document.documentElement.lang;
    var supported = ['ko', 'ja', 'ru', 'zh-cn', 'zh-tw', 'de', 'fr', 'es', 'it'];
    var nlsLocale = locale.toLowerCase();
    if (typeof require === 'function' && typeof require.config === 'function') {
        require.config({
            'vs/nls': { availableLanguages: { '*': supported.indexOf(nlsLocale) !== -1 ? nlsLocale : 'en' } }
        });
    }
})();

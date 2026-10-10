import assert from "node:assert/strict";
import { readFile, readdir } from "node:fs/promises";
import path from "node:path";
import { test } from "node:test";
import { fileURLToPath } from "node:url";
import { parse } from "@babel/parser";
import {
  LANGUAGE_STORAGE_KEY,
  LANGUAGE_NAMES,
  SUPPORTED_LOCALES,
  isLocale,
  parseLanguagePreference,
  persistLanguagePreference,
  readLanguagePreference,
  resolveLocale,
} from "../src/language.ts";
import { catalogs, messages, translate } from "../src/messages.ts";

test("browser language order takes precedence over the supported language order", () => {
  assert.equal(resolveLocale(["fr-FR", "ja-JP", "ko-KR"]), "fr");
  assert.equal(resolveLocale(["ko-KR", "en-US", "ja-JP"]), "ko");
  assert.equal(resolveLocale(["en-GB", "ko-KR"]), "en");
});

test("regional, script, and case variants resolve to their supported base language", () => {
  for (const [tag, expected] of [
    ["ko-KR", "ko"],
    ["ja-JP", "ja"],
    ["en-US", "en"],
    ["JA-jpan-JP", "ja"],
    [" EN-latn-US ", "en"],
    ["ru-RU", "ru"],
    ["uk-UA", "uk"],
    ["vi-VN", "vi"],
    ["de-AT", "de"],
    ["fr-CA", "fr"],
    ["th-TH", "th"],
    ["es-MX", "es"],
    ["pt-BR", "pt"],
    ["pt-PT", "pt"],
    ["it-CH", "it"],
  ]) {
    assert.equal(resolveLocale([tag]), expected, tag);
  }
});

test("unsupported and malformed language lists safely fall back to English", () => {
  for (const list of [[], ["ar", "nl-NL"], ["ko_KR", "ja--JP", "enUS"], [""]]) {
    assert.equal(resolveLocale(list), "en");
  }
  assert.equal(resolveLocale(["enUS", "ja-JP"]), "ja");
});

test("Chinese scripts override regions and traditional regions select the Taiwan interface", () => {
  for (const [tag, expected] of [
    ["zh", "zh-CN"],
    ["zh-CN", "zh-CN"],
    ["zh-SG", "zh-CN"],
    ["zh-TW", "zh-TW"],
    ["zh-HK", "zh-TW"],
    ["zh-MO", "zh-TW"],
    ["ZH-Hant", "zh-TW"],
    ["zh-Hant-CN", "zh-TW"],
    ["zh-Hans", "zh-CN"],
    ["zh-Hans-TW", "zh-CN"],
    ["zh-Hant-TW-u-nu-hanidec", "zh-TW"],
    ["zh-CN-u-co-hant", "zh-CN"],
  ])
    assert.equal(resolveLocale([tag]), expected, tag);
  assert.equal(resolveLocale(["nl-NL", "zh-Hant", "en-US"]), "zh-TW");
  assert.equal(resolveLocale(["zh-CN"], "zh-TW"), "zh-TW");
  assert.equal(resolveLocale(["zh-TW"], "zh-CN"), "zh-CN");
});

test("an explicit supported preference overrides the browser list", () => {
  assert.equal(resolveLocale(["ko-KR"], "en"), "en");
  assert.equal(resolveLocale(["en-US"], "ja"), "ja");
  assert.equal(resolveLocale(["ja-JP"], "ko"), "ko");
  assert.equal(resolveLocale(["ja-JP"], "auto"), "ja");
});

test("saved preferences accept only exact supported values", () => {
  for (const value of SUPPORTED_LOCALES) {
    assert.equal(isLocale(value), true);
    assert.equal(parseLanguagePreference(value), value);
  }
  for (const value of [
    null,
    undefined,
    "",
    "auto",
    "nl",
    "EN",
    "ko-KR",
    "zh",
    "zh-cn",
    "zh-Hant",
    " ko ",
    1,
    {},
  ]) {
    assert.equal(isLocale(value), false);
    assert.equal(parseLanguagePreference(value), "auto");
  }
});

test("all fourteen supported languages have selectable names and persistent overrides", () => {
  assert.equal(SUPPORTED_LOCALES.length, 14);
  for (const locale of SUPPORTED_LOCALES) {
    assert.ok(LANGUAGE_NAMES[locale]?.trim(), locale);
    assert.equal(resolveLocale(["en-US"], locale), locale);
    const values = new Map();
    const storage = {
      getItem: (key) => values.get(key) ?? null,
      setItem: (key, value) => values.set(key, value),
      removeItem: (key) => values.delete(key),
    };
    assert.equal(persistLanguagePreference(storage, locale), true);
    assert.equal(readLanguagePreference(storage), locale);
  }
});

test("reading absent, invalid, or denied storage resumes automatic resolution", () => {
  assert.equal(readLanguagePreference(), "auto");
  assert.equal(readLanguagePreference({ getItem: () => null }), "auto");
  assert.equal(
    readLanguagePreference({ getItem: () => "unsupported" }),
    "auto",
  );
  assert.equal(
    readLanguagePreference({
      getItem: () => {
        throw new Error("denied");
      },
    }),
    "auto",
  );
  let requestedKey;
  assert.equal(
    readLanguagePreference({
      getItem: (key) => {
        requestedKey = key;
        return "ja";
      },
    }),
    "ja",
  );
  assert.equal(requestedKey, LANGUAGE_STORAGE_KEY);
});

test("explicit choices persist, while automatic mode removes only the language override", () => {
  const values = new Map([["other-setting", "preserved"]]);
  const storage = {
    setItem: (key, value) => values.set(key, value),
    removeItem: (key) => values.delete(key),
  };
  assert.equal(persistLanguagePreference(storage, "ko"), true);
  assert.equal(values.get(LANGUAGE_STORAGE_KEY), "ko");
  assert.equal(persistLanguagePreference(storage, "auto"), true);
  assert.equal(values.has(LANGUAGE_STORAGE_KEY), false);
  assert.equal(values.get("other-setting"), "preserved");
});

test("denied writes and removals cannot invalidate a session preference", () => {
  const storage = {
    setItem() {
      throw new Error("denied");
    },
    removeItem() {
      throw new Error("denied");
    },
  };
  assert.equal(persistLanguagePreference(undefined, "ja"), false);
  const preference = "ja";
  assert.equal(persistLanguagePreference(storage, preference), false);
  assert.equal(resolveLocale(["ko-KR"], preference), "ja");
  assert.equal(persistLanguagePreference(storage, "auto"), false);
  assert.equal(resolveLocale(["ko-KR"], "auto"), "ko");
});

test("translations interpolate zero counts and reorder named values naturally", () => {
  assert.equal(translate("en", "{count} tokens", { count: 0 }), "0 tokens");
  assert.equal(translate("ko", "{count} tokens", { count: 0 }), "토큰 0개");
  assert.equal(
    translate("ja", "{count} tokens", { count: 3 }),
    "3 個のトークン",
  );
  assert.equal(
    translate("ko", "{count} of {total} entries", { count: 2, total: 19 }),
    "전체 19개 중 2개 항목",
  );
  assert.equal(
    translate("en", "Go to step {step}", { step: 0 }),
    "Go to step 0",
  );
});

test("technical identifiers, diagnostics, and substitution metacharacters remain literal", () => {
  const raw = 'id := "[a-z]+"; {count} $& $\' $`';
  assert.equal(
    translate("ko", "Highlighted grammar source for {symbol}.", {
      symbol: raw,
    }),
    `${raw}에 대응하는 문법을 강조했습니다.`,
  );
  assert.equal(translate("ja", "shift"), "shift");
  assert.equal(
    translate("ko", "Unexpected token 'α'."),
    "Unexpected token 'α'.",
  );
  assert.equal(
    translate("ko", "Unknown {value}", { value: raw }),
    `Unknown ${raw}`,
  );
  assert.equal(translate("ja", "Unknown {value}"), "Unknown {value}");
  assert.equal(
    translate("ko", "Unknown {value}", Object.create({ value: "inherited" })),
    "Unknown {value}",
  );
  assert.equal(translate("ko", "toString"), "toString");
});

function placeholders(message) {
  return [...message.matchAll(/\{([a-zA-Z][a-zA-Z0-9_]*)\}/g)]
    .map((match) => match[1])
    .sort();
}

test("all thirteen translation catalogs cover every message and preserve its placeholders", () => {
  assert.ok(Object.keys(messages).length > 140);
  const keys = Object.keys(messages).sort();
  assert.equal(Object.keys(catalogs).length, SUPPORTED_LOCALES.length - 1);
  for (const locale of SUPPORTED_LOCALES.filter((locale) => locale !== "en")) {
    const catalog = catalogs[locale];
    assert.deepEqual(Object.keys(catalog).sort(), keys, locale);
    for (const [key, localized] of Object.entries(catalog)) {
      assert.ok(localized.trim().length > 0, `${locale}: ${key}`);
      assert.deepEqual(
        placeholders(localized),
        placeholders(key),
        `${locale}: ${key}`,
      );
    }
  }
});

test("every language interpolates symbols literally and retains raw engine diagnostics", () => {
  const symbol = "Expr {count} $& $' $`";
  for (const locale of SUPPORTED_LOCALES) {
    const localized = translate(locale, "Highlighted source for {symbol}.", {
      symbol,
    });
    assert.ok(localized.includes(symbol), locale);
    assert.equal(
      translate(locale, "Unexpected token 'α'."),
      "Unexpected token 'α'.",
      locale,
    );
    assert.equal(translate(locale, "toString"), "toString", locale);
  }
});

const frontendRoot = fileURLToPath(new URL("../", import.meta.url));

async function sourceFiles(directory) {
  const entries = await readdir(directory, { withFileTypes: true });
  const groups = await Promise.all(
    entries.map(async (entry) => {
      const location = path.join(directory, entry.name);
      if (entry.isDirectory()) return sourceFiles(location);
      return /\.(ts|tsx)$/.test(entry.name) ? [location] : [];
    }),
  );
  return groups.flat();
}

function literalBranches(expression) {
  if (!expression) return [];
  if (expression.type === "StringLiteral") return [expression.value];
  if (expression.type === "ConditionalExpression") {
    return [
      ...literalBranches(expression.consequent),
      ...literalBranches(expression.alternate),
    ];
  }
  return [];
}

function visit(node, callback) {
  if (!node || typeof node !== "object") return;
  if (Array.isArray(node)) {
    for (const child of node) visit(child, callback);
    return;
  }
  if (typeof node.type === "string") callback(node);
  for (const [key, value] of Object.entries(node)) {
    if (key !== "loc" && key !== "comments") visit(value, callback);
  }
}

test("all authored UI keys, example captions, navigation labels, and boot labels are translated", async () => {
  const authored = new Set();
  for (const location of await sourceFiles(path.join(frontendRoot, "src"))) {
    const source = await readFile(location, "utf8");
    const tree = parse(source, {
      sourceType: "module",
      plugins: ["typescript", "jsx"],
    });
    visit(tree, (node) => {
      if (node.type === "CallExpression" && node.callee.type === "Identifier") {
        if (node.callee.name === "t") {
          for (const key of literalBranches(node.arguments[0]))
            authored.add(key);
        } else if (node.callee.name === "translate") {
          for (const key of literalBranches(node.arguments[1]))
            authored.add(key);
        }
      }
      if (
        location.endsWith("playground.tsx") &&
        node.type === "ObjectProperty" &&
        node.value.type === "StringLiteral"
      ) {
        const name = node.key.name ?? node.key.value;
        if (["name", "description", "label"].includes(name))
          authored.add(node.value.value);
      }
    });
  }
  for (const relative of ["../wwwroot/index.html", "../Pages/Home.razor"]) {
    const source = await readFile(path.join(frontendRoot, relative), "utf8");
    for (const match of source.matchAll(/data-bootstrap-i18n="([^"]+)"/g))
      authored.add(match[1]);
  }
  const missing = [...authored].filter((key) => !Object.hasOwn(messages, key));
  assert.ok(
    authored.size > 140,
    `Unexpectedly small UI inventory: ${authored.size}`,
  );
  assert.deepEqual(missing, []);
});

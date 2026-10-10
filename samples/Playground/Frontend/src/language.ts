export const SUPPORTED_LOCALES = [
  "en",
  "ko",
  "ja",
  "ru",
  "zh-CN",
  "zh-TW",
  "uk",
  "vi",
  "de",
  "fr",
  "th",
  "es",
  "pt",
  "it",
] as const;
export type Locale = (typeof SUPPORTED_LOCALES)[number];
export type LanguagePreference = Locale | "auto";

export const LANGUAGE_STORAGE_KEY = "janglim-language";
export const LANGUAGE_NAMES: Readonly<Record<Locale, string>> = {
  en: "English",
  ko: "한국어",
  ja: "日本語",
  ru: "Русский",
  "zh-CN": "简体中文",
  "zh-TW": "繁體中文（台灣）",
  uk: "Українська",
  vi: "Tiếng Việt",
  de: "Deutsch",
  fr: "Français",
  th: "ไทย",
  es: "Español",
  pt: "Português",
  it: "Italiano",
};

export interface LanguageStorage {
  getItem(key: string): string | null;
  setItem(key: string, value: string): void;
  removeItem(key: string): void;
}

export function isLocale(value: unknown): value is Locale {
  return (
    typeof value === "string" &&
    (SUPPORTED_LOCALES as readonly string[]).includes(value)
  );
}

/** Only an explicitly supported saved value overrides browser preferences. */
export function parseLanguagePreference(value: unknown): LanguagePreference {
  return isLocale(value) ? value : "auto";
}

/** The first supported browser language wins, including regional/script tags. */
export function resolveLocale(
  browserLanguages: readonly string[] = [],
  preference: LanguagePreference = "auto",
): Locale {
  if (isLocale(preference)) return preference;
  for (const language of browserLanguages) {
    const tag = language.trim().toLowerCase();
    if (!/^[a-z]{2,3}(?:-[a-z0-9]+)*$/.test(tag)) continue;
    const subtags = tag.split("-");
    const locale = subtags[0];
    if (locale === "zh") {
      const script = subtags[1]?.length === 4 ? subtags[1] : undefined;
      const region = subtags[script ? 2 : 1];
      if (script === "hant") return "zh-TW";
      if (script === "hans") return "zh-CN";
      return region === "tw" || region === "hk" || region === "mo"
        ? "zh-TW"
        : "zh-CN";
    }
    if (isLocale(locale)) return locale;
  }
  return "en";
}

export function readLanguagePreference(
  storage?: Pick<LanguageStorage, "getItem">,
): LanguagePreference {
  try {
    return parseLanguagePreference(storage?.getItem(LANGUAGE_STORAGE_KEY));
  } catch {
    return "auto";
  }
}

/** A failed write never decides the active locale; the provider keeps session state. */
export function persistLanguagePreference(
  storage: Pick<LanguageStorage, "setItem" | "removeItem"> | undefined,
  preference: LanguagePreference,
): boolean {
  if (!storage) return false;
  try {
    if (isLocale(preference)) storage.setItem(LANGUAGE_STORAGE_KEY, preference);
    else storage.removeItem(LANGUAGE_STORAGE_KEY);
    return true;
  } catch {
    return false;
  }
}

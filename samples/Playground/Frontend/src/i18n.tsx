import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from "react";
import {
  LANGUAGE_STORAGE_KEY,
  parseLanguagePreference,
  persistLanguagePreference,
  readLanguagePreference,
  resolveLocale,
  type LanguagePreference,
  type LanguageStorage,
  type Locale,
} from "./language";
import { translate, type MessageValues } from "./messages";

export type { LanguagePreference, Locale } from "./language";

interface I18nContextValue {
  locale: Locale;
  preference: LanguagePreference;
  setPreference(preference: LanguagePreference): void;
  t(message: string, values?: MessageValues): string;
}

// Standalone component previews keep their existing English behavior.
const I18nContext = createContext<I18nContextValue>({
  locale: "en",
  preference: "auto",
  setPreference() {},
  t: (message, values) => translate("en", message, values),
});

function browserLanguages(): readonly string[] {
  if (typeof navigator === "undefined") return [];
  return navigator.languages?.length
    ? [...navigator.languages]
    : [navigator.language];
}

function browserStorage(): LanguageStorage | undefined {
  try {
    return typeof window === "undefined" ? undefined : window.localStorage;
  } catch {
    return undefined;
  }
}

export function I18nProvider({ children }: { children: ReactNode }) {
  const [preference, setStatePreference] = useState<LanguagePreference>(() =>
    readLanguagePreference(browserStorage()),
  );
  const [languages, setLanguages] = useState(browserLanguages);
  const currentPreference = useRef(preference);
  const locale = resolveLocale(languages, preference);

  const setPreference = useCallback((next: LanguagePreference) => {
    const validated = parseLanguagePreference(next);
    currentPreference.current = validated;
    setStatePreference(validated);
    if (validated === "auto") setLanguages(browserLanguages());
    // Storage denial never rolls back the user's active choice for this session.
    persistLanguagePreference(browserStorage(), validated);
  }, []);

  useEffect(() => {
    const onLanguageChange = () => {
      if (currentPreference.current === "auto")
        setLanguages(browserLanguages());
    };
    const onStorage = (event: StorageEvent) => {
      if (event.key !== null && event.key !== LANGUAGE_STORAGE_KEY) return;
      if (event.storageArea !== null && event.storageArea !== browserStorage())
        return;
      const next = parseLanguagePreference(
        event.key === null ? null : event.newValue,
      );
      currentPreference.current = next;
      setStatePreference(next);
      if (next === "auto") setLanguages(browserLanguages());
    };
    window.addEventListener("languagechange", onLanguageChange);
    window.addEventListener("storage", onStorage);
    return () => {
      window.removeEventListener("languagechange", onLanguageChange);
      window.removeEventListener("storage", onStorage);
    };
  }, []);

  useEffect(() => {
    document.documentElement.lang = locale;
    document.title = translate(
      locale,
      "Janglim Playground · Explore your grammar",
    );
    let description = document.querySelector<HTMLMetaElement>(
      'meta[name="description"]',
    );
    if (!description) {
      description = document.createElement("meta");
      description.name = "description";
      document.head.appendChild(description);
    }
    description.content = translate(
      locale,
      "An interactive EBNF and LALR parser playground. Visualize syntax trees and explore every parsing decision with Janglim.",
    );
  }, [locale]);

  const value = useMemo<I18nContextValue>(
    () => ({
      locale,
      preference,
      setPreference,
      t: (message, values) => translate(locale, message, values),
    }),
    [locale, preference, setPreference],
  );

  return <I18nContext.Provider value={value}>{children}</I18nContext.Provider>;
}

export function useI18n() {
  return useContext(I18nContext);
}

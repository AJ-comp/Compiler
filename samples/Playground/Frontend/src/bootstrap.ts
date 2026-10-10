import { isLocale, readLanguagePreference, resolveLocale } from "./language";
import { translate } from "./messages";

// Localize the first loading frame before Blazor or React is available, using
// the same language precedence and messages as the mounted application.
let initialPreference: ReturnType<typeof readLanguagePreference> = "auto";
try {
  initialPreference = readLanguagePreference(window.localStorage);
} catch {
  // Browsers may deny even accessing localStorage. Use their language list.
}
const initialLocale = resolveLocale(
  navigator.languages?.length ? navigator.languages : [navigator.language],
  initialPreference,
);
document.documentElement.lang = initialLocale;
document.title = translate(
  initialLocale,
  "Janglim Playground · Explore your grammar",
);
const description = document.querySelector('meta[name="description"]');
description?.setAttribute(
  "content",
  translate(
    initialLocale,
    "An interactive EBNF and LALR parser playground. Visualize syntax trees and explore every parsing decision with Janglim.",
  ),
);

function updateLoadingLabels() {
  const lang = document.documentElement.lang;
  const locale = isLocale(lang) ? lang : "en";
  document
    .querySelectorAll<HTMLElement>("[data-bootstrap-i18n]")
    .forEach((element) => {
      const key = element.dataset.bootstrapI18n;
      if (!key) return;
      const text = translate(locale, key);
      if (element.textContent !== text) element.textContent = text;
    });
}
updateLoadingLabels();
// Blazor briefly replaces the initial loading frame with its host placeholder.
// Only authored loading/error labels are touched; editor and React content aren't.
new MutationObserver(updateLoadingLabels).observe(document.documentElement, {
  childList: true,
  subtree: true,
  attributes: true,
  attributeFilter: ["lang"],
});

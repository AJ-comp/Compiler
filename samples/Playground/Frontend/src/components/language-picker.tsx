import { ChevronDown, Globe } from "lucide-react";
import { useI18n } from "../i18n";
import { isLocale, LANGUAGE_NAMES, SUPPORTED_LOCALES } from "../language";
import { Button } from "./ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuLabel,
  DropdownMenuRadioGroup,
  DropdownMenuRadioItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "./ui/dropdown-menu";

export function LanguagePicker() {
  const { locale, preference, setPreference, t } = useI18n();
  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button
          variant="outline"
          size="sm"
          className="language-trigger"
          aria-label={t("Language: {language}", {
            language: LANGUAGE_NAMES[locale],
          })}
        >
          <Globe size={15} aria-hidden="true" />
          <span className="language-current" lang={locale}>
            {LANGUAGE_NAMES[locale]}
          </span>
          <ChevronDown size={12} aria-hidden="true" />
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="language-menu">
        <DropdownMenuLabel>{t("Language")}</DropdownMenuLabel>
        <DropdownMenuSeparator />
        <DropdownMenuRadioGroup
          value={preference}
          onValueChange={(value) => {
            if (value === "auto" || isLocale(value)) setPreference(value);
          }}
          aria-label={t("Language")}
        >
          <DropdownMenuRadioItem value="auto">
            {t("Automatic (browser)")}
          </DropdownMenuRadioItem>
          {SUPPORTED_LOCALES.map((supported) => (
            <DropdownMenuRadioItem
              key={supported}
              value={supported}
              lang={supported}
            >
              {LANGUAGE_NAMES[supported]}
            </DropdownMenuRadioItem>
          ))}
        </DropdownMenuRadioGroup>
        {preference === "auto" && (
          <>
            <DropdownMenuSeparator />
            <p className="language-auto-note">
              {t("Following browser language")}
            </p>
          </>
        )}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}

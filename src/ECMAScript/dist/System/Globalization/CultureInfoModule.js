import { _23b9e8d671b5210e } from "System/Globalization/GregorianCalendarModule.js";
import { _5ad63706a889c294, _d8080c573d45b4b4 } from "System/StringModule.js";
const InvariantCultureName = "";
const InvariantCultureDisplayName = "Invariant Language (Invariant Country)";
const InvariantIetfLanguageTag = "";
const InvariantCultureByIetfName = "iv";
const InvariantThreeLetterIsoLanguageName = "ivl";
const InvariantThreeLetterWindowsLanguageName = "IVL";
function GetStringHashCode(text) {
  let hash = 0;
  for (let i = 0; i < text.length; i++)
    hash = (hash << 5) - hash + _5ad63706a889c294(text, i).charCodeAt(0);
  return hash | 0;
}
function IsNeutralCultureCore(instance) {
  let normalized = NormalizeCultureInfo(instance);
  if (normalized.length === 0)
    return false;
  let index = normalized.lastIndexOf("-");
  if (index < 0)
    return true;
  let last = normalized.substring(index + 1);
  if (last.length === 2)
    return false;
  if (last.length === 3) {
    for (let i = 0; i < 3; i++) {
      if (_5ad63706a889c294(last, i).charCodeAt(0) < "0".charCodeAt(0) || _5ad63706a889c294(last, i).charCodeAt(0) > "9".charCodeAt(0))
        return true;
    }
    return false;
  }
  return true;
}
function GetParentCulture(instance) {
  let normalized = NormalizeCultureInfo(instance);
  if (normalized.length === 0)
    return "";
  if (normalized === "iv")
    return "";
  let locale = new Intl.Locale(normalized);
  let language = locale.language;
  let script = locale.script ?? "";
  let region = locale.region ?? "";
  if (region !== null && region.length !== 0) {
    if (script !== null && script.length !== 0)
      return language + "-" + script;
    if (language === "zh") {
      let maximizedScript = locale.maximize().script ?? "";
      if (maximizedScript !== null && maximizedScript.length !== 0)
        return language + "-" + maximizedScript;
    }
    return language;
  }
  if (script !== null && script.length !== 0)
    return language;
  let index = normalized.lastIndexOf("-");
  if (index < 0)
    return "";
  return normalized.substring(0, 0 + index);
}
function GetLanguagePart(instance) {
  return instance.length === 0 ? "iv" : _d8080c573d45b4b4(instance, "-", 0)[0];
}
function GetIetfLanguageTag(instance) {
  return instance.length === 0 ? "" : instance;
}
function CreateLanguageDisplayNames(locale) {
  return new Intl.DisplayNames(locale, {
    type: "language",
    fallback: "code",
    languageDisplay: "dialect"
  });
}
function GetLocalizedCultureName(instance, displayLocale) {
  let normalized = NormalizeCultureInfo(instance);
  if (normalized.length === 0)
    return "Invariant Language (Invariant Country)";
  if (normalized === "iv")
    return "iv";
  try {
    let displayNames = CreateLanguageDisplayNames(displayLocale);
    let value = displayNames.of(normalized);
    return value ?? normalized;
  } catch {
    return normalized;
  }
}
function GetDisplayName(instance) {
  return GetLocalizedCultureName(instance, GetCurrentUICultureName());
}
function GetNativeName(instance) {
  let normalized = NormalizeCultureInfo(instance);
  return normalized.length === 0 ? "Invariant Language (Invariant Country)" : GetLocalizedCultureName(normalized, normalized);
}
function GetEnglishName(instance) {
  return GetLocalizedCultureName(instance, "en");
}
function GetThreeLetterIsoLanguageName(instance) {
  let normalized = NormalizeCultureInfo(instance);
  if (normalized.length === 0)
    return "ivl";
  if (normalized === "iv")
    return "";
  let language = GetLanguagePart(normalized);
  switch (language) {
    case "aa":
      return "aar";
    case "af":
      return "afr";
    case "ak":
      return "aka";
    case "am":
      return "amh";
    case "ar":
      return "ara";
    case "as":
      return "asm";
    case "az":
      return "aze";
    case "ba":
      return "bak";
    case "be":
      return "bel";
    case "bg":
      return "bul";
    case "bm":
      return "bam";
    case "bn":
      return "ben";
    case "bo":
      return "bod";
    case "br":
      return "bre";
    case "bs":
      return "bos";
    case "ca":
      return "cat";
    case "ce":
      return "che";
    case "co":
      return "cos";
    case "cs":
      return "ces";
    case "cu":
      return "chu";
    case "cv":
      return "chv";
    case "cy":
      return "cym";
    case "da":
      return "dan";
    case "de":
      return "deu";
    case "dv":
      return "div";
    case "dz":
      return "dzo";
    case "ee":
      return "ewe";
    case "el":
      return "ell";
    case "en":
      return "eng";
    case "eo":
      return "epo";
    case "es":
      return "spa";
    case "et":
      return "est";
    case "eu":
      return "eus";
    case "fa":
      return "fas";
    case "ff":
      return "ful";
    case "fi":
      return "fin";
    case "fo":
      return "fao";
    case "fr":
      return "fra";
    case "fy":
      return "fry";
    case "ga":
      return "gle";
    case "gd":
      return "gla";
    case "gl":
      return "glg";
    case "gn":
      return "grn";
    case "gu":
      return "guj";
    case "gv":
      return "glv";
    case "ha":
      return "hau";
    case "he":
      return "heb";
    case "hi":
      return "hin";
    case "hr":
      return "hrv";
    case "hu":
      return "hun";
    case "hy":
      return "hye";
    case "ia":
      return "ina";
    case "id":
      return "ind";
    case "ig":
      return "ibo";
    case "ii":
      return "iii";
    case "is":
      return "isl";
    case "it":
      return "ita";
    case "iu":
      return "iku";
    case "ja":
      return "jpn";
    case "jv":
      return "jav";
    case "ka":
      return "kat";
    case "ki":
      return "kik";
    case "kk":
      return "kaz";
    case "kl":
      return "kal";
    case "km":
      return "khm";
    case "kn":
      return "kan";
    case "ko":
      return "kor";
    case "kr":
      return "kau";
    case "ks":
      return "kas";
    case "kw":
      return "cor";
    case "ky":
      return "kir";
    case "la":
      return "lat";
    case "lb":
      return "ltz";
    case "lg":
      return "lug";
    case "ln":
      return "lin";
    case "lo":
      return "lao";
    case "lt":
      return "lit";
    case "lu":
      return "lub";
    case "lv":
      return "lav";
    case "mg":
      return "mlg";
    case "mi":
      return "mri";
    case "mk":
      return "mkd";
    case "ml":
      return "mal";
    case "mn":
      return "mon";
    case "mr":
      return "mar";
    case "ms":
      return "msa";
    case "mt":
      return "mlt";
    case "my":
      return "mya";
    case "nb":
      return "nob";
    case "nd":
      return "nde";
    case "ne":
      return "nep";
    case "nl":
      return "nld";
    case "nn":
      return "nno";
    case "no":
      return "nor";
    case "nr":
      return "nbl";
    case "oc":
      return "oci";
    case "om":
      return "orm";
    case "or":
      return "ori";
    case "os":
      return "oss";
    case "pa":
      return "pan";
    case "pl":
      return "pol";
    case "ps":
      return "pus";
    case "pt":
      return "por";
    case "qu":
      return "que";
    case "rm":
      return "roh";
    case "rn":
      return "run";
    case "ro":
      return "ron";
    case "ru":
      return "rus";
    case "rw":
      return "kin";
    case "sa":
      return "san";
    case "sc":
      return "srd";
    case "sd":
      return "snd";
    case "se":
      return "sme";
    case "sg":
      return "sag";
    case "si":
      return "sin";
    case "sk":
      return "slk";
    case "sl":
      return "slv";
    case "sn":
      return "sna";
    case "so":
      return "som";
    case "sq":
      return "sqi";
    case "sr":
      return "srp";
    case "ss":
      return "ssw";
    case "st":
      return "sot";
    case "su":
      return "sun";
    case "sv":
      return "swe";
    case "sw":
      return "swa";
    case "ta":
      return "tam";
    case "te":
      return "tel";
    case "tg":
      return "tgk";
    case "th":
      return "tha";
    case "ti":
      return "tir";
    case "tk":
      return "tuk";
    case "tn":
      return "tsn";
    case "to":
      return "ton";
    case "tr":
      return "tur";
    case "ts":
      return "tso";
    case "tt":
      return "tat";
    case "ug":
      return "uig";
    case "uk":
      return "ukr";
    case "ur":
      return "urd";
    case "uz":
      return "uzb";
    case "ve":
      return "ven";
    case "vi":
      return "vie";
    case "vo":
      return "vol";
    case "wo":
      return "wol";
    case "xh":
      return "xho";
    case "yi":
      return "yid";
    case "yo":
      return "yor";
    case "zh":
      return "zho";
    case "zu":
      return "zul";
    default:
      return language.length === 3 ? language : language;
  }
}
function GetThreeLetterWindowsLanguageName(instance) {
  let normalized = NormalizeCultureInfo(instance);
  if (normalized.length === 0)
    return "IVL";
  if (normalized === "iv")
    return "ZZZ";
  switch (normalized) {
    case "ar-SA":
      return "ARA";
    case "ar-EG":
      return "ARE";
    case "en":
    case "en-US":
      return "ENU";
    case "en-GB":
      return "ENG";
    case "nb-NO":
      return "NOR";
    case "nn-NO":
      return "NON";
    case "es-ES":
      return "ESN";
    case "es-MX":
      return "ESM";
    case "pt-PT":
      return "PTG";
    case "pt-BR":
      return "PTB";
    case "sv-SE":
      return "SVE";
    case "tr-TR":
      return "TRK";
    case "pl-PL":
      return "PLK";
    case "cs-CZ":
      return "CSY";
    case "zh":
    case "zh-CN":
    case "zh-Hans":
      return "CHS";
    case "zh-TW":
      return "CHT";
    case "zh-HK":
      return "ZHH";
    case "sr":
      return "SRB";
    case "sr-Cyrl":
    case "sr-Cyrl-RS":
      return "SRO";
    case "sr-Latn":
    case "sr-Latn-RS":
      return "SRM";
    case "az-Cyrl":
    case "az-Cyrl-AZ":
      return "AZC";
    case "az":
    case "az-Latn":
    case "az-Latn-AZ":
      return "AZE";
    case "bs":
    case "bs-Latn":
    case "bs-Latn-BA":
      return "BSB";
    case "bs-Cyrl":
    case "bs-Cyrl-BA":
      return "BSC";
  }
  return GetThreeLetterIsoLanguageName(normalized).toUpperCase();
}
function CreateSpecificCulture(name) {
  if (name.length === 0)
    return "";
  if (name === "iv")
    return "";
  let supported = Intl.DateTimeFormat.supportedLocalesOf(name);
  if (supported.length === 0)
    throw new Error(`CultureNotFoundException: Culture '${name ?? ""}' is not supported.`);
  let resolved = supported[0];
  let locale = new Intl.Locale(resolved).maximize();
  let language = locale.language;
  let region = locale.region ?? "";
  let script = locale.script ?? "";
  if (region === null || region.length === 0)
    return resolved;
  if (resolved === "zh" || resolved === "zh-Hans")
    return "zh-CN";
  if (resolved === "zh-Hant")
    return "zh-HK";
  if (script === null || script.length === 0)
    return language + "-" + region;
  if (language === "zh")
    return language + "-" + region;
  if (script === "Latn" && language !== "az" && language !== "uz" && language !== "sr")
    return language + "-" + region;
  return language + "-" + script + "-" + region;
}
function CreateCultureInfo_7095ad00d05c5a6b(name) {
  if (name.length === 0)
    return "";
  if (name === "iv")
    return "iv";
  let supported = Intl.DateTimeFormat.supportedLocalesOf(name);
  if (supported.length === 0)
    throw new Error(`CultureNotFoundException: Culture '${name ?? ""}' is not supported.`);
  return supported[0];
}
function CreateCultureInfo_aa6c757bb07bc9a7(culture) {
  throw new Error(`NotSupportedException: LCID-based CultureInfo constructors are not supported: ${culture.toString()}.`);
}
function GetCultureInfoByIetfLanguageTag(name) {
  return CreateCultureInfo_7095ad00d05c5a6b(name);
}
function NormalizeCultureInfo(instance) {
  return instance.length === 0 ? "" : CreateCultureInfo_7095ad00d05c5a6b(instance);
}
function GetCurrentCultureName() {
  return CreateCultureInfo_7095ad00d05c5a6b((new Intl.NumberFormat).resolvedOptions().locale);
}
function GetCurrentUICultureName() {
  try {
    return CreateCultureInfo_7095ad00d05c5a6b((new Intl.NumberFormat).resolvedOptions().locale);
  } catch {
    return GetCurrentCultureName();
  }
}
/*jazor:clr-member System.Globalization.CultureInfo.CultureInfo(string)*/
export function _b7486264ae338f27(name) {
  return CreateCultureInfo_7095ad00d05c5a6b(name);
}
/*jazor:clr-member System.Globalization.CultureInfo.CultureInfo(string, bool)*/
export function _df21a93fd9f84197(name, useUserOverride) {
  return CreateCultureInfo_7095ad00d05c5a6b(name);
}
/*jazor:clr-member System.Globalization.CultureInfo.CultureInfo(int)*/
export function _22aaac09e253b1f9(culture) {
  return CreateCultureInfo_aa6c757bb07bc9a7(culture);
}
/*jazor:clr-member System.Globalization.CultureInfo.CultureInfo(int, bool)*/
export function _d0948ef9f698ec85(culture, useUserOverride) {
  return CreateCultureInfo_aa6c757bb07bc9a7(culture);
}
/*jazor:clr-member static System.Globalization.CultureInfo.CreateSpecificCulture(string)*/
export function _a078d5ccbbf2345a(name) {
  return CreateSpecificCulture(name);
}
/*jazor:clr-member static System.Globalization.CultureInfo.CurrentCulture.get*/
export function _1a26e2e2e4e0ca1d() {
  return GetCurrentCultureName();
}
/*jazor:clr-member static System.Globalization.CultureInfo.CurrentUICulture.get*/
export function _eca32c250ead7de9() {
  return GetCurrentUICultureName();
}
/*jazor:clr-member static System.Globalization.CultureInfo.InstalledUICulture.get*/
export function _98e743867688a06d() {
  return GetCurrentUICultureName();
}
/*jazor:clr-member static System.Globalization.CultureInfo.InvariantCulture.get*/
export function _e4c4d53d69e72382() {
  return "";
}
/*jazor:clr-member virtual System.Globalization.CultureInfo.Parent.get*/
export function _cd29576576563da3(instance) {
  return GetParentCulture(instance);
}
/*jazor:clr-member System.Globalization.CultureInfo.IetfLanguageTag.get*/
export function _9c9f6e469362911e(instance) {
  return GetIetfLanguageTag(instance);
}
/*jazor:clr-member virtual System.Globalization.CultureInfo.DisplayName.get*/
export function _59b041331098ad55(instance) {
  return GetDisplayName(instance);
}
/*jazor:clr-member virtual System.Globalization.CultureInfo.NativeName.get*/
export function _a4804f687bfc0013(instance) {
  return GetNativeName(instance);
}
/*jazor:clr-member virtual System.Globalization.CultureInfo.EnglishName.get*/
export function _97ad9637d1f75e7c(instance) {
  return GetEnglishName(instance);
}
/*jazor:clr-member virtual System.Globalization.CultureInfo.TwoLetterISOLanguageName.get*/
export function _112fba1dc945fa1a(instance) {
  return GetLanguagePart(instance);
}
/*jazor:clr-member virtual System.Globalization.CultureInfo.ThreeLetterISOLanguageName.get*/
export function _285ede13a469ce7b(instance) {
  return GetThreeLetterIsoLanguageName(instance);
}
/*jazor:clr-member virtual System.Globalization.CultureInfo.ThreeLetterWindowsLanguageName.get*/
export function _1f981ccac713f3d9(instance) {
  return GetThreeLetterWindowsLanguageName(instance);
}
/*jazor:clr-member override System.Globalization.CultureInfo.Equals(object)*/
export function _dfe1a8cc1c9e5e52(instance, value) {
  let other = typeof value === "string" ? value : null;
  if (other === null)
    return false;
  try {
    return NormalizeCultureInfo(instance) === NormalizeCultureInfo(other);
  } catch {
    return false;
  }
}
/*jazor:clr-member override System.Globalization.CultureInfo.GetHashCode()*/
export function _b3aae6e43cf38d8a(instance) {
  return GetStringHashCode(NormalizeCultureInfo(instance));
}
/*jazor:clr-member override System.Globalization.CultureInfo.ToString()*/
export function _559b27327f84f1af(instance) {
  return NormalizeCultureInfo(instance);
}
/*jazor:clr-member virtual System.Globalization.CultureInfo.IsNeutralCulture.get*/
export function _0bedb111138c14ed(instance) {
  return IsNeutralCultureCore(instance);
}
/*jazor:clr-member static System.Globalization.CultureInfo.ClearCachedData()*/
export function _73e163fe0d6f4c41() { }
/*jazor:clr-member virtual System.Globalization.CultureInfo.Calendar.get*/
export function _2ab4f6aaba1be337(instance) {
  return _23b9e8d671b5210e();
}
/*jazor:clr-member virtual System.Globalization.CultureInfo.OptionalCalendars.get*/
export function _5031598284c711b5(instance) {
  return [_23b9e8d671b5210e()];
}
/*jazor:clr-member System.Globalization.CultureInfo.UseUserOverride.get*/
export function _4b6ab04957c3b1d8(instance) {
  return false;
}
/*jazor:clr-member System.Globalization.CultureInfo.GetConsoleFallbackUICulture()*/
export function _e746a9049464da41(instance) {
  let normalized = NormalizeCultureInfo(instance);
  return normalized === "iv" ? "" : normalized;
}
/*jazor:clr-member virtual System.Globalization.CultureInfo.Clone()*/
export function _52d3a5ff068445a1(instance) {
  return instance;
}
/*jazor:clr-member static System.Globalization.CultureInfo.ReadOnly(System.Globalization.CultureInfo)*/
export function _f3218a923929edaf(ci) {
  return ci;
}
/*jazor:clr-member System.Globalization.CultureInfo.IsReadOnly.get*/
export function _1a2fc3e83feec6fd(instance) {
  return true;
}
/*jazor:clr-member static System.Globalization.CultureInfo.GetCultureInfo(int)*/
export function _be269d85f3085630(culture) {
  return CreateCultureInfo_aa6c757bb07bc9a7(culture);
}
/*jazor:clr-member static System.Globalization.CultureInfo.GetCultureInfo(string)*/
export function _a536c354b66082b9(name) {
  return CreateCultureInfo_7095ad00d05c5a6b(name);
}
/*jazor:clr-member static System.Globalization.CultureInfo.GetCultureInfo(string, string)*/
export function _e17d240a4c1653be(name, altName) {
  CreateCultureInfo_7095ad00d05c5a6b(altName);
  return CreateCultureInfo_7095ad00d05c5a6b(name);
}
/*jazor:clr-member static System.Globalization.CultureInfo.GetCultureInfo(string, bool)*/
export function _a43a2bb07ef29293(name, predefinedOnly) {
  return CreateCultureInfo_7095ad00d05c5a6b(name);
}
/*jazor:clr-member static System.Globalization.CultureInfo.GetCultureInfoByIetfLanguageTag(string)*/
export function _1d57f4ce6dee8a81(name) {
  return GetCultureInfoByIetfLanguageTag(name);
}

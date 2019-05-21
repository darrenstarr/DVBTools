namespace DVBToolsCommon
{
    public class ISO639Table
    {
        public static string V1toV2(string v1)
        {
            for (int i = 0; i < V1toV2Table.Length; i += 2)
            {
                if (V1toV2Table[i] == v1)
                    return V1toV2Table[i + 1];
            }
            return "Unk";
        }

        public static string V1ToLanguage(string v1)
        {
            for (int i = 0; i < V1toV2Table.Length; i += 2)
            {
                if (V1toV2Table[i] == v1)
                    return V2ToLanguage(V1toV2Table[i + 1]);
            }
            return "Unknown";
        }

        public static string V2ToLanguage(string v2)
        {
            for (int i = 0; i < LanguageCodes.Length; i += 2)
            {
                if (LanguageCodes[i] == v2)
                    return LanguageCodes[i + 1];
            }
            return "Unknown";
        }

        public static bool IsV2Entry(string v2)
        {
            for (int i = 0; i < LanguageCodes.Length; i += 2)
            {
                if (LanguageCodes[i] == v2)
                    return true;
            }
            return false;
        }

        public static string[] V1toV2Table = 
        {
            "aa",                   // Afar
            "aar",
            "ab",                  // Abkhazian
            "abk",
            "af",                  // Afrikaans
            "afr",
            "ak",                  // Akan
            "aka",
            "sq",                  // Albanian
            "alb",
            "am",                  // Amharic
            "amh",
            "ar",                  // Arabic
            "ara",
            "an",                  // Aragonese
            "arg",
            "hy",                  // Armenian
            "arm",
            "as",                  // Assamese
            "asm",
            "av",                  // Avaric
            "ava",
            "ae",                  // Avestan
            "ave",
            "ay",                  // Aymara
            "aym",
            "az",                  // Azerbaijani
            "aze",
            "ba",                  // Bashkir
            "bak",
            "bm",                  // Bambara
            "bam",
            "eu",                  // Basque
            "baq",
            "be",                  // Belarusian
            "bel",
            "bn",                  // Bengali
            "ben",
            "bh",                  // Bihari
            "bih",
            "bi",                  // Bislama
            "bis",
            "bo",                  // Tibetan
            "tib",
            "bs",                  // Bosnian
            "bos",
            "br",                  // Breton
            "bre",
            "bg",                  // Bulgarian
            "bul",
            "my",                  // Burmese
            "bur",
            "ca",                  // Catalan, Valencian
            "cat",
            "cs",                  // Czech
            "cze",
            "ch",                  // Chamorro
            "cha",
            "ce",                  // Chechen
            "che",
            "zh",                  // Chinese
            "chi",
            "cu",                  // Church Slavic
            "chu",
            "cv",                  // Chuvash
            "chv",
            "kw",                  // Cornish
            "cor",
            "co",                  // Corsican
            "cos",
            "cr",                  // Cree
            "cre",
            "cy",                  // Welsh
            "wel",
            "da",                  // Danish
            "dan",
            "de",                  // German
            "ger",
            "dv",                  // Divehi
            "div",
            "nl",                  // Dutch, Flemish
            "dut",
            "dz",                  // Dzongkha
            "dzo",
            "el",                  // Greek, Modern (1453-)
            "gre",
            "en",                  // English
            "eng",
            "eo",                  // Esperanto
            "epo",
            "et",                  // Estonian
            "est",
            "ee",                  // Ewe
            "ewe",
            "fo",                  // Faroese
            "fao",
            "fa",                  // Persian
            "per",
            "fj",                  // Fijian
            "fij",
            "fi",                  // Finnish
            "fin",
            "fr",                  // French
            "fre",
            "fy",                  // Western Frisian
            "fry",
            "ff",                  // Fulah
            "ful",
            "ka",                  // Georgian
            "geo",
            "gd",                  // Gaelic, Scottish Gaelic
            "gla",
            "ga",                  // Irish
            "gle",
            "gl",                  // Galician
            "glg",
            "gv",                  // Manx
            "glv",
            "gn",                  // Guarani
            "grn",
            "gu",                  // Gujarati
            "guj",
            "ht",                  // Haitian, Haitian Creole
            "hat",
            "ha",                  // Hausa
            "hau",
            "he",                  // Hebrew
            "heb",
            "hz",                  // Herero
            "her",
            "hi",                  // Hindi
            "hin",
            "ho",                  // Hiri Motu
            "hmo",
            "hr",                  // Croatian
            "scr",
            "hu",                  // Hungarian
            "hun",
            "ig",                  // Igbo
            "ibo",
            "is",                  // Icelandic
            "ice",
            "io",                  // Ido
            "ido",
            "ii",                  // Sichuan Yi
            "iii",
            "iu",                  // Inuktitut
            "iku",
            "ie",                  // Interlingue
            "ile",
            "ia",                  // Interlingua (International Auxiliary Language Association)
            "ina",
            "id",                  // Indonesian
            "ind",
            "ik",                  // Inupiaq
            "ipk",
            "it",                  // Italian
            "ita",
            "jv",                  // Javanese
            "jav",
            "ja",                  // Japanese
            "jpn",
            "kl",                  // Kalaallisut, Greenlandic
            "kal",
            "kn",                  // Kannada
            "kan",
            "ks",                  // Kashmiri
            "kas",
            "kr",                  // Kanuri
            "kau",
            "kk",                  // Kazakh
            "kaz",
            "km",                  // Central Khmer
            "khm",
            "ki",                  // Kikuyu, Gikuyu
            "kik",
            "rw",                  // Kinyarwanda
            "kin",
            "ky",                  // Kirghiz, Kyrgyz
            "kir",
            "kv",                  // Komi
            "kom",
            "kg",                  // Kongo
            "kon",
            "ko",                  // Korean
            "kor",
            "kj",                  // Kuanyama, Kwanyama
            "kua",
            "ku",                  // Kurdish
            "kur",
            "lo",                  // Lao
            "lao",
            "la",                  // Latin
            "lat",
            "lv",                  // Latvian
            "lav",
            "li",                  // Limburgan, Limburger, Limburgish
            "lim",
            "ln",                  // Lingala
            "lin",
            "lt",                  // Lithuanian
            "lit",
            "lb",                  // Luxembourgish, Letzeburgesch
            "ltz",
            "lu",                  // Luba-Katanga
            "lub",
            "lg",                  // Ganda
            "lug",
            "mk",                  // Macedonian
            "mac",
            "mh",                  // Marshallese
            "mah",
            "ml",                  // Malayalam
            "mal",
            "mi",                  // Maori
            "mao",
            "mr",                  // Marathi
            "mar",
            "ms",                  // Malay
            "may",
            "mg",                  // Malagasy
            "mlg",
            "mt",                  // Maltese
            "mlt",
            "mo",                  // Moldavian
            "mol",
            "mn",                  // Mongolian
            "mon",
            "na",                  // Nauru
            "nau",
            "nv",                  // Navajo, Navaho
            "nav",
            "nr",                  // Ndebele, South, South Ndebele
            "nbl",
            "nd",                  // Ndebele, North, North Ndebele
            "nde",
            "ng",                  // Ndonga
            "ndo",
            "ne",                  // Nepali
            "nep",
            "nn",                  // Norwegian Nynorsk, Nynorsk, Norwegian
            "nno",
            "nb",                  // Bokmål, Norwegian, Norwegian Bokmål
            "nob",
            "no",                  // Norwegian
            "nor",
            "ny",                  // Chichewa, Chewa, Nyanja
            "nya",
            "oc",                  // Occitan (post 1500), Provençal
            "oci",
            "oj",                  // Ojibwa
            "oji",
            "or",                  // Oriya
            "ori",
            "om",                  // Oromo
            "orm",
            "os",                  // Ossetian, Ossetic
            "oss",
            "pa",                  // Panjabi, Punjabi
            "pan",
            "pi",                  // Pali
            "pli",
            "pl",                  // Polish
            "pol",
            "pt",                  // Portuguese
            "por",
            "ps",                  // Pushto
            "pus",
            "qu",                  // Quechua
            "que",
            "rm",                  // Romansh
            "roh",
            "ro",                  // Romanian
            "rum",
            "rn",                  // Rundi
            "run",
            "ru",                  // Russian
            "rus",
            "sg",                  // Sango
            "sag",
            "sa",                  // Sanskrit
            "san",
            "sr",                  // Serbian
            "scc",
            "si",                  // Sinhala, Sinhalese
            "sin",
            "sk",                  // Slovak
            "slo",
            "sl",                  // Slovenian
            "slv",
            "se",                  // Northern Sami
            "sme",
            "sm",                  // Samoan
            "smo",
            "sn",                  // Shona
            "sna",
            "sd",                  // Sindhi
            "snd",
            "so",                  // Somali
            "som",
            "st",                  // Sotho, Southern
            "sot",
            "es",                  // Spanish, Castilian
            "spa",
            "sc",                  // Sardinian
            "srd",
            "ss",                  // Swati
            "ssw",
            "su",                  // Sundanese
            "sun",
            "sw",                  // Swahili
            "swa",
            "sv",                  // Swedish
            "swe",
            "ty",                  // Tahitian
            "tah",
            "ta",                  // Tamil
            "tam",
            "tt",                  // Tatar
            "tat",
            "te",                  // Telugu
            "tel",
            "tg",                  // Tajik
            "tgk",
            "tl",                  // Tagalog
            "tgl",
            "th",                  // Thai
            "tha",
            "ti",                  // Tigrinya
            "tir",
            "to",                  // Tonga (Tonga Islands)
            "ton",
            "tn",                  // Tswana
            "tsn",
            "ts",                  // Tsonga
            "tso",
            "tk",                  // Turkmen
            "tuk",
            "tr",                  // Turkish
            "tur",
            "tw",                  // Twi
            "twi",
            "ug",                  // Uighur, Uyghur
            "uig",
            "uk",                  // Ukrainian
            "ukr",
            "ur",                  // Urdu
            "urd",
            "uz",                  // Uzbek
            "uzb",
            "ve",                  // Venda
            "ven",
            "vi",                  // Vietnamese
            "vie",
            "vo",                  // Volapük
            "vol",
            "wa",                  // Walloon
            "wln",
            "wo",                  // Wolof
            "wol",
            "xh",                  // Xhosa
            "xho",
            "yi",                  // Yiddish
            "yid",
            "za",                  // Zhuang, Chuang
            "zha",
            "zu",                  // Zulu
            "zul",
        };

        public static string[] LanguageCodes =
        {
        	"abk", "Abkhazian",
			"ace", "Achinese",
			"ach", "Acoli",
			"ada", "Adangme",
			"aar", "Afar",
			"afh", "Afrihili",
			"afr", "Afrikaans",
			"afa", "Afro-Asiatic (Other)",
			"aka", "Akan",
			"akk", "Akkadian",
			"alb", "Albanian",
			"ale", "Aleut",
			"alg", "Algonquian languages",
			"tut", "Altaic (Other)",
			"amh", "Amharic",
			"apa", "Apache languages",
			"ara", "Arabic",
			"arc", "Aramaic",
			"arp", "Arapaho",
			"arn", "Araucanian",
			"arw", "Arawak",
			"arm", "Armenian",
			"art", "Artificial (Other)",
			"asm", "Assamese",
			"ast", "Asturian, Bable",
			"ath", "Athapascan languages",
			"aus", "Australian languages",
			"map", "Austronesian (Other)",
			"ava", "Avaric",
			"ave", "Avestan",
			"awa", "Awadhi",
			"aym", "Aymara",
			"aze", "Azerbaijani",
			"ban", "Balinese",
			"bat", "Baltic (Other)",
			"bal", "Baluchi",
			"bam", "Bambara",
			"bai", "Bamileke languages",
			"bad", "Banda",
			"bnt", "Bantu (Other)",
			"bas", "Basa",
			"bak", "Bashkir",
			"baq", "Basque",
			"btk", "Batak (Indonesia)",
			"bej", "Beja",
			"bel", "Belarusian",
			"bem", "Bemba",
			"ben", "Bengali",
			"ber", "Berber (Other)",
			"bho", "Bhojpuri",
			"bih", "Bihari",
			"bik", "Bikol",
			"bin", "Bini",
			"bis", "Bislama;",
			"nob", "Bokmål, Norwegian, Norwegian Bokmål",
			"bos", "Bosnian",
			"bra", "Braj",
			"bre", "Breton",
			"bug", "Buginese",
			"bul", "Bulgarian",
			"bua", "Buriat",
			"bur", "Burmese",
			"cad", "Caddo",
			"car", "Carib",
			"spa", "Castilian, Spanish",
			"cat", "Catalan",
			"cau", "Caucasian (Other)",
			"ceb", "Cebuano",
			"cel", "Celtic (Other)",
			"cai", "Central American Indian (Other)",
            "chg", "Chagatai",
            "cmc", "Chamic languages",
            "cha", "Chamorro",
            "che", "Chechen",
            "chr", "Cherokee",
            "nya", "Chewa, Chichewa, Nyanja",
            "chy", "Cheyenne",
            "chb", "Chibcha",
            "chi", "Chinese",
            "chn", "Chinook jargon",
            "chp", "Chipewyan",
            "cho", "Choctaw",
            "zha", "Chuang, Zhuang",
            "chu", "Church Slavic",
            "chk", "Chuukese",
            "chv", "Chuvash",
            "cop", "Coptic",
            "cor", "Cornish",
            "cos", "Corsican",
            "cre", "Cree",
            "mus", "Creek",
            "crp", "Creoles and pidgins(Other)",
            "cpe", "Creoles and pidgins, English-based (Other)",
            "cpf", "Creoles and pidgins, French-based (Other)",
            "cpp", "Creoles and pidgins, Portuguese-based (Other)",
            "scr", "Croatian",
            "cus", "Cushitic (Other)",
            "cze", "Czech",
            "dak", "Dakota",
            "dan", "Danish",
            "day", "Dayak",
            "del", "Delaware",
            "din", "Dinka",
            "div", "Divehi",
            "doi", "Dogri",
            "dgr", "Dogrib",
            "dra", "Dravidian (Other)",
            "dua", "Duala",
            "dut", "Dutch",
            "dum", "Dutch, Middle (ca. 1050-1350)",
            "dyu", "Dyula",
            "dzo", "Dzongkha",
            "efi", "Efik",
            "egy", "Egyptian (Ancient)",
            "eka", "Ekajuk",
            "elx", "Elamite",
            "eng", "English",
            "enm", "English, Middle (1100-1500)",
            "ang", "English, Old (ca.450-1100)",
            "epo", "Esperanto",
            "est", "Estonian",
            "ewe", "Ewe",
            "ewo", "Ewondo",
            "fan", "Fang",
            "fat", "Fanti",
            "fao", "Faroese",
            "fij", "Fijian",
            "fin", "Finnish",
            "fiu", "Finno-Ugrian (Other)",
            "fon", "Fon",
            "fre", "French",
            "frm", "French, Middle (ca.1400-1600)",
            "fro", "French, Old (842-ca.1400)",
            "fry", "Frisian",
            "fur", "Friulian",
            "ful", "Fulah",
            "gaa", "Ga",
            "gla", "Gaelic, Scottish Gaelic",
            "glg", "Gallegan",
            "lug", "Ganda",
            "gay", "Gayo",
            "gba", "Gbaya",
            "gez", "Geez",
            "geo", "Georgian",
            "ger", "German",
            "nds", "German, Low; Saxon, Low; Low German; Low Saxon",
            "gmh", "German, Middle High (ca.1050-1500)",
            "goh", "German, Old High (ca.750-1050)",
            "gem", "Germanic (Other)",
            "kik", "Gikuyu, Kikuyu",
            "gil", "Gilbertese",
            "gon", "Gondi",
            "gor", "Gorontalo",
            "got", "Gothic",
            "grb", "Grebo",
            "grc", "Greek, Ancient (to 1453)",
            "gre", "Greek, Modern (1453-)",
            "grn", "Guarani",
            "guj", "Gujarati",
            "gwi", "GwichÂ´in",
            "hai", "Haida",
            "hau", "Hausa",
            "haw", "Hawaiian",
            "heb", "Hebrew",
            "her", "Herero",
            "hil", "Hiligaynon",
            "him", "Himachali",
            "hin", "Hindi",
            "hmo", "Hiri Motu",
            "hit", "Hittite",
            "hmn", "Hmong",
            "hun", "Hungarian",
            "hup", "Hupa",
            "iba", "Iban",
            "ice", "Icelandic",
            "ido", "Ido",
            "ibo", "Igbo",
            "ijo", "Ijo",
            "ilo", "Iloko",
            "smn", "Inari Sami",
            "inc", "Indic (Other)",
            "ine", "Indo-European (Other)",
            "ind", "Indonesian",
            "ina", "Interlingua",
            "ile", "Interlingue",
            "iku", "Inuktitut",
            "ipk", "Inupiaq",
            "ira", "Iranian (Other)",
            "gle", "Irish",
            "mga", "Irish, Middle (900-1200)",
            "sga", "Irish, Old (to 900)",
            "iro", "Iroquoian languages",
            "ita", "Italian",
            "jpn", "Japanese",
            "jav", "Javanese",
            "jrb", "Judeo-Arabic",
            "jpr", "Judeo-Persian",
            "kab", "Kabyle",
            "kac", "Kachin",
            "kal", "Kalaallisut",
            "kam", "Kamba",
            "kan", "Kannada",
            "kau", "Kanuri",
            "kaa", "Kara-Kalpak",
            "kar", "Karen",
            "kas", "Kashmiri",
            "kaw", "Kawi",
            "kaz", "Kazakh",
            "kha", "Khasi",
            "khm", "Khmer",
            "khi", "Khoisan (Other)",
            "kho", "Khotanese",
            "kmb", "Kimbundu",
            "kin", "Kinyarwanda",
            "kir", "Kirghiz",
            "kom", "Komi",
            "kon", "Kongo",
            "kok", "Konkani",
            "kor", "Korean",
            "kos", "Kosraean",
            "kpe", "Kpelle",
            "kro", "Kru",
            "kua", "Kuanyama, Kwanyama",
            "kum", "Kumyk",
            "kur", "Kurdish",
            "kru", "Kurukh",
            "kut", "Kutenai",
            "lad", "Ladino",
            "lah", "Lahnda",
            "lam", "Lamba",
            "lao", "Lao",
            "lat", "Latin",
            "lav", "Latvian",
            "ltz", "Letzeburgesch, Luxembourgish",
            "lez", "Lezghian",
            "lin", "Lingala",
            "lit", "Lithuanian",
            "loz", "Lozi",
            "lub", "Luba-Katanga",
            "lua", "Luba-Lulua",
            "lui", "Luiseno",
            "smj", "Lule Sami",
            "lun", "Lunda",
            "luo", "Luo (Kenya and Tanzania)",
            "lus", "Lushai",
            "mac", "Macedonian",
            "mad", "Madurese",
            "mag", "Magahi",
            "mai", "Maithili",
            "mak", "Makasar",
            "mlg", "Malagasy",
            "may", "Malay",
            "mal", "Malayalam",
            "mlt", "Maltese",
            "mnc", "Manchu",
            "mdr", "Mandar",
            "man", "Mandingo",
            "mni", "Manipuri",
            "mno", "Manobo languages",
            "glv", "Manx",
            "mao", "Maori",
            "mar", "Marathi",
            "chm", "Mari",
            "mah", "Marshallese",
            "mwr", "Marwari",
            "mas", "Masai",
            "myn", "Mayan languages",
            "men", "Mende",
            "mic", "Micmac",
            "min", "Minangkabau",
            "mis", "Miscellaneous languages",
            "moh", "Mohawk",
            "mol", "Moldavian",
            "mkh", "Mon-Khmer (Other)",
            "lol", "Mongo",
            "mon", "Mongolian",
            "mos", "Mossi",
            "mul", "Multiple languages",
            "mun", "Munda languages",
            "nah", "Nahuatl",
            "nau", "Nauru",
            "nav", "Navaho, Navajo",
            "nde", "Ndebele, North; North Ndebele",
            "nbl", "Ndebele, South; South Ndebele",
            "ndo", "Ndonga",
            "nep", "Nepali",
            "new", "Newari",
            "nia", "Nias",
            "nic", "Niger-Kordofanian (Other)",
            "ssa", "Nilo-Saharan (Other)",
            "niu", "Niuean",
            "non", "Norse, Old",
            "nai", "North American Indian (Other)",
            "sme", "Northern Sami",
            "nor", "Norwegian",
            "nno", "Norwegian Nynorsk; Nynorsk, Norwegian",
            "nub", "Nubian languages",
            "nym", "Nyamwezi",
            "nyn", "Nyankole",
            "nyo", "Nyoro",
            "nzi", "Nzima",
            "oci", "Occitan (post 1500), ProvenÃ§al",
            "oji", "Ojibwa",
            "ori", "Oriya",
            "orm", "Oromo",
            "osa", "Osage",
            "oss", "Ossetian, Ossetic",
            "oto", "Otomian languages",
            "pal", "Pahlavi",
            "pau", "Palauan",
            "pli", "Pali",
            "pam", "Pampanga",
            "pag", "Pangasinan",
            "pan", "Panjabi",
            "pap", "Papiamento",
            "paa", "Papuan (Other)",
            "per", "Persian",
            "peo", "Persian, Old (ca.600-400)",
            "phi", "Philippine (Other)",
            "phn", "Phoenician",
            "pon", "Pohnpeian",
            "pol", "Polish",
            "por", "Portuguese",
            "pra", "Prakrit languages",
            "pro", "ProvenÃ§al, Old (to 1500)",
            "pus", "Pushto",
            "que", "Quechua",
            "roh", "Raeto-Romance",
            "raj", "Rajasthani",
            "rap", "Rapanui",
            "rar", "Rarotongan",
            "roa", "Romance (Other)",
            "rum", "Romanian",
            "rom", "Romany",
            "run", "Rundi",
            "rus", "Russian",
            "sal", "Salishan languages",
            "sam", "Samaritan Aramaic",
            "smi", "Sami languages (Other)",
            "smo", "Samoan",
            "sad", "Sandawe",
            "sag", "Sango",
            "san", "Sanskrit",
            "sat", "Santali",
            "srd", "Sardinian",
            "sas", "Sasak",
            "sco", "Scots",
            "sel", "Selkup",
            "sem", "Semitic (Other)",
            "scc", "Serbian",
            "srr", "Serer",
            "shn", "Shan",
            "sna", "Shona",
            "sid", "Sidamo",
            "sgn", "Sign languages",
            "bla", "Siksika",
            "snd", "Sindhi",
            "sin", "Sinhalese",
            "sit", "Sino-Tibetan (Other)",
            "sio", "Siouan languages",
            "sms", "Skolt Sami",
            "den", "Slave (Athapascan)",
            "sla", "Slavic (Other)",
            "slo", "Slovak",
            "slv", "Slovenian",
            "sog", "Sogdian",
            "som", "Somali",
            "son", "Songhai",
            "snk", "Soninke",
            "wen", "Sorbian languages",
            "nso", "Sotho, Northern",
            "sot", "Sotho, Southern",
            "sai", "South American Indian (Other)",
            "sma", "Southern Sami",
            "suk", "Sukuma",
            "sux", "Sumerian",
            "sun", "Sundanese",
            "sus", "Susu",
            "swa", "Swahili",
            "ssw", "Swati",
            "swe", "Swedish",
            "syr", "Syriac",
            "tgl", "Tagalog",
            "tah", "Tahitian",
            "tai", "Tai (Other)",
            "tgk", "Tajik",
            "tmh", "Tamashek",
            "tam", "Tamil",
            "tat", "Tatar",
            "tel", "Telugu",
            "ter", "Tereno",
            "tet", "Tetum",
            "tha", "Thai",
            "tib", "Tibetan",
            "tig", "Tigre",
            "tir", "Tigrinya",
            "tem", "Timne",
            "tiv", "Tiv",
            "tli", "Tlingit",
            "tpi", "Tok Pisin",
            "tkl", "Tokelau",
            "tog", "Tonga (Nyasa)",
            "ton", "Tonga (Tonga Islands)",
            "tsi", "Tsimshian",
            "tso", "Tsonga",
            "tsn", "Tswana",
            "tum", "Tumbuka",
            "tup", "Tupi languages",
            "tur", "Turkish",
            "ota", "Turkish, Ottoman (1500-1928)",
            "tuk", "Turkmen",
            "tvl", "Tuvalu",
            "tyv", "Tuvinian",
            "twi", "Twi",
            "uga", "Ugaritic",
            "uig", "Uighur",
            "ukr", "Ukrainian",
            "umb", "Umbundu",
            "und", "Undetermined",
            "urd", "Urdu",
            "uzb", "Uzbek",
            "vai", "Vai",
            "ven", "Venda",
            "vie", "Vietnamese",
            "vol", "VolapÃ¼k",
            "vot", "Votic",
            "wak", "Wakashan languages",
            "wal", "Walamo",
            "wln", "Walloon",
            "war", "Waray",
            "was", "Washo",
            "wel", "Welsh",
            "wol", "Wolof",
            "xho", "Xhosa",
            "sah", "Yakut",
            "yao", "Yao",
            "yap", "Yapese",
            "yid", "Yiddish",
            "yor", "Yoruba",
            "ypk", "Yupik languages",
            "znd", "Zande",
            "zap", "Zapotec",
            "zen", "Zenaga",
            "zul", "Zulu",
            "zun", "Zuni",
        };
    }
}

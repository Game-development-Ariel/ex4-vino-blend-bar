<div dir="rtl" lang="he">

# בר הבלנדים: ערב ראשון

לקוח מתאר במילים איזה יין הוא רוצה, ולא אומר מאילו ענבים. השחקן מוזג מהבקבוקים לכוס, טועם, מתקן ומגיש לפני שנגמרת ללקוח הסבלנות. בערב יש שלושה לקוחות.

מסמך העיצוב: [Vino-Blend-Bar](https://github.com/Game-development-Ariel/Vino-Blend-Bar)

## קישורים

* המשחק באיץ' (אפשר לשחק בדפדפן): [game-dev-ariel.itch.io/ex4-vino-blend-bar](https://game-dev-ariel.itch.io/ex4-vino-blend-bar)
* דיווח על שימוש בבינה מלאכותית: [AI_REPORT.md](AI_REPORT.md)

## איך מריצים

1. אחרי `git clone` מריצים `git lfs pull`, כי התמונות והצלילים שמורים ב-Git LFS.
2. ב-Unity Hub פותחים את התיקייה של המאגר, בגרסה 6000.3.25f1.
3. פותחים את הסצנה `Assets/Scenes/WineBar` ולוחצים Play.

הסצנה והנתונים נבנים מקוד ב-[WineBarSceneBuilder.cs](Assets/Scripts/Editor/WineBarSceneBuilder.cs). אחרי שינוי בכלים מריצים `Tools > Rebuild Scene`. בנייה לדפדפן: `Tools > Build WebGL`.

## איך משחקים

הכול בעכבר (או במגע):

* לחיצה על בקבוק מוזגת 10% מהכוס. מתחת לכל בקבוק כתוב איך הזן מרגיש, באותן מילים שהלקוחות משתמשים בהן.
* **Taste** מתאר את מה שיש בכוס, אבל עולה 5 שניות מהסבלנות של הלקוח.
* **Empty glass** מרוקן את הכוס. **Serve** עובד רק כשהכוס מלאה (100%).
* אחרי ההגשה רואים ציון, מה היה גבוה או נמוך מדי, ומה הלקוח רצה.
* **ניצחון**: ממוצע של 3 כוכבים לפחות בשלושת הלקוחות.
* **הפסד**: ממוצע נמוך יותר. לקוח שנגמרה לו הסבלנות עוזב בלי לשלם ומקבל 0.

## פיסיקה

| תכונה | איפה | קובץ |
|---|---|---|
| Rigidbody2D | כל מזיגה היא טיפת יין שעפה מהבקבוק לכוס | [WineDrop.cs](Assets/Scripts/Pouring/WineDrop.cs) |
| AddForce | הברמן זורק את הטיפה במהירות שמחושבת לפי נוסחת הזריקה, כך שתנחת בכוס מכל בקבוק על המדף | [Bartender.cs](Assets/Scripts/Pouring/Bartender.cs) |

הכוס היא טריגר ([Glass.cs](Assets/Scripts/Pouring/Glass.cs)): כשטיפה נכנסת היא בודקת איזה זן יש בה וכמה, ומוסיפה לבלנד. טיפה שלא נכנסת לכוס פשוט נופלת.

## מבנה התיקיות

<div dir="ltr">

```
Assets/
├── Scripts/
│   ├── Wine/       GrapeData, Blend, WineProfile, WineWords
│   ├── Pouring/    GrapeBottle, Bartender, WineDrop, Glass
│   ├── Guests/     GuestRecipe, GuestOrder, BlendJudge, ServeResult
│   ├── Game/       BarEvening, Patience, BarEveningSettings
│   ├── Input/      IClickable, WorldClicker
│   ├── UI/         BarHud, GuestPanel, BlendPanel, GlassFill, EveningEndPanel, ResultOverlay, UiFactory, ...
│   ├── Audio/      Sfx
│   └── Editor/     the tools that build the scene
├── Data/           Grapes/ (8 grapes), Day1Settings (the guests of the evening)
├── Prefabs/        WineDrop
├── Scenes/  Sprites/  Resources/  Settings/
```

</div>

`Game` מחזיקה את החוקים (מתי מנצחים ומפסידים), ו-`UI` מציגה. קוד המשחק לא מכיר את קוד הממשק: הממשק מאזין לאירועים של המשחק, ולא להפך. הכפתורים קוראים לפונקציות של BarEvening, אבל BarEvening לא יודע שיש כפתורים.

## יחסי המחלקות

כל האירועים כאן הם אירועים רגילים של C# (`event Action`). כל הרשמה נעשית ב-`OnEnable` ומבוטלת ב-`OnDisable`.

בתרשים: מלבן הוא מחלקה, חץ מקווקו הוא אירוע, וחץ רגיל הוא קריאה ישירה.

<div dir="ltr">

```mermaid
flowchart LR
    WorldClicker -- OnClicked --> GrapeBottle
    GrapeBottle -. Clicked .-> Bartender
    Bartender -- creates, AddForce --> WineDrop
    Glass -- OnTriggerEnter2D reads --> WineDrop
    Glass -- Add --> Blend
    Glass -. Changed .-> GlassFill
    Glass -. Changed .-> BlendPanel

    BarEvening -- Restart, Spend --> Patience
    Patience -. RanOut .-> BarEvening
    BarEvening -- Judge --> BlendJudge
    BlendJudge -- reads --> WineProfile
    BarEvening -- enabled --> Bartender
    BarEvening -- Empty, reads Blend --> Glass

    BarEvening -. GuestArrived, GuestServed, GuestGaveUp, Tasted .-> BlendPanel
    BarEvening -. GuestArrived, GuestServed, GuestGaveUp .-> GuestPanel
    BarEvening -. GuestArrived, GuestServed .-> BarHud
    BarEvening -. EveningEnded .-> EveningEndPanel
    BlendPanel -- Taste, Serve, NextGuest --> BarEvening
    BlendPanel -- Empty --> Glass
    GuestPanel -- reads --> Patience
```

</div>

| אירוע | מי שולח | מי מקבל |
|---|---|---|
| GrapeBottle.Clicked | GrapeBottle | Bartender (מוזג) |
| Glass.Changed | Glass | GlassFill (גובה וצבע היין), BlendPanel (מה בכוס, האם אפשר להגיש) |
| Patience.RanOut | Patience | BarEvening (הלקוח עוזב) |
| GuestArrived, GuestServed, GuestGaveUp, Tasted | BarEvening | BlendPanel, GuestPanel, BarHud |
| EveningEnded | BarEvening | EveningEndPanel |

## החלטות ארכיטקטורה

**1. מזיגה פיזיקלית במקום כפתורי פלוס ומינוס.** באב-הטיפוס שלנו היו כפתורי + ו- ליד כל זן, ומחלקה אחת גדולה (WineShopGame) שבנתה את כל המסך והחזיקה את כל המצב. עכשיו כל מזיגה היא טיפה שנושאת את הזן ואת הכמות, והכוס מוסיפה לבלנד רק את מה שנכנס אליה. ככה הבקבוק, הברמן והכוס לא מכירים זה את זה מעבר לטיפה. החלופה (להשאיר את הכפתורים) הייתה פשוטה יותר, אבל בלי שום פיסיקה ועם הרבה פחות תחושה של בר.

**2. שפה אחת ללקוחות ולטעימה.** [WineWords](Assets/Scripts/Wine/WineWords.cs) הופך מספרים למילים ("firm, grippy tannins"). גם הלקוח שמזמין וגם הטעימה משתמשים בו, ולכן מה שהלקוח אומר ומה שהטעימה מתארת תמיד באותן מילים, וזה בדיוק מה שהשחקן לומד. החלופה הייתה לכתוב את בקשות הלקוחות ביד, ואז היה אפשר בטעות לבקש "גוף מלא" כשהבלנד הנכון בכלל לא מלא.

**3. אירועים רגילים של C# ולא אירועים כנכסים.** כל אירוע כאן נושא מידע (ההזמנה, תוצאת ההגשה), ולכל המאזינים ממילא יש הפניה ל-BarEvening. אירוע כנכס היה מוסיף קובץ לכל אירוע בלי להוריד אף תלות.

**4. השופט הוא מחלקה רגילה.** [BlendJudge](Assets/Scripts/Guests/BlendJudge.cs), [Blend](Assets/Scripts/Wine/Blend.cs) ו-[WineProfile](Assets/Scripts/Wine/WineProfile.cs) הם מחלקות C# רגילות ולא MonoBehaviour. הם רק מחשבים, אין להם מה לעשות בסצנה, וכך קל להבין אותם בלי יוניטי.

המספרים לכוונון (סבלנות, מחיר טעימה, תשלום, כמה כוכבים צריך) נמצאים בנכס [Day1Settings](Assets/Data/Day1Settings.asset), והזנים בנכסים ב-`Assets/Data/Grapes`. הספים של המילים ושל הציון נשארו בקוד, כי הם ההגדרה של השפה של המשחק ולא ערכים שמכווננים מערב לערב.

## איך מוסיפים דברים

**לקוח חדש (המקבילה ל"אויב חדש" כאן).** מוסיפים שורה לרשימת guests בנכס [Day1Settings](Assets/Data/Day1Settings.asset): שם, בלנד וניחוחות. אין שינוי בקוד: מה שהלקוח אומר נבנה מהבלנד.

**זן חדש.** נכס GrapeData חדש (`Create > Wine Bar > Grape`) ובקבוק נוסף על המדף ב-[WineBarSceneBuilder.cs](Assets/Scripts/Editor/WineBarSceneBuilder.cs). החישוב של הבלנד והניסוח של הלקוחות עובדים על כל זן.

## סינגלטון ומשתנים סטטיים

אין סינגלטון.

| איפה | מה | למה |
|---|---|---|
| [WineWords](Assets/Scripts/Wine/WineWords.cs) | מחלקה סטטית | רק פונקציות שהופכות מספר למילים. אין בה מצב |
| [Sfx](Assets/Scripts/Audio/Sfx.cs) | מחלקה סטטית עם AudioSource אחד | הקליק של Play again צריך להמשיך גם כשהסצנה נטענת מחדש. אין בו מצב של המשחק |
| [ScreenFader](Assets/Scripts/UI/ScreenFader.cs) | `FadeAndLoad` סטטית ודגל `isFading` | המסך השחור חייב לשרוד את טעינת הסצנה. הדגל מונע טעינה כפולה |
| UiFactory, UiAnim, UiTheme, ResultOverlay | מחלקות סטטיות של פונקציות עזר וקבועים | אין בהן מצב |

## מה השתנה לעומת מסמך השלבים

* במקום כפתורי פלוס ומינוס, לוחצים על בקבוק והיין נזרק לכוס. אי אפשר להוריד זן אחד, רק לרוקן את הכוס.
* בערב יש שלושת הלקוחות הראשונים של יום 1, כדי שהשלב יהיה קצר.

</div>

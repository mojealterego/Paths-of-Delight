using System;
using System.Collections.Generic;
using UnityEngine;

namespace PathsOfDelight
{
    public sealed class GameApp : MonoBehaviour
    {
        private LocalVault _vault;
        private LocalSave _save;
        private List<ContentCard> _cards;
        private readonly GameDirector _director = new GameDirector();
        private Edition _edition;
        private AppScreen _screen = AppScreen.Welcome;
        private AppScreen _resumeScreen = AppScreen.Board;
        private ContentCard _current;
        private AnswerChoice _answerA;
        private AnswerChoice _answerB;
        private ConsentChoice _consentA;
        private ConsentChoice _consentB;
        private bool _adultConfirmed;
        private Vector2 _scroll;
        private string _customPrompt = "";
        private string _customActivity = "";
        private int _customIntensity = 1;
        private GUIStyle _title;
        private GUIStyle _heading;
        private GUIStyle _body;
        private GUIStyle _button;
        private GUIStyle _dangerButton;
        private GUIStyle _card;
        private Texture2D _bg;
        private Texture2D _panel;
        private Texture2D _accent;
        private Texture2D _danger;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            _edition = EditionConfig.Current;
            _vault = new LocalVault();
            _save = _vault.Load();
            EnsureState();
            ReloadCards();
        }

        private void EnsureState()
        {
            if (_save.playerA == null) _save.playerA = new PlayerProfile { displayName = "Partner A" };
            if (_save.playerB == null) _save.playerB = new PlayerProfile { displayName = "Partner B" };
            if (_save.session == null) _save.session = new SessionState();
            if (_save.customCards == null) _save.customCards = new List<ContentCard>();
            if (string.IsNullOrEmpty(_save.session.sessionId)) _save.session.sessionId = Guid.NewGuid().ToString("N");
            if (_save.session.seed == 0) _save.session.seed = Environment.TickCount & 0x7fffffff;
        }

        private void ReloadCards() => _cards = new ContentCatalogService().Load(_edition, _save.customCards);

        private void OnGUI()
        {
            EnsureStyles();
            var scale = Mathf.Clamp(Screen.width / 1080f, 0.75f, 1.5f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            var w = Screen.width / scale;
            var h = Screen.height / scale;
            GUI.DrawTexture(new Rect(0, 0, w, h), _bg, ScaleMode.StretchToFill);
            GUILayout.BeginArea(new Rect(40, 28, w - 80, h - 56));
            Header();
            _scroll = GUILayout.BeginScrollView(_scroll, false, true);
            switch (_screen)
            {
                case AppScreen.Welcome: Welcome(); break;
                case AppScreen.ProfileA: Profile(_save.playerA, "Profil prywatny — Partner A", AppScreen.ProfileB); break;
                case AppScreen.ProfileB: Profile(_save.playerB, "Profil prywatny — Partner B", AppScreen.Lobby); break;
                case AppScreen.Lobby: Lobby(); break;
                case AppScreen.Board: Board(); break;
                case AppScreen.PrivateAnswerA: PrivateAnswer(true); break;
                case AppScreen.PrivateAnswerB: PrivateAnswer(false); break;
                case AppScreen.MatchResult: MatchResult(); break;
                case AppScreen.ConsentA: Consent(true); break;
                case AppScreen.ConsentB: Consent(false); break;
                case AppScreen.Activity: Activity(); break;
                case AppScreen.Paused: Paused(); break;
                case AppScreen.Summary: Summary(); break;
                case AppScreen.Privacy: Privacy(); break;
                case AppScreen.CustomContent: CustomContent(); break;
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void Header()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("ŚCIEŻKI ROZKOSZY", _title);
            GUILayout.FlexibleSpace();
            GUILayout.Label(_edition == Edition.Play ? "PLAY EDITION" : "ADULT EDITION", _heading);
            GUILayout.EndHorizontal();
            GUILayout.Space(12);
        }

        private void Welcome()
        {
            PanelStart();
            GUILayout.Label("Prywatna gra planszowa dla dwóch dorosłych partnerów", _heading);
            GUILayout.Space(16);
            GUILayout.Label("Ta wersja demonstracyjna używa samopotwierdzenia wieku. Nie jest to silna weryfikacja tożsamości ani wieku. Gra jest przeznaczona wyłącznie dla osób 18+.", _body);
            GUILayout.Space(18);
            _adultConfirmed = GUILayout.Toggle(_adultConfirmed, " Potwierdzamy, że oboje mamy co najmniej 18 lat.", _body);
            GUILayout.Space(20);
            GUI.enabled = _adultConfirmed;
            if (BigButton("ROZPOCZNIJ KONFIGURACJĘ")) _screen = AppScreen.ProfileA;
            GUI.enabled = true;
            GUILayout.Space(10);
            if (BigButton("PRYWATNOŚĆ I DANE")) _screen = AppScreen.Privacy;
            PanelEnd();
        }

        private void Profile(PlayerProfile profile, string title, AppScreen next)
        {
            PanelStart();
            GUILayout.Label(title, _heading);
            GUILayout.Label("Nazwa widoczna tylko lokalnie", _body);
            profile.displayName = GUILayout.TextField(profile.displayName ?? "", 24, GUILayout.Height(54));
            GUILayout.Space(12);
            GUILayout.Label("Wiek: " + profile.age + " (minimum 18)", _body);
            GUILayout.BeginHorizontal();
            if (SmallButton("−")) profile.age = Mathf.Max(18, profile.age - 1);
            if (SmallButton("+")) profile.age = Mathf.Min(99, profile.age + 1);
            GUILayout.EndHorizontal();
            GUILayout.Space(14);
            GUILayout.Label("Maksymalna intensywność tej sesji: " + profile.intensityCeiling + "/3", _body);
            profile.intensityCeiling = Mathf.RoundToInt(GUILayout.HorizontalSlider(profile.intensityCeiling, 1, 3, GUILayout.Height(34)));
            GUILayout.Space(18);
            GUILayout.Label("Wyłącz kategorie, których nie chcesz widzieć. Te ustawienia nie są pokazywane partnerowi.", _body);
            BoundaryToggle(profile, "touch", "Kontakt dotykowy");
            BoundaryToggle(profile, "blindfold", "Zasłonięte oczy");
            BoundaryToggle(profile, "roleplay", "Odgrywanie ról");
            BoundaryToggle(profile, "props", "Akcesoria");
            GUILayout.Space(18);
            if (BigButton("ZAPISZ I DALEJ"))
            {
                _vault.Save(_save);
                _screen = next;
            }
            PanelEnd();
        }

        private void BoundaryToggle(PlayerProfile profile, string tag, string label)
        {
            var blocked = profile.blockedTags.Contains(tag);
            var next = GUILayout.Toggle(blocked, " Wyłącz: " + label, _body);
            if (next && !blocked) profile.blockedTags.Add(tag);
            if (!next && blocked) profile.blockedTags.Remove(tag);
        }

        private void Lobby()
        {
            PanelStart();
            GUILayout.Label("Lobby pary", _heading);
            GUILayout.Label("Profile są gotowe. Gra stosuje niższy z dwóch limitów intensywności i wspólną część dozwolonych kategorii.", _body);
            GUILayout.Space(12);
            GUILayout.Label(_save.playerA.displayName + " • limit " + _save.playerA.intensityCeiling + "/3", _body);
            GUILayout.Label(_save.playerB.displayName + " • limit " + _save.playerB.intensityCeiling + "/3", _body);
            GUILayout.Space(18);
            if (BigButton("NOWA SESJA")) StartSession();
            if (BigButton("WŁASNE KARTY")) _screen = AppScreen.CustomContent;
            if (BigButton("PRYWATNOŚĆ I DANE")) _screen = AppScreen.Privacy;
            PanelEnd();
        }

        private void StartSession()
        {
            _save.session = new SessionState
            {
                sessionId = Guid.NewGuid().ToString("N"),
                seed = Environment.TickCount & 0x7fffffff,
                turn = 0,
                boardPosition = 0,
                sharedPoints = 0,
                paused = false,
                currentCardId = ""
            };
            _vault.Save(_save);
            _screen = AppScreen.Board;
        }

        private void Board()
        {
            PanelStart();
            GUILayout.Label("Plansza • runda " + (_save.session.turn + 1) + "/8", _heading);
            GUILayout.Label("Wspólne punkty: " + _save.session.sharedPoints, _body);
            GUILayout.Space(14);
            DrawBoard();
            GUILayout.Space(18);
            if (BigButton("ODKRYJ NASTĘPNE POLE")) BeginRound();
            if (BigButton("PAUZA")) Pause(AppScreen.Board);
            if (BigButton("ZAKOŃCZ SESJĘ")) _screen = AppScreen.Summary;
            PanelEnd();
        }

        private void DrawBoard()
        {
            for (var row = 0; row < 3; row++)
            {
                GUILayout.BeginHorizontal();
                for (var col = 0; col < 4; col++)
                {
                    var index = row * 4 + col;
                    var old = GUI.backgroundColor;
                    GUI.backgroundColor = index == _save.session.boardPosition ? new Color(0.78f, 0.55f, 0.2f) : new Color(0.22f, 0.12f, 0.18f);
                    GUILayout.Box((index + 1).ToString("00"), _card, GUILayout.Height(70), GUILayout.ExpandWidth(true));
                    GUI.backgroundColor = old;
                }
                GUILayout.EndHorizontal();
            }
        }

        private void BeginRound()
        {
            _current = _director.SelectNext(_cards, _save.playerA, _save.playerB, _save.session, _edition);
            if (_current == null)
            {
                _screen = AppScreen.Summary;
                return;
            }
            _save.session.currentCardId = _current.id;
            _answerA = AnswerChoice.No;
            _answerB = AnswerChoice.No;
            _consentA = ConsentChoice.Decline;
            _consentB = ConsentChoice.Decline;
            _screen = AppScreen.PrivateAnswerA;
        }

        private void PrivateAnswer(bool playerA)
        {
            PanelStart();
            GUILayout.Label("Ekran prywatny — " + (playerA ? _save.playerA.displayName : _save.playerB.displayName), _heading);
            GUILayout.Label("Przekaż urządzenie właściwej osobie. Odpowiedź nie zostanie pokazana partnerowi.", _body);
            GUILayout.Space(14);
            GUILayout.Label(_current != null ? _current.prompt : "Brak karty", _heading);
            GUILayout.Space(18);
            if (BigButton("TAK")) SubmitPrivateAnswer(playerA, AnswerChoice.Yes);
            if (BigButton("MOŻE")) SubmitPrivateAnswer(playerA, AnswerChoice.Maybe);
            if (BigButton("NIE")) SubmitPrivateAnswer(playerA, AnswerChoice.No);
            GUILayout.Space(10);
            if (BigButton("POMIŃ BEZ KARY")) SkipRound();
            if (BigButton("PAUZA")) Pause(playerA ? AppScreen.PrivateAnswerA : AppScreen.PrivateAnswerB);
            PanelEnd();
        }

        private void SubmitPrivateAnswer(bool playerA, AnswerChoice choice)
        {
            if (playerA)
            {
                _answerA = choice;
                _screen = AppScreen.PrivateAnswerB;
            }
            else
            {
                _answerB = choice;
                _screen = AppScreen.MatchResult;
            }
        }

        private void MatchResult()
        {
            PanelStart();
            var match = ConsentEngine.IsBlindMatch(_answerA, _answerB);
            GUILayout.Label(match ? "WSPÓLNE TAK" : "BRAK WSPÓLNEGO TAK", _heading);
            GUILayout.Space(12);
            GUILayout.Label(match
                ? "Gra wykryła zgodność. Zanim pojawi się aktywność, każda osoba osobno potwierdzi zgodę jeszcze raz."
                : "Gra nie ujawnia, kto wybrał TAK, MOŻE lub NIE. Nie ma kary, utraty punktów ani presji.", _body);
            GUILayout.Space(18);
            if (match)
            {
                if (BigButton("PRZEJDŹ DO ZGODY")) _screen = AppScreen.ConsentA;
            }
            else if (BigButton("DALEJ")) AdvanceNeutral();
            if (BigButton("PAUZA")) Pause(AppScreen.MatchResult);
            PanelEnd();
        }

        private void Consent(bool playerA)
        {
            PanelStart();
            GUILayout.Label("Potwierdzenie zgody — " + (playerA ? _save.playerA.displayName : _save.playerB.displayName), _heading);
            GUILayout.Label("To osobna decyzja. Możesz odmówić lub przerwać bez żadnej konsekwencji w grze.", _body);
            GUILayout.Space(16);
            if (BigButton("ZGADZAM SIĘ NA TĘ AKTYWNOŚĆ")) SubmitConsent(playerA, ConsentChoice.Accept);
            if (BigButton("NIE / POMIŃ")) SubmitConsent(playerA, ConsentChoice.Decline);
            if (BigButton("PAUZA")) Pause(playerA ? AppScreen.ConsentA : AppScreen.ConsentB);
            PanelEnd();
        }

        private void SubmitConsent(bool playerA, ConsentChoice choice)
        {
            if (playerA)
            {
                _consentA = choice;
                _screen = AppScreen.ConsentB;
                return;
            }
            _consentB = choice;
            if (ConsentEngine.CanStartActivity(_consentA, _consentB)) _screen = AppScreen.Activity;
            else AdvanceNeutral();
        }

        private void Activity()
        {
            PanelStart();
            GUILayout.Label("Wspólna aktywność", _heading);
            GUILayout.Label(_current != null ? _current.activity : "", _body);
            GUILayout.Space(16);
            GUILayout.Label("Możecie zatrzymać lub pominąć aktywność w dowolnej chwili. Punkty są za dobrowolne ukończenie rundy, nie za samą zgodę.", _body);
            GUILayout.Space(18);
            if (BigButton("UKOŃCZONE")) CompleteActivity();
            if (BigButton("POMIŃ BEZ KARY")) SkipRound();
            if (BigButton("PAUZA")) Pause(AppScreen.Activity);
            PanelEnd();
        }

        private void CompleteActivity()
        {
            if (_current != null)
                _save.session.sharedPoints += ConsentEngine.SharedReward(true, _current.rewardPoints);
            AdvanceNeutral();
        }

        private void SkipRound() => AdvanceNeutral();

        private void AdvanceNeutral()
        {
            if (_current != null && !_save.session.completedCardIds.Contains(_current.id)) _save.session.completedCardIds.Add(_current.id);
            _save.session.turn++;
            _save.session.boardPosition = Mathf.Min(11, _save.session.boardPosition + 1);
            _save.session.currentCardId = "";
            _current = null;
            _vault.Save(_save);
            _screen = _save.session.turn >= 8 ? AppScreen.Summary : AppScreen.Board;
        }

        private void Pause(AppScreen resume)
        {
            _resumeScreen = resume;
            _save.session.paused = true;
            _vault.Save(_save);
            _screen = AppScreen.Paused;
        }

        private void Paused()
        {
            PanelStart();
            GUILayout.Label("PAUZA", _heading);
            GUILayout.Label("Nic nie dzieje się w tle. Możecie wrócić wtedy, kiedy oboje chcecie.", _body);
            GUILayout.Space(18);
            if (BigButton("WZNÓW"))
            {
                _save.session.paused = false;
                _vault.Save(_save);
                _screen = _resumeScreen;
            }
            if (BigButton("ZAKOŃCZ SESJĘ")) _screen = AppScreen.Summary;
            PanelEnd();
        }

        private void Summary()
        {
            PanelStart();
            GUILayout.Label("Podsumowanie sesji", _heading);
            GUILayout.Label("Rundy: " + _save.session.turn, _body);
            GUILayout.Label("Wspólne punkty: " + _save.session.sharedPoints, _body);
            GUILayout.Label("Pominięcia i odmowy nie są zliczane ani oceniane.", _body);
            GUILayout.Space(18);
            if (BigButton("NOWA SESJA")) StartSession();
            if (BigButton("WRÓĆ DO LOBBY")) _screen = AppScreen.Lobby;
            PanelEnd();
        }

        private void Privacy()
        {
            PanelStart();
            GUILayout.Label("Prywatność", _heading);
            GUILayout.Label("Vertical slice działa lokalnie i nie wysyła odpowiedzi intymnych do analityki ani serwera. Zapis lokalny jest szyfrowany AES-CBC i uwierzytelniany HMAC-SHA256. Klucz demonstracyjny jest przechowywany lokalnie przez PlayerPrefs; produkcyjna wersja powinna przenieść klucz do Android Keystore / iOS Keychain.", _body);
            GUILayout.Space(12);
            GUILayout.Label("Możesz usunąć cały zapis jednym przyciskiem. Ta operacja usuwa lokalny sejw i klucz demonstracyjny.", _body);
            GUILayout.Space(18);
            if (BigButton("WRÓĆ")) _screen = _adultConfirmed ? AppScreen.Lobby : AppScreen.Welcome;
            var old = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.55f, 0.12f, 0.18f);
            if (GUILayout.Button("USUŃ WSZYSTKIE DANE LOKALNE", _dangerButton, GUILayout.Height(64)))
            {
                _vault.DeleteAll();
                _save = _vault.Load();
                EnsureState();
                ReloadCards();
                _adultConfirmed = false;
                _screen = AppScreen.Welcome;
            }
            GUI.backgroundColor = old;
            PanelEnd();
        }

        private void CustomContent()
        {
            PanelStart();
            GUILayout.Label("Własna karta prywatna", _heading);
            GUILayout.Label("Treść pozostaje w lokalnym, szyfrowanym zapisie. Nie jest publikowana społecznościowo.", _body);
            GUILayout.Space(12);
            GUILayout.Label("Pytanie", _body);
            _customPrompt = GUILayout.TextArea(_customPrompt, GUILayout.MinHeight(90));
            GUILayout.Label("Aktywność", _body);
            _customActivity = GUILayout.TextArea(_customActivity, GUILayout.MinHeight(120));
            GUILayout.Label("Intensywność: " + _customIntensity + "/3", _body);
            _customIntensity = Mathf.RoundToInt(GUILayout.HorizontalSlider(_customIntensity, 1, 3, GUILayout.Height(34)));
            GUILayout.Space(14);
            GUI.enabled = !string.IsNullOrWhiteSpace(_customPrompt) && !string.IsNullOrWhiteSpace(_customActivity);
            if (BigButton("DODAJ KARTĘ"))
            {
                _save.customCards.Add(new ContentCard
                {
                    id = "custom-" + Guid.NewGuid().ToString("N"),
                    prompt = _customPrompt.Trim(),
                    activity = _customActivity.Trim(),
                    intensity = _customIntensity,
                    edition = _edition == Edition.Adult ? "adult" : "play",
                    rewardPoints = 1,
                    tags = new string[0]
                });
                _customPrompt = "";
                _customActivity = "";
                _vault.Save(_save);
                ReloadCards();
            }
            GUI.enabled = true;
            GUILayout.Space(12);
            GUILayout.Label("Liczba własnych kart: " + _save.customCards.Count, _body);
            if (BigButton("WRÓĆ")) _screen = AppScreen.Lobby;
            PanelEnd();
        }

        private bool BigButton(string text) => GUILayout.Button(text, _button, GUILayout.Height(64));
        private bool SmallButton(string text) => GUILayout.Button(text, _button, GUILayout.Height(54), GUILayout.Width(120));
        private void PanelStart() { GUILayout.BeginVertical(_card); GUILayout.Space(12); }
        private void PanelEnd() { GUILayout.Space(12); GUILayout.EndVertical(); }

        private void EnsureStyles()
        {
            if (_title != null) return;
            _bg = Solid(new Color(0.055f, 0.025f, 0.045f));
            _panel = Solid(new Color(0.12f, 0.06f, 0.095f));
            _accent = Solid(new Color(0.78f, 0.55f, 0.20f));
            _danger = Solid(new Color(0.45f, 0.08f, 0.12f));
            _title = new GUIStyle(GUI.skin.label) { fontSize = 34, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.95f, 0.82f, 0.58f) } };
            _heading = new GUIStyle(GUI.skin.label) { fontSize = 25, fontStyle = FontStyle.Bold, wordWrap = true, normal = { textColor = Color.white } };
            _body = new GUIStyle(GUI.skin.label) { fontSize = 21, wordWrap = true, normal = { textColor = new Color(0.92f, 0.88f, 0.90f) } };
            _button = new GUIStyle(GUI.skin.button) { fontSize = 20, fontStyle = FontStyle.Bold, wordWrap = true, normal = { background = _accent, textColor = new Color(0.08f, 0.04f, 0.06f) }, active = { background = _panel, textColor = Color.white } };
            _dangerButton = new GUIStyle(_button) { normal = { background = _danger, textColor = Color.white } };
            _card = new GUIStyle(GUI.skin.box) { padding = new RectOffset(22, 22, 20, 20), margin = new RectOffset(0, 0, 8, 8), fontSize = 20, normal = { background = _panel, textColor = Color.white } };
        }

        private static Texture2D Solid(Color color)
        {
            var t = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            t.SetPixel(0, 0, color);
            t.Apply();
            return t;
        }
    }
}

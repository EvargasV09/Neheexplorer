using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Nehexplorer
{
    /// <summary>
    /// Minijuego 2: Trivia Rápida de las Cordilleras (Región Andina).
    /// Evalúa geografía, pisos térmicos, páramos y biodiversidad andina con preguntas y temporizador interactivo.
    /// </summary>
    public class AndeanTriviaGame : MinigameBase
    {
        [Header("Datos Educativos")]
        [Tooltip("ScriptableObject con las preguntas y datos andinos. Editable por docentes.")]
        [SerializeField] private AndeanTriviaDataSet dataSet;

        [Header("UI - Encabezado y Progreso")]
        [SerializeField] private TextMeshProUGUI gameTitleText;
        [SerializeField] private TextMeshProUGUI instructionsText;
        [SerializeField] private TextMeshProUGUI questionCounterText;
        [SerializeField] private TextMeshProUGUI scoreCounterText;

        [Header("UI - Temporizador")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Image timerFillImage;
        [SerializeField] private Color timerNormalColor = new Color(0.42f, 0.78f, 0.98f);
        [SerializeField] private Color timerWarningColor = new Color(0.95f, 0.35f, 0.35f);

        [Header("UI - Tarjeta de Pregunta")]
        [SerializeField] private TextMeshProUGUI categoryBadgeText;
        [SerializeField] private TextMeshProUGUI categoryEmojiText;
        [SerializeField] private TextMeshProUGUI questionBodyText;
        [SerializeField] private UnityEngine.UI.Image questionImage;

        [Header("UI - Botones de Respuesta")]
        [SerializeField] private Button[] optionButtons = new Button[4];
        [SerializeField] private TextMeshProUGUI[] optionTexts = new TextMeshProUGUI[4];
        [SerializeField] private Image[] optionBackgrounds = new Image[4];

        [Header("UI - Panel de Explicación y Retroalimentación")]
        [SerializeField] private GameObject explanationPanel;
        [SerializeField] private TextMeshProUGUI feedbackTitleText;
        [SerializeField] private TextMeshProUGUI feedbackExplanationText;
        [SerializeField] private Button nextQuestionButton;
        [SerializeField] private TextMeshProUGUI nextButtonText;

        [Header("Colores de Opciones")]
        [SerializeField] private Color normalOptionColor = new Color(0.16f, 0.25f, 0.36f, 1f);
        [SerializeField] private Color correctOptionColor = new Color(0.15f, 0.68f, 0.38f, 1f);
        [SerializeField] private Color wrongOptionColor = new Color(0.85f, 0.28f, 0.28f, 1f);
        [SerializeField] private Color highlightOptionColor = new Color(0.24f, 0.48f, 0.72f, 1f);

        // Estado interno
        private List<TriviaQuestion> _activeQuestions = new List<TriviaQuestion>();
        private int _currentQuestionIndex = 0;
        private int _correctCount = 0;
        private float _timeRemaining = 0f;
        private float _questionDuration = 18f;
        private int _requiredScore = 3;
        private bool _isAnswered = false;
        private Coroutine _timerCoroutine;

        protected override void Awake()
        {
            base.Awake();
            regionId = "Andina";
            regionDisplayName = "Región Andina";

            if (nextQuestionButton != null)
            {
                nextQuestionButton.onClick.AddListener(OnNextQuestionClicked);
            }

            for (int i = 0; i < optionButtons.Length; i++)
            {
                int index = i;
                if (optionButtons[i] != null)
                {
                    optionButtons[i].onClick.AddListener(() => OnOptionSelected(index));
                }
            }
        }

        public override void StartGame()
        {
            base.StartGame();

            _currentQuestionIndex = 0;
            _correctCount = 0;
            _isAnswered = false;

            if (dataSet != null)
            {
                factTitle = dataSet.winFactTitle;
                educationalFact = dataSet.winEducationalFact;
                _questionDuration = dataSet.secondsPerQuestion > 0 ? dataSet.secondsPerQuestion : 18f;
                _requiredScore = dataSet.requiredScore > 0 ? dataSet.requiredScore : 3;

                if (gameTitleText != null) gameTitleText.text = dataSet.minigameTitle;
                if (instructionsText != null) instructionsText.text = dataSet.instructions;

                _activeQuestions = new List<TriviaQuestion>(dataSet.questions);
            }
            else
            {
                _questionDuration = 18f;
                _requiredScore = 3;
                _activeQuestions = GetFallbackQuestions();
            }

            if (_activeQuestions == null || _activeQuestions.Count == 0)
            {
                _activeQuestions = GetFallbackQuestions();
            }

            if (explanationPanel != null)
            {
                explanationPanel.SetActive(false);
            }

            UpdateStatsUI();
            LoadCurrentQuestion();
        }

        private void LoadCurrentQuestion()
        {
            if (_currentQuestionIndex >= _activeQuestions.Count)
            {
                FinishQuiz();
                return;
            }

            _isAnswered = false;
            var currentQ = _activeQuestions[_currentQuestionIndex];

            if (explanationPanel != null)
            {
                explanationPanel.SetActive(false);
            }

            if (categoryBadgeText != null) categoryBadgeText.text = currentQ.category.ToUpper();
            if (categoryEmojiText != null) categoryEmojiText.text = currentQ.categoryEmoji;
            if (questionBodyText != null) questionBodyText.text = currentQ.questionText;
            if (questionImage != null)
            {
                questionImage.sprite = currentQ.illustration;
                questionImage.enabled = currentQ.illustration != null;
                questionImage.preserveAspect = true;
                questionImage.raycastTarget = false;
            }

            string[] prefixes = new string[] { "A) ", "B) ", "C) ", "D) " };

            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (optionButtons[i] == null) continue;

                optionButtons[i].interactable = true;

                if (optionBackgrounds != null && i < optionBackgrounds.Length && optionBackgrounds[i] != null)
                {
                    optionBackgrounds[i].color = normalOptionColor;
                }

                if (optionTexts != null && i < optionTexts.Length && optionTexts[i] != null)
                {
                    if (currentQ.options != null && i < currentQ.options.Length)
                    {
                        optionTexts[i].text = prefixes[i] + currentQ.options[i];
                    }
                    else
                    {
                        optionTexts[i].text = "";
                    }
                    optionTexts[i].color = Color.white;
                }
            }

            UpdateStatsUI();

            // Iniciar cuenta regresiva
            if (_timerCoroutine != null) StopCoroutine(_timerCoroutine);
            _timerCoroutine = StartCoroutine(TimerRoutine());
        }

        private IEnumerator TimerRoutine()
        {
            _timeRemaining = _questionDuration;

            while (_timeRemaining > 0f)
            {
                _timeRemaining -= Time.deltaTime;
                UpdateTimerUI();
                yield return null;
            }

            _timeRemaining = 0f;
            UpdateTimerUI();

            if (!_isAnswered)
            {
                OnTimeExpired();
            }
        }

        private void UpdateTimerUI()
        {
            if (timerText != null)
            {
                timerText.text = $"TIEMPO: {Mathf.CeilToInt(_timeRemaining)}s";
            }

            if (timerFillImage != null)
            {
                float ratio = Mathf.Clamp01(_timeRemaining / _questionDuration);
                timerFillImage.fillAmount = ratio;
                timerFillImage.color = ratio < 0.3f ? timerWarningColor : timerNormalColor;
            }
        }

        private void OnOptionSelected(int selectedIndex)
        {
            if (_isAnswered || !isGameActive) return;

            _isAnswered = true;
            if (_timerCoroutine != null) StopCoroutine(_timerCoroutine);

            var currentQ = _activeQuestions[_currentQuestionIndex];
            bool isCorrect = (selectedIndex == currentQ.correctIndex);

            if (isCorrect)
            {
                _correctCount++;
            }

            UpdateStatsUI();

            // Bloquear interactividad y colorear
            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (optionButtons[i] != null) optionButtons[i].interactable = false;

                if (i == currentQ.correctIndex)
                {
                    if (optionBackgrounds[i] != null) optionBackgrounds[i].color = correctOptionColor;
                    if (optionTexts[i] != null) optionTexts[i].text += "  [CORRECTA]";
                }
                else if (i == selectedIndex && !isCorrect)
                {
                    if (optionBackgrounds[i] != null) optionBackgrounds[i].color = wrongOptionColor;
                    if (optionTexts[i] != null) optionTexts[i].text += "  [X]";
                }
            }

            ShowExplanation(isCorrect, isCorrect ? "¡CORRECTO! SABIDURÍA GANADA" : "¡RESPUESTA INCORRECTA!", currentQ.explanation);
        }

        private void OnTimeExpired()
        {
            if (_isAnswered || !isGameActive) return;

            _isAnswered = true;
            var currentQ = _activeQuestions[_currentQuestionIndex];

            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (optionButtons[i] != null) optionButtons[i].interactable = false;

                if (i == currentQ.correctIndex)
                {
                    if (optionBackgrounds[i] != null) optionBackgrounds[i].color = correctOptionColor;
                    if (optionTexts[i] != null) optionTexts[i].text += "  [CORRECTA]";
                }
            }

            ShowExplanation(false, "¡TIEMPO AGOTADO!", currentQ.explanation);
        }

        private void ShowExplanation(bool isCorrect, string statusTitle, string explanation)
        {
            if (explanationPanel != null)
            {
                explanationPanel.SetActive(true);

                if (feedbackTitleText != null)
                {
                    feedbackTitleText.text = statusTitle;
                    feedbackTitleText.color = isCorrect
                        ? new Color(0.35f, 0.95f, 0.55f)
                        : new Color(0.95f, 0.45f, 0.45f);
                }

                if (feedbackExplanationText != null)
                {
                    feedbackExplanationText.text = explanation;
                }

                if (nextButtonText != null)
                {
                    bool isLast = (_currentQuestionIndex + 1 >= _activeQuestions.Count);
                    nextButtonText.text = isLast ? "VER RESULTADO FINAL →" : "SIGUIENTE PREGUNTA →";
                }
            }
        }

        private void OnNextQuestionClicked()
        {
            _currentQuestionIndex++;
            LoadCurrentQuestion();
        }

        private void UpdateStatsUI()
        {
            if (questionCounterText != null)
            {
                int current = Mathf.Min(_currentQuestionIndex + 1, _activeQuestions.Count);
                questionCounterText.text = $"Pregunta: {current} / {_activeQuestions.Count}";
            }

            if (scoreCounterText != null)
            {
                scoreCounterText.text = $"Aciertos: {_correctCount} / {_activeQuestions.Count}";
            }
        }

        private void FinishQuiz()
        {
            bool won = _correctCount >= _requiredScore;
            EndGame(won);

            if (resultTitleText != null)
            {
                resultTitleText.text = won
                    ? $"¡SUPERASTE EL RETO ANDINO! ({_correctCount}/{_activeQuestions.Count} Aciertos)"
                    : $"INTENTO FINALIZADO ({_correctCount}/{_activeQuestions.Count} Aciertos - Mínimo {_requiredScore})";
            }

            if (returnButtonText != null)
            {
                returnButtonText.text = won ? "RECLAMAR SABIDURÍA Y VOLVER" : "INTENTAR DE NUEVO";
            }
        }

        protected override void OnReturnToHubClicked()
        {
            if (hasWon)
            {
                base.OnReturnToHubClicked();
            }
            else
            {
                // Si no ganó, volver a empezar la trivia
                StartGame();
            }
        }

        private List<TriviaQuestion> GetFallbackQuestions()
        {
            return new List<TriviaQuestion>
            {
                new TriviaQuestion
                {
                    category = "Páramos y Agua",
                    categoryEmoji = "💧",
                    questionText = "¿Qué planta icónica de los páramos andinos atrapa la humedad de la niebla y la transforma en agua pura para los ríos?",
                    options = new string[] { "Frailejón", "Cactus candelabro", "Palma de cera", "Manglar rojo" },
                    correctIndex = 0,
                    explanation = "Los frailejones tienen hojas vellosas que capturan las gotas de la niebla andina, nutriendo nacimientos de agua que abastecen a millones de personas."
                },
                new TriviaQuestion
                {
                    category = "Relieve y Cordilleras",
                    categoryEmoji = "🏔️",
                    questionText = "Al ingresar a Colombia desde el sur, la Cordillera de los Andes se divide en tres cadenas montañosas. ¿Cuáles son?",
                    options = new string[] { "Norte, Centro y Sur", "Occidental, Central y Oriental", "Pacífica, Caribe y Amazónica", "Nevada, Volcánica y Selva" },
                    correctIndex = 1,
                    explanation = "En el Macizo Colombiano la gran cordillera se bifurca en las tres cordilleras (Occidental, Central y Oriental) que recorren el territorio nacional."
                },
                new TriviaQuestion
                {
                    category = "Pisos Térmicos y Cultivos",
                    categoryEmoji = "☕",
                    questionText = "¿En qué piso térmico (clima templado, entre 1.000 y 2.000 msnm) se cultiva principalmente el café suave colombiano?",
                    options = new string[] { "Piso Cálido (0 - 1.000 m)", "Piso Templado (1.000 - 2.000 m)", "Piso Glacial / Nieves (> 4.500 m)", "Piso Páramo (3.000 - 4.500 m)" },
                    correctIndex = 1,
                    explanation = "Las laderas andinas templadas del Eje Cafetero y Santander brindan la temperatura, nubosidad y suelos volcánicos idóneos para el café arábica."
                },
                new TriviaQuestion
                {
                    category = "Fauna de Altura",
                    categoryEmoji = "🦅",
                    questionText = "¿Qué majestuosa ave habita los riscos andinos y es considerada el ave nacional y símbolo del escudo de Colombia?",
                    options = new string[] { "Flamenco rosado", "Guacamaya bandera", "Cóndor de los Andes", "Colibrí colirrufo" },
                    correctIndex = 2,
                    explanation = "El Cóndor de los Andes puede alcanzar más de 3 metros de envergadura alar y es el amo y guardián de las corrientes térmicas andinas."
                },
                new TriviaQuestion
                {
                    category = "Árbol Nacional",
                    categoryEmoji = "🌴",
                    questionText = "¿Cuál es el árbol nacional de Colombia, que habita en el Valle de Cocora (Quindío) y puede medir hasta 60 metros de altura?",
                    options = new string[] { "Ceiba gigante", "Palma de Cera del Quindío", "Eucalipto de montaña", "Pino romerón" },
                    correctIndex = 1,
                    explanation = "La Palma de Cera del Quindío es la palmera más alta del planeta y el hogar esencial del loro orejiamarillo en la cordillera central."
                }
            };
        }
    }
}

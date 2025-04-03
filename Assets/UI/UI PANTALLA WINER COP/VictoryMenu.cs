using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;

public class VictoryMenu : MonoBehaviour
{
    private UIDocument uiDocument;
    private Button returnButton;
    private VisualElement container;
    private Label congratsLabel;

    // Para animaciones
    private bool isAnimating = false;

    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        // Get references to UI elements
        container = root.Q<VisualElement>("Container");
        congratsLabel = root.Q<Label>("CongratsText");
        returnButton = root.Q<Button>("ReturnButton");

        // Add button click event
        returnButton.clicked += OnReturnButtonClicked;

        // Configuración inicial para la animación de entrada
        container.style.scale = new Scale(Vector3.one * 0.5f);
        container.style.opacity = 0;

        // Comienza la animación de entrada
        StartCoroutine(AnimateEntranceCoroutine());

        // Inicia la animación pulsante
        StartCoroutine(PulseAnimationCoroutine(congratsLabel));
    }

    private IEnumerator AnimateEntranceCoroutine()
    {
        float duration = 0.3f;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float smootht = EaseOutQuint(t);

            container.style.scale = new Scale(Vector3.Lerp(Vector3.one * 0.5f, Vector3.one, smootht));
            container.style.opacity = Mathf.Lerp(0, 1, smootht);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Asegura valores finales exactos
        container.style.scale = new Scale(Vector3.one);
        container.style.opacity = 1;
    }

    private IEnumerator PulseAnimationCoroutine(VisualElement element)
    {
        while (true)
        {
            // Crece
            float growDuration = 1f;
            float growElapsedTime = 0;

            while (growElapsedTime < growDuration)
            {
                float t = growElapsedTime / growDuration;
                float smootht = EaseInOutSine(t);

                element.style.scale = new Scale(Vector3.Lerp(Vector3.one, Vector3.one * 1.1f, smootht));

                growElapsedTime += Time.deltaTime;
                yield return null;
            }

            // Encoge
            float shrinkDuration = 1f;
            float shrinkElapsedTime = 0;

            while (shrinkElapsedTime < shrinkDuration)
            {
                float t = shrinkElapsedTime / shrinkDuration;
                float smootht = EaseInOutSine(t);

                element.style.scale = new Scale(Vector3.Lerp(Vector3.one * 1.1f, Vector3.one, smootht));

                shrinkElapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }

    private void OnReturnButtonClicked()
    {
        if (!isAnimating)
        {
            isAnimating = true;
            StartCoroutine(AnimateExitAndLoadScene());
        }
    }

    private IEnumerator AnimateExitAndLoadScene()
    {
        float duration = 0.2f;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float smootht = EaseInQuint(t);

            container.style.scale = new Scale(Vector3.Lerp(Vector3.one, Vector3.one * 1.2f, smootht));
            container.style.opacity = Mathf.Lerp(1, 0, smootht);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Asegura valores finales exactos
        container.style.scale = new Scale(Vector3.one * 1.2f);
        container.style.opacity = 0;

        // Carga la escena
        SceneManager.LoadScene("menuInicial");
    }

    // Funciones de easing personalizadas
    private float EaseOutQuint(float x)
    {
        return 1 - Mathf.Pow(1 - x, 5);
    }

    private float EaseInOutSine(float x)
    {
        return -(Mathf.Cos(Mathf.PI * x) - 1) / 2;
    }

    private float EaseInQuint(float x)
    {
        return x * x * x * x * x;
    }
}
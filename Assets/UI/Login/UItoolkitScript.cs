using UnityEngine;
using UnityEngine.UIElements;

public class StartScreenUI : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset startScreenUI; // Arrastra tu archivo UXML aquí

    private void OnEnable()
    {
        var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;
        var startScreen = startScreenUI.CloneTree();
        rootVisualElement.Add(startScreen);
    }
}

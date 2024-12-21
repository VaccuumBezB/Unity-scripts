using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConsoleHelp : MonoBehaviour
{
    [SerializeField] private DeveloperConsole developerConsole;
    private TMP_InputField inputField;

    private void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        
        // Добавляем обработчик события отправки
        inputField.onSubmit.AddListener(OnSubmit);
        
        // Добавляем обработчик нажатия Enter
        inputField.onEndEdit.AddListener((string text) => {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OnSubmit(text);
            }
        });
    }

    public void OnSubmit(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        
        developerConsole.ExecuteCommand(text.ToUpper()); // Преобразуем в верхний регистр для соответствия командам
        inputField.text = ""; // Очищаем поле ввода
        inputField.ActivateInputField(); // Возвращаем фокус на поле ввода
    }
}
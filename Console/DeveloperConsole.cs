using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class DeveloperConsole : MonoBehaviour
{
    [SerializeField] private GameObject consolePanel;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI outputText;
    [SerializeField] private ScrollRect scrollRect;

    [SerializeField] DiamondSquare ds_;
    [SerializeField] EffectManager effectManager;
    [SerializeField] InventoryManager invManager; 
    [SerializeField] ItemObject item;
    
    public bool isConsoleVisible = false;
    private List<string> commandHistory = new List<string>();
    private int historyIndex = -1;
    
    private Dictionary<string, System.Action<string[]>> commands;

    public void Awake()
    {
        InitializeCommands();
        //consolePanel.SetActive(false);
    }

    void Start()
    {
        AppendOutput(@"    __  __     ____           ____      _       __           __    ____");
        AppendOutput(@"   / / / /__  / / /___       / __ )    | |     / /___  _____/ /___/ / /");
        AppendOutput(@"  / /_/ / _ \/ / / __ \     / __  |____| | /| / / __ \/ ___/ / __  / / ");
        AppendOutput(@" / __  /  __/ / / /_/ /    / /_/ /_____/ |/ |/ / /_/ / /  / / /_/ /_/  ");
        AppendOutput(@"/_/ /_/\___/_/_/\____( )  /_____/      |__/|__/\____/_/  /_/\__,_(_)   ");
        AppendOutput(@"                     |/                                                ");
    }

    public void InitializeCommands()
    {
        commands = new Dictionary<string, System.Action<string[]>>
        {
            { "HELP", HandleHelp },
            { "SPAWN", HandleSpawn },
            { "CLEAR", HandleClear },
            { "TP", HandleTeleport },
            { "TIME", HandleTime },
            { "GOD", HandleGodMode },
            { "DESTROY", HandleDestroy },
            { "GIVEITEM", HandleGiveItem },
            { "GIVEEFFECT", HandleGiveEffect },
            { "GETSEED", HandleGetSeed }
        };
        effectManager = GameObject.FindGameObjectWithTag("Player").GetComponent<EffectManager>();
    }

    public void Update()
    {
        // Открытие/закрытие консоли по клавише тильда
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            ToggleConsole();
        }

        if (!isConsoleVisible) return;

        // История команд (стрелки вверх/вниз)
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            NavigateHistory(1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            NavigateHistory(-1);
        }
    }

    public void ToggleConsole()
    {
        isConsoleVisible = !isConsoleVisible;
        consolePanel.SetActive(isConsoleVisible);
        
        if (isConsoleVisible)
        {
            inputField.ActivateInputField();
        }
    }

    public void ExecuteCommand(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return;

        // Добавляем команду в историю
        commandHistory.Add(input);
        historyIndex = commandHistory.Count;

        // Выводим команду в консоль
        AppendOutput($"@player> {input}");

        // Разбираем команду на части
        string[] parts = input.Split(' ');
        string command = parts[0].ToUpper();
        string[] parameters = new string[parts.Length - 1];
        System.Array.Copy(parts, 1, parameters, 0, parts.Length - 1);

        // Выполняем команду
        if (commands.ContainsKey(command))
        {
            try
            {
                commands[command].Invoke(parameters);
            }
            catch (System.Exception e)
            {
                AppendOutput($"Ошибка выполнения команды(13): {e.Message}");
            }
        }
        else
        {
            AppendOutput($"Неизвестная команда: {command}");
        }

        // Очищаем поле ввода и фокусируемся на нем
        inputField.text = "";
        inputField.ActivateInputField();
    }

    public void NavigateHistory(int direction)
    {
        if (commandHistory.Count == 0) return;

        historyIndex = Mathf.Clamp(historyIndex + direction, 0, commandHistory.Count - 1);
        inputField.text = commandHistory[historyIndex];
        inputField.caretPosition = inputField.text.Length;
    }

    public void AppendOutput(string message)
    {
        outputText.text += $"\n{message}";
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    #region Command Handlers

    public void HandleHelp(string[] parameters)
    {
        AppendOutput("Доступные команды:");
        AppendOutput("HELP - Показать это сообщение");
        AppendOutput("SPAWN [prefabName] - Создать обьект");
        AppendOutput("CLEAR - Очистить вывод консоли");
        AppendOutput("TP [x] [y] [z] - Переместить игрока");
        AppendOutput("TIME [value] - Установить время суток");
        AppendOutput("GOD - Переключить режим бога");
        AppendOutput("DESTROY - Уничтожить обьект по линии взгляда");
        AppendOutput("GIVEITEM [item] [amount] - Выдаёт предмет игроку");
        AppendOutput("GIVEEFFECT [effectName] - Выдаёт эффект игроку");
        AppendOutput("SEED - Возвращает ключ-генератор мира");
    }

    public void HandleSpawn(string[] parameters)
    {
        if (parameters.Length == 0)
        {
            AppendOutput("Синтаксис: spawn [prefabName]");
            return;
        }

        string prefabName = parameters[0];
        GameObject prefab = Resources.Load<GameObject>($"Prefabs/{prefabName}");
        
        if (prefab == null)
        {
            AppendOutput($"Обьект '{prefabName}' не найден");
            return;
        }

        Vector3 spawnPosition = Camera.main.transform.position + Camera.main.transform.forward * 5f;
        Instantiate(prefab, spawnPosition, Quaternion.identity);
        AppendOutput($"Создан обьект '{prefabName}'");
    }

    public void HandleClear(string[] parameters)
    {
        outputText.text = "";
        AppendOutput(@"    __  __     ____           ____      _       __           __    ____");
        AppendOutput(@"   / / / /__  / / /___       / __ )    | |     / /___  _____/ /___/ / /");
        AppendOutput(@"  / /_/ / _ \/ / / __ \     / __  |____| | /| / / __ \/ ___/ / __  / / ");
        AppendOutput(@" / __  /  __/ / / /_/ /    / /_/ /_____/ |/ |/ / /_/ / /  / / /_/ /_/  ");
        AppendOutput(@"/_/ /_/\___/_/_/\____( )  /_____/      |__/|__/\____/_/  /_/\__,_(_)   ");
        AppendOutput(@"                     |/                                                ");
    }

    public void HandleTeleport(string[] parameters)
    {
        if (parameters.Length != 3)
        {
            AppendOutput("Синтаксис: tp [x] [y] [z]");
            return;
        }

        if (float.TryParse(parameters[0], out float x) &&
            float.TryParse(parameters[1], out float y) &&
            float.TryParse(parameters[2], out float z))
        {
            Vector3 _position = new Vector3(x, y, z);
            if (GameObject.FindGameObjectWithTag("Player") is GameObject player)
            {
                player.transform.position = _position;
                AppendOutput($"Игрок перенесён в позицию {_position}");
            }
        }
        else
        {
            AppendOutput("Неверные координаты");
        }
    }

    public void HandleGetSeed(string[] parameters)
    {
        AppendOutput((ds_.seed).ToString());
    }

    public void HandleTime(string[] parameters)
    {
        if (parameters.Length != 1)
        {
            AppendOutput("Синтаксис: time [0-24]");
            return;
        }

        if (float.TryParse(parameters[0], out float time))
        {
            
            AppendOutput($"Установлено время {time}");
        }
        else
        {
            AppendOutput("Неверное значение времени");
        }
    }

    public void HandleGodMode(string[] parameters)
    {
        // Здесь нужно реализовать включение/выключение режима бога
        AppendOutput("Режим бога переключен");
    }

    public void HandleGiveEffect(string[] parameters)
    {
        if (parameters.Length != 1)
        {
            AppendOutput("Синтаксис: GIVEEFFECT [effectName]");
            return;
        }

        string effectName = parameters[0];
        effectManager.AddEffect(Resources.Load<Effect>($"PlayerEffects/Effects/{effectName}"));

        AppendOutput($"Эффект {effectName} выдан");
    }

    public void HandleDestroy(string[] parameters)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject objectToDestroy = hit.collider.gameObject;
            string objectName = objectToDestroy.name;
            Destroy(objectToDestroy);
            AppendOutput($"Уничтожен объект: {objectName}");
        }
        else
        {
            AppendOutput("Объект не найден");
        }
    }

    public void HandleGiveItem(string[] parameters)
    {
        if (parameters.Length != 2)
        {
            AppendOutput("Синтаксис: GIVEITEM [название_предмета] [количество]");
            return;
        }

        if (!int.TryParse(parameters[1], out int amount))
        {
            AppendOutput("Неверное количество предметов");
            return;
        }

        string itemName = parameters[0];
        
        // Здесь можно добавить поиск предмета по имени в будущем
        if (item != null)
        {
            invManager.AddItem(item, amount);
            AppendOutput($"Выдано {amount} {item.name}");
        }
        else
        {
            AppendOutput($"Предмет '{itemName}' не найден");
        }
    }
    #endregion
} 
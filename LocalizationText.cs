/*using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
//using Newtonsoft.Json;

public class LocalizationText : MonoBehaviour
{
    [SerializeField] private bool TMP;
    private readonly HttpClient _httpClient = new HttpClient();
    private readonly string _apiUrl = "https://libretranslate.com";
    private readonly string _apiKey = null;

    private class TranslationResult
    {
        public string TranslatedText { get; set; }
    }

    private class LanguageInfo
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    private void Start()
    {
        SetLang(PlayerPrefs.GetString("lang", "ru")); // Русский по умолчанию
    }

    public async void SetLang(string lang)
    {
        try
        {
            string textToTranslate;
            if (TMP)
            {
                TMP_Text tmpText = GetComponent<TMP_Text>();
                textToTranslate = tmpText.text;
                string translatedText = await TranslateAsync(textToTranslate, lang);
                tmpText.text = translatedText;
            }
            else
            {
                Text uiText = GetComponent<Text>();
                textToTranslate = uiText.text;
                string translatedText = await TranslateAsync(textToTranslate, lang);
                uiText.text = translatedText;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Ошибка перевода: {ex.Message}");
        }
    }

    private async Task<string> TranslateAsync(string text, string targetLang, string sourceLang = "auto")
    {
        try
        {
            var payload = new
            {
                q = text,
                source = sourceLang,
                target = targetLang,
                api_key = _apiKey
            };

            var content = new StringContent(
                JsonUtility.ToJson(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync($"{_apiUrl}/translate", content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            var translationResult = JsonUtility.FromJson<TranslationResult>(result);

            return translationResult.TranslatedText;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Ошибка перевода: {ex.Message}");
            return text; // Возвращаем исходный текст в случае ошибки
        }
    }

    async Task<List<LanguageInfo>> GetLanguagesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_apiUrl}/languages");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            var languages = JsonUtility.FromJson<List<LanguageInfo>>(result);
            return languages;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Ошибка получения списка языков: {ex.Message}");
            return new List<LanguageInfo>();
        }
    }
}
*/
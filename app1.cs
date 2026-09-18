using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

class DictionaryApp
{
    private Dictionary<string, Dictionary<string, List<string>>> dictionaries = new();
    private string currentDict = "";

    public void Run()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Словники ===");
            if (string.IsNullOrEmpty(currentDict))
            {
                Console.WriteLine("1. Створити словник");
                Console.WriteLine("2. Вибрати словник");
                Console.WriteLine("3. Вихід");
                var choice = Console.ReadLine();
                if (choice == "1") CreateDictionary();
                else if (choice == "2") SelectDictionary();
                else if (choice == "3") break;
            }
            else
            {
                Console.WriteLine($"Поточний словник: {currentDict}");
                Console.WriteLine("1. Додати слово та переклад");
                Console.WriteLine("2. Змінити слово або переклад");
                Console.WriteLine("3. Видалити слово або переклад");
                Console.WriteLine("4. Шукати переклад слова");
                Console.WriteLine("5. Експортувати слово");
                Console.WriteLine("6. Зберегти у файл");
                Console.WriteLine("7. Завантажити з файлу");
                Console.WriteLine("8. Назад до вибору словника");
                var choice = Console.ReadLine();
                if (choice == "1") AddWord();
                else if (choice == "2") EditWord();
                else if (choice == "3") DeleteWord();
                else if (choice == "4") SearchWord();
                else if (choice == "5") ExportWord();
                else if (choice == "6") SaveToFile();
                else if (choice == "7") LoadFromFile();
                else if (choice == "8") currentDict = "";
            }
        }
    }

    private void CreateDictionary()
    {
        Console.Write("Введіть назву словника (напр. англо-український): ");
        var name = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(name) && !dictionaries.ContainsKey(name))
        {
            dictionaries[name] = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            currentDict = name;
        }
    }

    private void SelectDictionary()
    {
        if (dictionaries.Count == 0) { Console.WriteLine("Немає словників."); Console.ReadKey(); return; }
        foreach (var d in dictionaries.Keys) Console.WriteLine($"- {d}");
        Console.Write("Виберіть словник: ");
        var name = Console.ReadLine();
        if (dictionaries.ContainsKey(name)) currentDict = name;
    }

    private void AddWord()
    {
        Console.Write("Введіть слово: ");
        var word = Console.ReadLine()?.Trim();
        Console.Write("Введіть переклад (через кому, якщо кілька): ");
        var translations = Console.ReadLine()?.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)).ToList();
        
        if (string.IsNullOrEmpty(word) || translations == null || translations.Count == 0) return;

        if (!dictionaries[currentDict].ContainsKey(word))
            dictionaries[currentDict][word] = new List<string>();

        foreach (var t in translations)
        {
            if (!dictionaries[currentDict][word].Contains(t, StringComparer.OrdinalIgnoreCase))
                dictionaries[currentDict][word].Add(t);
        }
    }

    private void EditWord()
    {
        Console.Write("Введіть слово для редагування: ");
        var word = Console.ReadLine()?.Trim();
        if (word == null || !dictionaries[currentDict].ContainsKey(word)) return;

        Console.WriteLine("1. Змінити саме слово");
        Console.WriteLine("2. Змінити конкретний переклад");
        var ch = Console.ReadLine();
        if (ch == "1")
        {
            Console.Write("Нове слово: ");
            var newWord = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(newWord))
            {
                var trans = dictionaries[currentDict][word];
                dictionaries[currentDict].Remove(word);
                dictionaries[currentDict][newWord] = trans;
            }
        }
        else if (ch == "2")
        {
            var list = dictionaries[currentDict][word];
            for (int i = 0; i < list.Count; i++) Console.WriteLine($"{i + 1}. {list[i]}");
            Console.Write("Номер перекладу для зміни: ");
            if (int.TryParse(Console.ReadLine(), out int idx) && idx >= 1 && idx <= list.Count)
            {
                Console.Write("Новий варіант перекладу: ");
                var newT = Console.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(newT)) list[idx - 1] = newT;
            }
        }
    }

    private void DeleteWord()
    {
        Console.Write("Введіть слово: ");
        var word = Console.ReadLine()?.Trim();
        if (word == null || !dictionaries[currentDict].ContainsKey(word)) return;

        Console.WriteLine("1. Видалити все слово");
        Console.WriteLine("2. Видалити конкретний переклад");
        var ch = Console.ReadLine();
        if (ch == "1")
        {
            dictionaries[currentDict].Remove(word);
        }
        else if (ch == "2")
        {
            var list = dictionaries[currentDict][word];
            if (list.Count <= 1) { Console.WriteLine("Неможливо видалити останній переклад."); Console.ReadKey(); return; }
            for (int i = 0; i < list.Count; i++) Console.WriteLine($"{i + 1}. {list[i]}");
            Console.Write("Номер перекладу для видалення: ");
            if (int.TryParse(Console.ReadLine(), out int idx) && idx >= 1 && idx <= list.Count)
            {
                list.RemoveAt(idx - 1);
            }
        }
    }

    private void SearchWord()
    {
        Console.Write("Введіть слово для пошуку: ");
        var word = Console.ReadLine()?.Trim();
        if (word != null && dictionaries[currentDict].TryGetValue(word, out var list))
        {
            Console.WriteLine($"Переклади: {string.Join(", ", list)}");
        }
        else
        {
            Console.WriteLine("Слово не знайдено.");
        }
        Console.ReadKey();
    }

    private void ExportWord()
    {
        Console.Write("Введіть слово для експорту: ");
        var word = Console.ReadLine()?.Trim();
        if (word != null && dictionaries[currentDict].TryGetValue(word, out var list))
        {
            File.WriteAllLines($"{word}_export.txt", new[] { $"{word}: {string.Join(", ", list)}" });
            Console.WriteLine("Експортовано.");
        }
        Console.ReadKey();
    }

    private void SaveToFile()
    {
        var json = JsonSerializer.Serialize(dictionaries);
        File.WriteAllText("dictionaries.json", json);
        Console.WriteLine("Збережено.");
        Console.ReadKey();
    }

    private void LoadFromFile()
    {
        if (File.Exists("dictionaries.json"))
        {
            var json = File.ReadAllText("dictionaries.json");
            dictionaries = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<string>>>>(json) ?? dictionaries;
            Console.WriteLine("Завантажено.");
        }
        Console.ReadKey();
    }
}

class Program
{
    static void Main()
    {
        new DictionaryApp().Run();
    }
}

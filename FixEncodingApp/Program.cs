using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

class Program
{
    static void Main()
    {
        string rootDir = @"d:\projeler\Proje\webProje\HastaneYonetimSistemi\HastaneYonetimSistemi";
        
        var replacements = new Dictionary<string, string>
        {
            { "ÅŸ", "ş" },
            { "Åž", "Ş" },
            { "Ä±", "ı" },
            { "Ä°", "İ" },
            { "Ã§", "ç" },
            { "Ã‡", "Ç" },
            { "Ã¶", "ö" },
            { "Ã–", "Ö" },
            { "Ã¼", "ü" },
            { "Ãœ", "Ü" },
            { "ÄŸ", "ğ" },
            { "Äž", "Ğ" }
        };

        var files = Directory.GetFiles(rootDir, "*.*", SearchOption.AllDirectories);
        
        foreach (var file in files)
        {
            if (file.EndsWith(".cs") || file.EndsWith(".cshtml"))
            {
                string content = File.ReadAllText(file, Encoding.UTF8);
                bool changed = false;
                
                foreach (var kvp in replacements)
                {
                    if (content.Contains(kvp.Key))
                    {
                        content = content.Replace(kvp.Key, kvp.Value);
                        changed = true;
                    }
                }
                
                if (changed)
                {
                    Console.WriteLine($"Fixing {file}");
                    File.WriteAllText(file, content, new UTF8Encoding(true)); // true for BOM
                }
            }
        }
    }
}

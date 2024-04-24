using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace sanitizzazioneLPG.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ImportaBottone.Click += ImportaBottone_Clicked;
    }

    private async void ImportaBottone_Clicked(object? sender, RoutedEventArgs args)
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(this);

        // Start async operation to open the dialog.
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Selezione file JSON",
            AllowMultiple = false,
            FileTypeFilter = new[] { Json }
        });

        if (files.Count >= 1)
        {
            // Open reading stream from the first file.
            await using var stream = await files[0].OpenReadAsync();
            using var streamReader = new StreamReader(stream);
            // Reads all the content of file as a text.
            Console.Write(await streamReader.ReadToEndAsync());
        }
        else 
            Console.WriteLine("errore: scegliere un file");
    }

    private static FilePickerFileType Json { get; } = new("File JSON")
    {
        Patterns = new[] { "*.json", "*.JSON", "*.Json" },
    };
}
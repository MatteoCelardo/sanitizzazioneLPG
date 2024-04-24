using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using sanitizzazioneLPG.Servizio;

namespace sanitizzazioneLPG.ModelliViste;

public partial class HomeModelloVista : ObservableObject
{
    private readonly IServizio _s;
    
    [ObservableProperty]
    private string? _percorso = "prova";

    public HomeModelloVista(IServizio s) 
    {
        _s = s; 
    }

    public HomeModelloVista() 
    {
        _s = Gestore.Istanza; 
    }

    [RelayCommand]
    private void ImportaJSON()
    {
        Console.WriteLine("prova");

        /*
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
            */
    }

    /*
    private static FilePickerFileType Json { get; } = new("File JSON")
    {
        Patterns = new[] { "*.json", "*.JSON", "*.Json" },
    };
    */

    /*
    [RelayCommand(CanExecute = nameof(CanNome))]
    private void ImportaJSON()
    {
        Console.WriteLine("prova");
    }
    private bool CanNome() => condizione da verificare peché il bottone sia cliccabile

    mettere sopra la variabile che contiene i dati da controllare: 
    [NotifyCanExecuteChangedFor(nameOf(CanNome))] → ogni volta che cambia viene verificato CanNome
    */
}

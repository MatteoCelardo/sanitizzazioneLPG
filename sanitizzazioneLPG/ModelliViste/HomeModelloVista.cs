using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using sanitizzazioneLPG.Servizio;

namespace sanitizzazioneLPG.ModelliViste;

public partial class HomeModelloVista : ObservableObject
{
    private readonly IServizio _s;
    
    // permette di notificare alla view che il valore di _percorso è cambiato e va 
    // aggiornato tramite Percorso 
    [ObservableProperty]
    // permette di rieseguire la canExecute su CancellaJSONCommand ogni volta che 
    // _percorso cambia valore
    [NotifyCanExecuteChangedFor(nameof(CancellaJSONCommand))]
    [NotifyCanExecuteChangedFor(nameof(ValidaJSONCommand))]
    [NotifyCanExecuteChangedFor(nameof(SanitizzaDBCommand))]
    private string? _percorso;

    [ObservableProperty]
    private string? _codiceJson;

    public HomeModelloVista(IServizio s) 
    {
        _s = s; 
    }

    public HomeModelloVista() 
    {
        _s = Gestore.Istanza; 
    }

    #region funzioni bottoni
    // permette di creare un comando richiamabile dalla view che esegue questa funzione. 
    // il nome del comando sarà sempre nomeFunzioneCommand
    [RelayCommand]
    private async Task ImportaJSON()
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        TopLevel topLevel = TopLevel.GetTopLevel(((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow);

        // Start async operation to open the dialog.
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Selezione file JSON",
            AllowMultiple = false,
            FileTypeFilter = new[] { Json }
        });

        if (files.Count >= 1)
        {
            Percorso = files[0].Path.AbsolutePath;
            _s.ImportaJSON(Percorso);
            await using var stream = await files[0].OpenReadAsync();
            StreamReader streamReader = new StreamReader(stream);
            CodiceJson = streamReader.ReadToEnd();
        }
        else 
            _s.MostraMsg("Errore","Scegliere un file JSON da importare");
    }

    // funzione per specificare al file picker che deve prendere JSON
    private static FilePickerFileType Json { get; } = new("File JSON")
    {
        Patterns = new[] { "*.json", "*.JSON", "*.Json" },
    };

    // CanExecute permette di specificare la funzione che determina se la funzione 
    // sia eseguibile o meno
    [RelayCommand(CanExecute = nameof(JsonPresente))]
    private void CancellaJSON()
    {
        _s.CancellaJSON();
        Percorso = null;
        CodiceJson = null;
    }

    [RelayCommand(CanExecute = nameof(JsonPresente))]
    private void ValidaJSON()
    {
        _s.ValidaJSON(Percorso);
    }

    [RelayCommand(CanExecute = nameof(JsonPresente))]
    private void SanitizzaDB()
    {
        _s.SanitizzaDB(EnumSanit.CANC);
    }

    //funzione che controlla la condizione per la quale i bottoni sono cliccabili
    private bool JsonPresente() => !string.IsNullOrEmpty(Percorso);

    #endregion
}

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
        Console.WriteLine("prova");;
    }

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

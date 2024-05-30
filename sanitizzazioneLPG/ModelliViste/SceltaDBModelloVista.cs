using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using sanitizzazioneLPG.Servizio;
using sanitizzazioneLPG.Viste;

namespace sanitizzazioneLPG.ModelliViste;

public partial class SceltaDBModelloVista : ModelloVistaBase
{
    private readonly IServizio _s;

    // URI usata per raggiungere il DB

    // permette di notificare alla view che il valore di _uri è cambiato e va 
    // aggiornato tramite Uri
    [ObservableProperty]
    // permette di rieseguire la canExecute su LoginCommand ogni volta che 
    // _uri cambia valore
    [NotifyCanExecuteChangedFor(nameof(ConnessioneCommand))]
    private string? _uri;

    // username dell'utente del DB
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnessioneCommand))]
    private string? _usr;

    // password dell'utente del DB
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnessioneCommand))]
    private string? _pwd;

    [ObservableProperty]
    private string? _err;

    public SceltaDBModelloVista(IServizio s)
    {
        _s = s;
    }

    public SceltaDBModelloVista()
    {
        _s = Gestore.Istanza;
    }

    // permette di creare un comando richiamabile dalla view che esegue questa funzione. 
    // il nome del comando sarà sempre nomeFunzioneCommand.
    // CanExecute permette di specificare la funzione che determina se la funzione 
    // sia eseguibile o meno
    [RelayCommand(CanExecute = nameof(DatiPresenti))]
    private void Connessione()
    {
        _s.ConnettiDB(Usr,Pwd,Uri);
    }

    private bool DatiPresenti()
    {
        return !string.IsNullOrEmpty(Uri) 
            && !string.IsNullOrEmpty(Usr)
            && !string.IsNullOrEmpty(Pwd);
    }
}

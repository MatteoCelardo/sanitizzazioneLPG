using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using sanitizzazioneLPG.Dominio;

namespace sanitizzazioneLPG.ModelliViste;

public partial class ContainerModelloVista : ModelloVistaBase
{
    // permette di fare il binding con la GUI e mantenere i dati mostrati coerenti col 
    // valore salvato nella variabile
    [ObservableProperty]
    private ModelloVistaBase _pagCorrente;

    private readonly ObservableCollection<TemplateOggPannello> _templates; 

    // booleano per sapere se il pannello sia aperto o chiuso
    [ObservableProperty]
    private bool _panAperto;

    // tiene traccia dell'oggetto selezionato nel pannello
    [ObservableProperty]
    private TemplateOggPannello? _oggSel;

    public ContainerModelloVista()
    {
        _pagCorrente = new SceltaDBModelloVista();
        _panAperto = false;
        _templates = new ObservableCollection<TemplateOggPannello>(
            [
                new TemplateOggPannello(typeof(SceltaDBModelloVista), "LockRegular", "Seleziona LPG"),
                new TemplateOggPannello(typeof(HomeModelloVista), "FingerprintRegular", "Sanitizzazione"),
            ]);
        OggSel = Templates.First(vm => vm.Modello == typeof(SceltaDBModelloVista));
    }
    

    partial void OnOggSelChanged(TemplateOggPannello? value)
    {
        if (value is null) return;

        var modVista = Activator.CreateInstance(value.Modello);

        if (modVista is not ModelloVistaBase modVistaBase) return;

        PagCorrente = modVistaBase;
    }

    [RelayCommand]
    private void TogglePan()
    {
        PanAperto = !PanAperto;
    }

    public ObservableCollection<TemplateOggPannello> Templates { get => _templates; }
}

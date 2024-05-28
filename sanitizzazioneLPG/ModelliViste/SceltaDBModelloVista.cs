using System;
using CommunityToolkit.Mvvm.ComponentModel;
using sanitizzazioneLPG.Servizio;

namespace sanitizzazioneLPG.ModelliViste;

public class SceltaDBModelloVista : ObservableObject
{
    private readonly IServizio _s;

    public SceltaDBModelloVista(IServizio s)
    {
        _s = s;
    }

    public SceltaDBModelloVista()
    {
        _s = Gestore.Istanza;
    }
}

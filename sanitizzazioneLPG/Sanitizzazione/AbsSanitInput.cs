using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using sanitizzazioneLPG.Dominio;

namespace sanitizzazioneLPG.Sanitizzazione;

public abstract class AbsSanitInput
{
    // lista dei caratteri speciali da sanitizzare
    private static readonly IList<string> carEsc = new ReadOnlyCollection<string>(
            new List<string>(["\'","\"","\\","/*","//"])
        );
        
    public static List<Nodo> SanitizzaNodi(List<Nodo> nodi)
    {
        throw new NotImplementedException();
    }

    public static List<Relazione> SanitizzaRelazioni(List<Relazione> relazioni)
    {
        throw new NotImplementedException();
    }

    public static List<Catena> SanitizzaCatene(List<Catena> catene)
    {
        throw new NotImplementedException();
    }
}

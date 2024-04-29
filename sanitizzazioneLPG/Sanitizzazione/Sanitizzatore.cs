using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using sanitizzazioneLPG.Dominio;

namespace sanitizzazioneLPG.Sanitizzazione;

public class Sanitizzatore : ISanit
{
    // lista dei caratteri speciali da sanitizzare
    private static readonly IList<string> carEsc = new ReadOnlyCollection<string>(
            new List<string>(["\'","\"","\\","/*","//"])
        );
    // lista delle parole chiave da sanitizzare
    private static readonly IList<string> parChiave = new ReadOnlyCollection<string>(
            new List<string>(["RETURN", "AS", "LOAD", "OR", "AND", "CALL", "WITH"])
        );
        
    public static DaSanitizzareNodo_ SanitizzaNodo(DaSanitizzareNodo_ dsn)
    {
        throw new NotImplementedException();
    }

    public static DaSanitizzareRel_ SanitizzaRel(DaSanitizzareRel_ dsr)
    {
        throw new NotImplementedException();
    }
}

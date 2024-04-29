using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using sanitizzazioneLPG.Dominio;

namespace sanitizzazioneLPG.Sanitizzazione;

public interface ISanit
{
    // lista dei caratteri speciali da sanitizzare
    protected static readonly IList<string> carEsc = new ReadOnlyCollection<string>(
            new List<string>(["\'","\"","\\","/*","//"])
        );
    // lista delle parole chiave da sanitizzare
    protected static readonly IList<string> parChiave = new ReadOnlyCollection<string>(
            new List<string>(["RETURN", "AS", "LOAD", "OR", "AND", "CALL", "WITH"])
        );

    /// // <summary>
    /// Sanitizza tutti i campi dell'oggetto <c>DaSanitizzareRel_</c> e degli oggetti 
    /// che contiene
    /// </summary>
    /// <param name="dsr">Oggetto da sanitizzare</param>
    /// <returns>Oggetto sanitizzato</returns>
    static abstract DaSanitizzareRel_ SanitizzaRel(DaSanitizzareRel_ dsr); 

    /// <summary>
    /// Sanitizza tutti i campi dell'oggetto <c>DaSanitizzareNodo_</c> e degli oggetti 
    /// che contiene
    /// </summary>
    /// <param name="dsn">Oggetto da sanitizzare</param>
    /// <returns>Oggetto sanitizzato</returns>
    static abstract DaSanitizzareNodo_ SanitizzaNodo(DaSanitizzareNodo_ dsn);
}

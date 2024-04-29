using sanitizzazioneLPG.Dominio;

namespace sanitizzazioneLPG.Sanitizzazione;

public interface ISanit
{
    /// <summary>
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

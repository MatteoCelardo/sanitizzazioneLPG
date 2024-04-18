namespace sanitizzazioneLPG;

public interface IPers
{
    /// <summary>
    /// Inseririsce nella persistenza gli oggetti del dominio 
    /// da sanitizzare
    /// </summary>
    /// <remarks>Il metodo suppone che sia già stata fatta la validazione del file specificato</remarks>
    /// <param name="path">Percorso al file JSON con le istruzioni di sanitizzazione</param>
    /// <exception cref="PersEcc">Eccezione sollevata nel caso in cui il file specificato sia vuoto</exception>
    void Crea(string path);

    /// <summary>
    /// cancella il contenuto della persistenza
    /// </summary>
    void Cancella();

    /// <summary>
    /// Valida il file fornito
    /// </summary>
    /// <param name="path">Percorso al file JSON da validare</param>
    /// <returns>
    /// Ritorna una lista vuota se non ci sono errori, la lista degli errori altrimenti 
    /// </returns>
    List<string> Valida(string path);
    
    /// <summary>
    /// permette di ottenere tutti gli oggetti presenti nella persistenza 
    /// </summary>
    /// <param name="etd">Tipo di oggetti del dominio di cui si vuole avere la lista</param>
    /// <exception cref="PersEcc">Eccezione sollevata nel caso la lista degli oggetti richiesti sia vuota</exception>
    List<IDom> ListAll(EnumTipoDom etd);
}

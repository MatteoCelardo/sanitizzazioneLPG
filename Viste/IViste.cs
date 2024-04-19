namespace sanitizzazioneLPG;

public interface IViste
{
    /// <summary>
    /// richiede l’importazione del file JSON che contiene la specifica degli elementi 
    /// da sanitizzare nel DB
    /// </summary>
    void importaJSON();
    /// <summary>
    /// richiede la cancellazione delle informazioni relative al file JSON caricato 
    /// in precedenza con la funzione <c>importaJSON()</c>
    /// </summary>
    void cancellaJSON();
    /// <summary>
    /// richiede la validazione di un file JSON prima che venga importato
    /// </summary>
    void validaJSON();
    /// <summary>
    /// richiede la sanitizzazione del DB secondo le specifiche date dal file JSON 
    /// importato con <c>importaJSON()</c>
    /// </summary>
    void sanitizzaDB();
}

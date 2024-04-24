namespace sanitizzazioneLPG.Servizio;

public interface IServizio
{
    /// <summary>
    /// importa all'interno della persistenza il contenuto del file JSON con 
    /// la specifica degli elementi da sanitizzare nel DB
    /// </summary>
    void ImportaJSON(string path);
    /// <summary>
    /// cancella le informazioni relative al file JSON caricato 
    /// in precedenza con la funzione <c>importaJSON()</c>
    /// </summary>
    void CancellaJSON();
    /// <summary>
    /// valida lo schema di un file JSON prima che venga importato
    /// </summary>
    bool ValidaJSON(string path);
    /// <summary>
    /// sanitizza il DB secondo le specifiche date dal file JSON 
    /// importato con <c>importaJSON()</c>
    /// </summary>
    void SanitizzaDB(EnumSanit s);
    /// <summary>
    /// permette di mostrare un popup contenente un messaggio e il tasto ok
    /// </summary>
    /// <param name="titolo">titolo della finestra popup</param>
    /// <param name="msg">messaggio da mostrare</param>
    void MostraMsg(string titolo, string msg);
}

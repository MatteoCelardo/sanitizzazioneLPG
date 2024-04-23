namespace sanitizzazioneLPG.ViewModels;

public interface IMainWindowVM
{
    /// <summary>
    /// importa all'interno della persistenza il contenuto del file JSON con 
    /// la specifica degli elementi da sanitizzare nel DB
    /// </summary>
    void importaJSON(string path);
    /// <summary>
    /// cancella le informazioni relative al file JSON caricato 
    /// in precedenza con la funzione <c>importaJSON()</c>
    /// </summary>
    void cancellaJSON();
    /// <summary>
    /// valida lo schema di un file JSON prima che venga importato
    /// </summary>
    void validaJSON(string path);
    /// <summary>
    /// sanitizza il DB secondo le specifiche date dal file JSON 
    /// importato con <c>importaJSON()</c>
    /// </summary>
    void sanitizzaDB(EnumSanit s);
}

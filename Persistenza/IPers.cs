namespace sanitizzazioneLPG;

public interface IPers
{
    /// <summary>
    /// Funzione usata per inserire nella persistenza corrente gli oggetti del dominio 
    /// da sanitizzare
    /// </summary>
    /// <param name="path">Percorso al file JSON con le istruzioni di sanitizzazione</param>
    void crea(string path);

    /// <summary>
    /// cancella il contenuto della persistenza
    /// </summary>
    void cancella();
    
    /// <summary>
    /// permette di ottenere tutti gli oggetti presenti nella persistenza 
    /// </summary>
    /// <param name="etd">Tipo di oggetti del dominio di cui si vuole avere la lista</param>
    List<IDom> listAll(EnumTipoDom etd);
}

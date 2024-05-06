using System.Collections.Generic;
using Newtonsoft.Json.Schema;
using sanitizzazioneLPG.Dominio;

namespace sanitizzazioneLPG.Persistenza;

public interface IPers
{
    /// <summary>
    /// Inseririsce nella persistenza gli oggetti del dominio 
    /// da sanitizzare
    /// </summary>
    /// <remarks>Il metodo suppone che sia già stata fatta la validazione del file specificato.
    /// Si ricorda che nelle catene devono alternarsi nodi e relazioni, motivo 
    /// per cui inserire due nodi o due relazioni di fila porterà alla generazione 
    /// dell'eccezione <c>PersExcEmpty</c>
    /// </remarks>
    /// <param name="path">Percorso al file JSON con le istruzioni di sanitizzazione</param>
    /// <exception cref="PersExcDupl">
    /// Eccezione sollevata nel caso una catena contenga due elementi dello stesso tipo 
    /// in fila (nodo-nodo, relazione-relazione)
    /// </exception>
    /// <exception cref="PersExcNotFound">
    /// Eccezione sollevata nel caso un id specificato in una catena non sia presente
    /// oppure se non si è già letto il file dal disco con la funzione di validazione
    /// </exception>
    /// <exception cref="PersExc">
    /// Eccezione sollevata nel caso in cui l'oggetto di una catena sia dello stesso 
    /// tipo del precedente oppure se si cerca di importare informazioni da un nuovo 
    /// file JSON senza prima svuotare la persistenza col metodo <c>Cancella()</c>
    /// </exception>
    void Importa();

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
    IList<ValidationError> Valida(string path);
    
    /// <summary>
    /// permette di ottenere tutti gli oggetti presenti nella persistenza 
    /// </summary>
    /// <param name="etd">Tipo di oggetti del dominio di cui si vuole avere la lista</param>
    /// <exception cref="PersExcNotFound">Eccezione sollevata nel caso la lista degli oggetti richiesti sia vuota</exception>
    List<IDom> ListAll(EnumTipoDom etd);
}

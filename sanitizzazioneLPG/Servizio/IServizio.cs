﻿using MsBox.Avalonia.Enums;

namespace sanitizzazioneLPG.Servizio;

public interface IServizio
{
    /// <summary>
    /// effettua la connessione al database specificato per la sanitizzazione
    /// </summary>
    /// <param name="usr">nome utente del DB</param>
    /// <param name="pwd">password relativa a <c>usr</c></param>
    /// <param name="uri">uri per raggiungere il DB</param>
    void ConnettiDB(string usr, string pwd, string uri);
    /// <summary>
    /// importa all'interno della persistenza il contenuto del file JSON con 
    /// la specifica degli elementi da sanitizzare nel DB
    /// </summary>
    /// <param name="path">percorso al file da importare</param>
    /// <returns><c>true</c> se l'importazione è andata a buon fine, <c>false</c> altrimenti</returns>
    bool ImportaJSON(string path);
    /// <summary>
    /// cancella le informazioni relative al file JSON caricato 
    /// in precedenza con la funzione <c>importaJSON()</c>
    /// </summary>
    void CancellaJSON();
    /// <summary>
    /// valida lo schema di un file JSON con quello predefinito
    /// </summary>
    /// <param name="path">percorso al file da importare</param>
    /// <returns>
    /// stringa formattata con gli errori riscontrati o stringa vuota in assenza 
    /// di errori
    /// /returns>
    string ValidaJSON(string path);
    /// <summary>
    /// sanitizza il DB secondo le specifiche date dal file JSON 
    /// importato con <c>importaJSON()</c>
    /// </summary>
    /// <param name="s">indica che azione vada intesa come sanitizzazione</param>
    void SanitizzaDB(EnumSanit s);
    /// <summary>
    /// permette di mostrare un popup contenente un messaggio e il tasto ok
    /// </summary>
    /// <param name="titolo">titolo della finestra popup</param>
    /// <param name="msg">messaggio da mostrare</param>
    /// <param name="i">icona da mostrare</param>
    /// <param name="b">bottoni da mostrare</param>
    void MostraMsg(string titolo, string msg, Icon i, ButtonEnum b);
}

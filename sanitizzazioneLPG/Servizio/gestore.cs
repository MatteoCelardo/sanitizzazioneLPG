using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Newtonsoft.Json.Schema;
using sanitizzazioneLPG.Persistenza;

namespace sanitizzazioneLPG.Servizio;

public class Gestore : IServizio
{
    private readonly IPers _pers;

    #region singleton
    private static Gestore? _istanza = null;
    private static object _mutex = new object();

    private Gestore()
    {
        this._pers = Pers.Istanza;
    }

    public static Gestore Istanza 
    {
        get 
        {
            lock(_mutex)
            {
                return _istanza ?? new Gestore();
            }
        }
    }
    #endregion

    public void CancellaJSON()
    {
        this._pers.Cancella();
        this.MostraMsg("Info", "Cancellazione del precedente file JSON importato completata correttamente",Icon.Info,ButtonEnum.Ok);
    }

    public void ImportaJSON(string path)
    {
        string err;
        try
        {
            err = this.Valida(path);
            if (string.IsNullOrEmpty(err))
            {
                this._pers.Crea(path);
                this.MostraMsg("Info", "Importazione del file JSON portata a termine correttamente",Icon.Info,ButtonEnum.Ok);
            }
            else 
                this.MostraMsg("Errore", err,Icon.Error,ButtonEnum.Ok);
            
        }
        catch (PersExc e)
        {
            
            if (e is PersExcDupl || e is PersExcNotFound)
                this.CancellaJSON();
                this.MostraMsg("Errore", e.Message + "\nI dati importati in precedenza verranno cancellati.",Icon.Error,ButtonEnum.Ok);
        }
    }

    public async void SanitizzaDB(EnumSanit s)
    {
        List<string> queryNodi;
        List<string> queryRel;
        List<string> queryCat;

        switch(s)
        {
            case EnumSanit.CANC: 
                queryRel = await Task.Run(GeneraQueryNodi);
                queryNodi = await Task.Run(GeneraQueryRel);
                queryCat = await Task.Run(GeneraQueryCat);
                break;
            default: 
                this.MostraMsg("Errore", "Il tipo di sanitizzazione selezionato non esiste",Icon.Error,ButtonEnum.Ok);
                break;
        }
    }

    public void ValidaJSON(string path)
    {
        string err = this.Valida(path);

        if(string.IsNullOrEmpty(err))
            this.MostraMsg("Info", $"Il file al percorso {path} è valido rispetto allo schema",Icon.Info,ButtonEnum.Ok);
        else 
            this.MostraMsg("Errore", err,Icon.Error,ButtonEnum.Ok);
        
    }

    public void MostraMsg(string titolo, string msg,Icon i = Icon.None, ButtonEnum b = ButtonEnum.Ok)
    {
        MessageBoxManager.GetMessageBoxStandard(titolo, msg, b, i).ShowAsync();
    }

    /// <summary>
    /// effettua la validazione senza mostrare messaggi all'utente
    /// </summary>
    /// <param name="path">percorso al file da validare</param>
    /// <returns>
    /// stringa formattata con gli errori riscontrati o stringa vuota in assenza 
    /// di errori
    /// /returns>
    private string Valida(string path)
    {
        IList<ValidationError> err = _pers.Valida(path);
        string ret = "";

        if(err.Count > 0)
        {
            ret = $"Il file al percorso {path} non rispetta lo schema predefinito.\n Errori: \n";
            foreach(ValidationError e in err)
                ret += " - Linea numero: " + e.LineNumber + " - Percorso: " + e.Path + " - Valore: " + e.Value + "\n" + "Errore: " + e.Message + "\n----\n";
        }
        return ret;
    }


    private List<string> GeneraQueryRel()
    {return new List<string>();}

    private List<string> GeneraQueryNodi()
    {return new List<string>();}

    private List<string> GeneraQueryCat()
    {return new List<string>();}
}

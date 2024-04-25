﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
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
        try
        {
            if (ValidaJSON(path))
            {
                this._pers.Crea(path);
                this.MostraMsg("Info", "Importazione del file JSON portata a termine correttamente",Icon.Info,ButtonEnum.Ok);
            }
            
        }
        catch (PersExc e)
        {
            this.MostraMsg("Errore", e.Message,Icon.Error,ButtonEnum.Ok);
            if (e is PersExcDupl || e is PersExcNotFound)
                this.CancellaJSON();
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

    public bool ValidaJSON(string path)
    {
        List<string> err = _pers.Valida(path);
        bool ret = err.Count == 0;

        if(ret)
            this.MostraMsg("Info", $"Il file al percorso {path} è valido rispetto allo schema",Icon.Info,ButtonEnum.Ok);
        else 
        {
            string msg = $"Il file al percorso {path} non rispetta lo schema predefinito.\n Errori: ";
            foreach(string s in err)
                msg += s;
            this.MostraMsg("Errore", msg,Icon.Error,ButtonEnum.Ok);
        }

        return ret;
    }

    public void MostraMsg(string titolo, string msg,Icon i = Icon.None, ButtonEnum b = ButtonEnum.Ok)
    {
        MessageBoxManager.GetMessageBoxStandard(titolo, msg, b, i).ShowAsync();
    }


    private List<string> GeneraQueryRel()
    {return new List<string>();}

    private List<string> GeneraQueryNodi()
    {return new List<string>();}

    private List<string> GeneraQueryCat()
    {return new List<string>();}
}

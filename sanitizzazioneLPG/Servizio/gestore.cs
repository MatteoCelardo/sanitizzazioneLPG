using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using sanitizzazioneLPG.Persistenza;

namespace sanitizzazioneLPG.Servizio;

public class Gestore : IServizio
{
    private readonly IPers _pers;

    #region singleton
    private static Gestore? _istanza = null;
    private static object mutex = new object();

    private Gestore()
    {
        _pers = Pers.Istanza;
    }

    public static Gestore Istanza 
    {
        get 
        {
            lock(mutex)
            {
                return _istanza ?? new Gestore();
            }
        }
    }
    #endregion

    public void CancellaJSON()
    {
        _pers.Cancella();
        MessageBoxManager.GetMessageBoxStandard("Info", "Cancellazione del precedente file JSON importato completata correttamente",ButtonEnum.Ok);
    }

    public void ImportaJSON(string path)
    {
        try
        {
            if (ValidaJSON(path))
            {
                _pers.Crea(path);
                MessageBoxManager.GetMessageBoxStandard("Info", "Importazione del file JSON portata a termine correttamente",ButtonEnum.Ok);
            }
            
        }
        catch (PersExc e) 
        {
            MessageBoxManager.GetMessageBoxStandard("Errore", e.Message,ButtonEnum.Ok);
        }
    }

    public void MostraMsg(string titolo, string msg)
    {
        MessageBoxManager.GetMessageBoxStandard(titolo, msg,ButtonEnum.Ok);
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
                MessageBoxManager.GetMessageBoxStandard("Errore", "Il tipo di sanitizzazione selezionato non",ButtonEnum.Ok);
                break;
        }
    }

    public bool ValidaJSON(string path)
    {
        List<string> err = _pers.Valida(path);
        bool ret = err.Count == 0;

        if(ret)
            MessageBoxManager.GetMessageBoxStandard("Info", $"Il file al percorso {path} è valido rispetto allo schema",ButtonEnum.Ok);
        else 
        {
            string msg = $"Il file al percorso {path} non rispetta lo schema predefinito.\n Errori: ";
            foreach(string s in err)
                msg += s;
            MessageBoxManager.GetMessageBoxStandard("Errore", msg,ButtonEnum.Ok);
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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Neo4jClient;
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
        this.MostraMsg("Info", "Cancellazione dei dati precedenemtente importati completata correttamente",Icon.Info,ButtonEnum.Ok);
    }

    public bool ImportaJSON(string path)
    {
        string err;
        bool ret = false;
        try
        {
            err = this.ValidaJSON(path);
            if (string.IsNullOrEmpty(err))
            {
                this._pers.Crea(path);
                this.MostraMsg("Info", "Importazione del file JSON portata a termine correttamente",Icon.Info,ButtonEnum.Ok);
                ret = true;
            }
            else 
                this.MostraMsg("Errore", err,Icon.Error,ButtonEnum.Ok);
            
        }
        catch (PersExc e)
        {
            
            if (e is PersExcDupl || e is PersExcNotFound)
                this.CancellaJSON();
            this.MostraMsg("Errore", e.Message,Icon.Error,ButtonEnum.Ok);
        }

        return ret;
    }

    public async void SanitizzaDB(EnumSanit s)
    {
        /*
        var client = new BoltGraphClient("bolt://localhost:7687", "neo4j", "tesiCelardo2024");
        await client.ConnectAsync();
        
        await client.Cypher
                    .Create("(:User {name:'temp'}), (:User {name:'wer', casa:'dasdioa'})")
                    .ExecuteWithoutResultsAsync();

        await client.Cypher
                    .Match("(u:User)")
                    .Where("u.name = 'wer'")
                    .Delete("u")
                    .ExecuteWithoutResultsAsync();*/
        Task<List<string>> queryNodi;
        Task<List<string>> queryRel;
        Task<List<string>> queryCat;

        switch(s)
        {
            case EnumSanit.CANC: 
                queryRel = Task.Run(GeneraQueryNodi);
                queryNodi = Task.Run(GeneraQueryRel);
                queryCat = Task.Run(GeneraQueryCat);
                Task.WaitAll(queryRel, queryNodi, queryCat);
                Console.WriteLine(queryRel.Result[0] + queryCat.Result[0] + queryNodi.Result[0]);
                break;
            default: 
                this.MostraMsg("Errore", "Il tipo di sanitizzazione selezionato non esiste",Icon.Error,ButtonEnum.Ok);
                break;
        }
    }
    
    public string ValidaJSON(string path)
    {
        IList<ValidationError> err = _pers.Valida(path);
        string ret = "";

        if(err.Count > 0)
        {
            ret = $"Il file al percorso {path} non rispetta lo schema predefinito.\n Errori: \n";
            foreach(ValidationError e in err)
                ret += "\t- Linea numero: " + e.LineNumber + " - Percorso: " + e.Path + " - Valore: " + e.Value + "\n" + "\t\tErrore: " + e.Message + "\n----\n";
        }
        return ret;
    }

    public void MostraMsg(string titolo, string msg,Icon i = Icon.None, ButtonEnum b = ButtonEnum.Ok)
    {
        MessageBoxManager.GetMessageBoxStandard(titolo, msg, b, i).ShowAsync();
    }


    private List<string> GeneraQueryRel()
    {
        //await Task.Delay(TimeSpan.FromSeconds(7));
        return ["prova"];
    }

    private async Task<List<string>> GeneraQueryNodi()
    {
        await Task.Delay(TimeSpan.FromSeconds(4));
        return ["ciao"];
    }

    private List<string> GeneraQueryCat()
    {
        //await Task.Delay(TimeSpan.FromSeconds(3));
        return ["casa"];
    }
}

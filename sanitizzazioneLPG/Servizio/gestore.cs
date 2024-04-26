using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Neo4jClient;
using Neo4jClient.Cypher;
using Newtonsoft.Json.Schema;
using sanitizzazioneLPG.Dominio;
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
        Task queryNodi;
        Task queryRel;
        Task queryCat;

        switch(s)
        {
            case EnumSanit.CANC: 
                BoltGraphClient? client = new BoltGraphClient("bolt://localhost:7687", "neo4j", "tesiCelardo2024");
                await client.ConnectAsync();

                // generazione ed esecuzione di tutte le query in thread paralleli.
                // Si ricorda che, stando alla documentazione e da piccoli test che 
                // ho condotto, ConnectAsync() restituisce una connessione thread safe

                queryNodi = Task.Run(() => this.SanitizzaNodi(client, _pers.ListAll(EnumTipoDom.NODI)));
                queryRel = Task.Run(() => this.SanitizzaRel(client, _pers.ListAll(EnumTipoDom.RELAZIONI)));
                queryCat = Task.Run(() => this.SanitizzaCat(client, _pers.ListAll(EnumTipoDom.CATENE)));
                Task.WaitAll(queryRel, queryNodi, queryCat);

                this.MostraMsg("Info", "Sanitizzazione portata a termine correttamente", Icon.Info, ButtonEnum.Ok);
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
    
    /// <summary>
    /// permette di mostrare una finestra di popup contenente un messaggio
    /// </summary>
    /// <param name="titolo">titolo della finestra</param>
    /// <param name="msg">messaggio da mostrare</param>
    /// <param name="i">icona da mostrare</param>
    /// <param name="b">bottoni da mostrare</param>
    public void MostraMsg(string titolo, string msg,Icon i = Icon.None, ButtonEnum b = ButtonEnum.Ok)
    {
        MessageBoxManager.GetMessageBoxStandard(titolo, msg, b, i).ShowAsync();
    }


    private Task SanitizzaRel(BoltGraphClient bgc, List<IDom> lr)
    {
        ICypherFluentQuery query;
        
        foreach(Relazione r in lr)
        {
            query = bgc.Cypher
                .Match("(r)")
                .WhereIf(!string.IsNullOrEmpty(r.IdRel.Etichetta),"$etichetta in labels(r)")
                .WithParam("etichetta",r.IdRel?.Etichetta);

            // aggiunta dei parametri in propNum
            foreach(string k in r.IdRel.PropNum.Keys)
            {
                query = query.AndWhere("$key = $value").WithParams(new{key = k, value = r.IdRel.PropNum[k]});
            }

            // aggiunta dei parametri in propStr
            foreach(string k in r.IdRel.PropStr.Keys)
            {
                query = query.AndWhere("$key = $value").WithParams(new{key = k, value = r.IdRel.PropStr[k]});
            }


            if(r.RelSens)
                query = query.Delete("r");  // relazione interamente sensibile
            else 
            {
                // relazione sensibile solo in parte
                query.se
            }

            query.ExecuteWithoutResultsAsync();
        }

        /*
        var stringsToRemove = new List<string> { "string1", "string2", "string3" };

        var query = client.Cypher
            .Match("(n:Node)")
            .ForEach("(string IN $stringsToRemove | SET n.property = REPLACE(n.property, string, ''))")
            .WithParam("stringsToRemove", stringsToRemove);
        
        query.ExecuteWithoutResults();
        */

        /*bgc.Cypher
            .Create("(:User {name:'temp'}), (:User {name:'wer', casa:'dasdioa'})")
            .ExecuteWithoutResultsAsync();

        var query = bgc.Cypher
                    .Match("(u)");
        query = query.Where("$etichetta in labels(u)").WithParam("etichetta","User");
        query = query.AndWhere("u.name = 'wer'");

        query.Delete("u").ExecuteWithoutResultsAsync();   
        return Task.Delay(2);     
        
        
        await client.Cypher
                    .Create("(:User {name:'temp'}), (:User {name:'wer', casa:'dasdioa'})")
                    .ExecuteWithoutResultsAsync();

        await client.Cypher
                    .Match("(u:User)")
                    .Where("u.name = 'wer'")
                    .Delete("u")
                    .ExecuteWithoutResultsAsync();*/
    }

    private Task SanitizzaNodi(BoltGraphClient bgc, List<IDom> ln)
    {
        return Task.Delay(1);
        /*
        bgc.Cypher
            .Create("(:User {name:'nod1'})")
            .ExecuteWithoutResultsAsync();
        bgc.Cypher
            .Create("(:User {name:'nod2'})")
            .ExecuteWithoutResultsAsync();
        bgc.Cypher
            .Create("(:User {name:'nod3'})")
            .ExecuteWithoutResultsAsync();
        return bgc.Cypher
            .Create("(:User {name:'nod4'})")
            .ExecuteWithoutResultsAsync();
            */
    }

    private Task SanitizzaCat(BoltGraphClient bgc, List<IDom> lc)
    {
        return Task.Delay(1);
        /*
        bgc.Cypher
            .Create("(:User {name:'cat1'})")
            .ExecuteWithoutResultsAsync();
        bgc.Cypher
            .Create("(:User {name:'cat2'})")
            .ExecuteWithoutResultsAsync();
        bgc.Cypher
            .Create("(:User {name:'cat3'})")
            .ExecuteWithoutResultsAsync();
        return bgc.Cypher
            .Create("(:User {name:'cat4'})")
            .ExecuteWithoutResultsAsync();
            */
    }
}

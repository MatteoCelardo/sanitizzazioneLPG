﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DynamicData;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Neo4jClient;
using Neo4jClient.Cypher;
using Newtonsoft.Json.Schema;
using sanitizzazioneLPG.Dominio;
using sanitizzazioneLPG.Persistenza;
using sanitizzazioneLPG.Sanitizzazione;

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


    private void SanitizzaRel(BoltGraphClient bgc, List<IDom> lr)
    {
        
        ICypherFluentQuery query;
        IList<string> chiavi;
        // flag usato per sapere se il primo elemento di propNum e propStr sia stato 
        // inserito a parte o meno. flag = 0 se non inserito a parte, 1 altrimenti
        int flag = 0; 
        
        foreach(Relazione r in lr)
        {
            query = bgc.Cypher
                .Match("(r)")
                .WhereIf(!string.IsNullOrEmpty(r.IdRel.Etichetta),"$etichetta in labels(r)")
                .WithParam("etichetta",r.IdRel?.Etichetta);

            // aggiunta dei parametri in propNum

            chiavi = r.IdRel.PropNum.Keys.ToList();

            // se non è stata inserita l'etichetta, bisogna ancora inserire il where 
            // nella query
            if(!query.Query.QueryText.Contains("WHERE"))
            {
                query = query.Where("$keyPN0 = $valuePN0").WithParams(new {keyPN0 = chiavi[0], valuePN0 = r.IdRel?.PropNum[chiavi[0]]});
                flag++;
            }

            // tramite il flag, so se includere o meno il primo elemento nel ciclo
            for(int i = 0 + flag; i < chiavi.Count; i++)
               query = query.AndWhere($"$keyPN{i} = $valuePN{i}").WithParam($"keyPN{i}", chiavi[i]).WithParam($"valuePN{i}", r.IdRel?.PropNum[chiavi[i]]);

            // aggiunta dei parametri in propStr. analogo a PropNum

            chiavi = r.IdRel.PropStr.Keys.ToList();
            flag = 0;
            if(!query.Query.QueryText.Contains("WHERE"))
            {
                query = query.Where("$keyPS0 = $valuePS0").WithParams(new {keyPS0 = chiavi[0], valuePS0 = r.IdRel?.PropStr[chiavi[0]]});
                flag++;
            }
            for(int i = 0 + flag; i <  chiavi.Count; i ++)
                query = query.AndWhere($"$keyPS{i} = $valuePS{i}").WithParam($"keyPS{i}", chiavi[i]).WithParam($"valuePS{i}", r.IdRel?.PropStr[chiavi[i]]);



            if(r.RelSens)
                query = query.Delete("r");  // relazione interamente sensibile
            else 
            {
                // relazione sensibile solo in parte

                /*
                    etichette (per i nodi) e prop sempre sens vanno messe sempre a null. 
                    teoricamente posso metterle direttamente senza errori

                    quelle in prop sens: provare con un case
                */
                
            }

            query.ExecuteWithoutResultsAsync();
        }
        

        
        /*
        //AppDomainUnloadedException: i have a list of strings, each of those needs to be put in a remove clause. the problem is that the list length is variable. how can i create a neo4jclient query to accomplish this task? 
        
        var stringsToRemove = new List<string> { "string1", "string2", "string3" };

        List<string> propertiesToRemove = new List<string> { "name", "casa", "property3" };

        string str = ""; 

        for(int i = 0; i < propertiesToRemove.Count; i++)
            str += $"n.$p{i} ";
        query = bgc.Cypher
            .Match("(n)")
            .Remove(str);

        for(int i = 0; i < propertiesToRemove.Count; i++)
            query = query.WithParam($"p{i}",propertiesToRemove[i]);

        Console.WriteLine(query.Query.QueryText);
        foreach(object o in query.Query.QueryParameters)
            Console.WriteLine(o.ToString());

        query.ExecuteWithoutResultsAsync();
        */
        
        /*
        bgc.Cypher
            .Create("(:User {name:'temp'}), (:User {name:'wer', casa:'dasdioa'})")
            .ExecuteWithoutResultsAsync();

        query = bgc.Cypher
                    .Match("(u)");
        query = query.Where("$etichetta in labels(u) AND u.name = 'wer'").WithParam("etichetta","User");
        query = query.Delete("u");

        Console.WriteLine(query.Query.QueryText);
        Console.WriteLine(query.Query.DebugQueryText);
        
        query.ExecuteWithoutResultsAsync();   */
    }

    private void SanitizzaNodi(BoltGraphClient bgc, List<IDom> ln)
    {
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
        bgc.Cypher
            .Create("(:User {name:'nod4'})")
            .ExecuteWithoutResultsAsync();
            */
    }

    private void SanitizzaCat(BoltGraphClient bgc, List<IDom> lc)
    {
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
        bgc.Cypher
            .Create("(:User {name:'cat4'})")
            .ExecuteWithoutResultsAsync();
            */
    }

    /// <summary>
    /// richiama il metodo SanitizzaNodo dell'interfaccia ISanit
    /// </summary>
    /// <typeparam name="T">classe concreta che implementa l'interfaccia</typeparam>
    /// <param name="dsn">oggetto da sanitizzare</param>
    /// <returns>oggetto sanitizzato</returns>
    private DaSanitizzareNodo_ SanitNodo<T>(DaSanitizzareNodo_ dsn) where T : ISanit
    {
        return T.SanitizzaNodo(dsn);
    }

    /// <summary>
    /// richiama il metodo SanitizzaRel dell'interfaccia ISanit
    /// </summary>
    /// <typeparam name="T">classe concreta che implementa l'interfaccia</typeparam>
    /// <param name="dsn">oggetto da sanitizzare</param>
    /// <returns>oggetto sanitizzato</returns>
    private DaSanitizzareRel_ SanitRel<T>(DaSanitizzareRel_ dsr) where T : ISanit
    {
        return T.SanitizzaRel(dsr);
    }
}

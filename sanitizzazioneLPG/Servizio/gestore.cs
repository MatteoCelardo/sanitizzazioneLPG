using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Joins;
using System.Threading.Tasks;
using Avalonia.Data.Converters;
using Microsoft.CodeAnalysis;
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
    private string _usr;
    private string _pwd;
    private string _uri;

    #region singleton
    private static Gestore? _istanza = null;
    private static object _mutex = new object();

    private Gestore()
    {
        this._pers = Pers.Istanza;
        _usr = "";
        _pwd = "";
        _uri = "";
    }

    public static Gestore Istanza 
    {
        get 
        {
            lock(_mutex)
            {
                if(_istanza == null)
                    _istanza = new Gestore();
                return _istanza;
            }
        }
    }
    #endregion

    
    public async void ConnettiDB(string usr, string pwd, string uri)
    {
        // se l'uri specifica una porta, si lascia quella; altrimenti viene impostata 
        // la porta 7687 di default
        string uriPorta = uri + (uri.Contains(':') ? "" : ":7687");
        try 
        {
            BoltGraphClient? client = new BoltGraphClient("bolt://"+uriPorta, usr, pwd);
            await client.ConnectAsync();

            this.MostraMsg("Info", "Credenziali e URI inseiri corretti. È ora possibile procedere con la sanitizzazione.",Icon.Info,ButtonEnum.Ok);

            _usr = usr;
            _pwd = pwd;
            _uri = uriPorta;
        }
        catch (Exception e)
        {
            this.MostraMsg("Errore","Non è stato possibile connettersi al DB con URI \"" + uri 
            + "\". Il DB non è raggiungibile o le credenziali inserite non sono corrette",Icon.Error,ButtonEnum.Ok);
        }
    }

    public void CancellaJSON()
    {
        this._pers.Cancella();
        this.MostraMsg("Info", "Cancellazione dei dati precedenemtente importati completata correttamente",Icon.Info,ButtonEnum.Ok);
    }

    public bool ImportaJSON(string path)
    {
        string err;
        bool ret = false;
        if (!string.IsNullOrEmpty(_uri) && !string.IsNullOrEmpty(_usr) && !string.IsNullOrEmpty(_uri))
        {
            try
            {
                err = this.ValidaJSON(path);
                if (string.IsNullOrEmpty(err))
                {
                    this._pers.Importa();
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
        }
        else 
        {
            this.MostraMsg("Errore", "Non è possibile importare file JSON prima di aver effettuato la connessione ad un DB",Icon.Error,ButtonEnum.Ok);
        }

        return ret;
    }

    public async void SanitizzaDB(EnumSanit s)
    {
        Task<string> queryNodi;
        Task<string> queryRel;
        Task<string> queryCat;

        switch(s)
        {
            case EnumSanit.CANC:
                try 
                {
                    BoltGraphClient? client = new BoltGraphClient("bolt://"+_uri, _usr, _pwd);
                    await client.ConnectAsync();

                    // generazione ed esecuzione di tutte le query in thread paralleli.
                    // Si ricorda che, stando alla documentazione e da piccoli test che 
                    // ho condotto, ConnectAsync() restituisce una connessione thread safe

                    queryNodi = Task.Run(() => this.SanitizzaNodi(client, _pers.ListAll(EnumTipoDom.NODI)));
                    queryRel = Task.Run(() => this.SanitizzaRel(client, _pers.ListAll(EnumTipoDom.RELAZIONI)));
                    queryCat = Task.Run(() => this.SanitizzaCat(client, _pers.ListAll(EnumTipoDom.CATENE)));
                    Task.WaitAll(queryRel, queryNodi, queryCat);

                    if(string.IsNullOrEmpty(queryNodi.Result) && string.IsNullOrEmpty(queryRel.Result) && string.IsNullOrEmpty(queryCat.Result))
                        this.MostraMsg("Info", "Sanitizzazione portata a termine correttamente", Icon.Info, ButtonEnum.Ok);
                    else 
                        this.MostraMsg("Errore", "La sanitizzazione non è andata a buon fine. Errori: \n" 
                        + "\tSanitizzazione dei nodi: " + queryNodi.Result 
                        + "\n\tSanitizzazione delle relazioni: " + queryRel.Result 
                        + "\n\tSanitizzazione delle catene: " + queryCat.Result, Icon.Error,ButtonEnum.Ok );
                }
                catch(Exception e )
                {
                    this.MostraMsg("Errore","Errore occorso in fase di connessione col db: " + e.Message, Icon.Error, ButtonEnum.Ok);
                }
                break;
            default: 
                this.MostraMsg("Errore", "Il tipo di sanitizzazione selezionato non esiste",Icon.Error,ButtonEnum.Ok);
                break;
        }
    }
    
    public string ValidaJSON(string path)
    {
        string ret = "";
        try{
            IList<ValidationError> err = _pers.Valida(path);

            if(err.Count > 0)
            {
                ret = $"Il file al percorso {path} non rispetta lo schema predefinito.\n Errori: \n";
                foreach(ValidationError e in err)
                    ret += "\t- Linea numero: " + e.LineNumber + " - Percorso: " +  e.Path +  " - Valore: " +  e.Value + "\n\t\tErrore: " +  e.Message +  "\n----\n";
            }
        }
        catch(PersExc e)
        {
            ret = e.Message;
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


    #region task sanitizzazione LPG
    private async Task<string> SanitizzaRel(BoltGraphClient bgc, List<IDom> lr)
    {
        
        ICypherFluentQuery query;
        
        IdRel_ idr;
        
        foreach(Relazione r in lr)
        {
            idr = r.IdRel;
            query = bgc.Cypher
                .With("'ok' AS risultato")
                .Match("()-[r]->()")
                .WhereIf(!string.IsNullOrEmpty(idr.Etichetta),"type(r) = $etichetta")
                .WithParam("etichetta",idr.Etichetta);
            
            query = this.CreaWhereProp(query, idr.PropStr, idr.PropNum,"r");

            if(r.RelSens)
                query = query.Delete("r");  // relazione interamente sensibile
            else 
                // relazione sensibile solo in parte
                query = this.CreaRemovePropSens(query, r.DaSanitizzare.PropSempreSens, r.DaSanitizzare.PropSensAssoc,"r");

            try 
            {
                var res = await query.Return(risultato => risultato.As<string>()).ResultsAsync;
            }
            catch(Exception e)
            {
                return e.Message;
            }
        }
        return "";
    }

    private async Task<string> SanitizzaNodi(BoltGraphClient bgc, List<IDom> ln)
    {
        ICypherFluentQuery query;
        IdNodo_ idn;
        
        foreach(Nodo n in ln)
        {
            idn = n.IdNodo;
            query = bgc.Cypher
                .With("'ok' AS risultato")
                .Match("(n)")
                .WhereIf(idn.Etichette.Length > 0,"ALL(etic IN $etichette WHERE etic IN labels(n))")
                .WithParam("etichette",idn.Etichette);

            query = this.CreaWhereProp(query, idn.PropStr, idn.PropNum, "n");


            if(n.NodoSens)
                query = query.DetachDelete("n");  // nodo interamente sensibile
            else 
            {
                // nodo sensibile solo in parte

                // rimozione di tutte le etichette sempre sensibili 
                if(n.DaSanitizzare.EtichetteSens.Length > 0)
                    query = query
                            .With("n, $etSempreSens AS ess, 'ok' AS risultato")
                            .WithParam("etSempreSens", n.DaSanitizzare.EtichetteSens)
                            .Call("apoc.do.when(ANY (ets IN ess WHERE ets in labels(n)), \'CALL apoc.create.removeLabels(n, ess) YIELD node RETURN node\', \'RETURN \"nulla\" AS risultato\',{n: n, ess: ess})")
                            .Yield("value");

                query = this.CreaRemovePropSens(query, n.DaSanitizzare.PropSempreSens,n.DaSanitizzare.PropSensAssoc,"n");
            }

            
            try 
            {
                await query.Return(risultato => risultato.As<string>()).ResultsAsync;
            }
            catch(Exception e)
            {
                return e.Message;
            }
        }

        return "";
    }

    private async Task<string> SanitizzaCat(BoltGraphClient bgc, List<IDom> lc)
    {
        ICypherFluentQuery query;
        // stringa contenente la clausola match
        string match;
        // stringa contenente la clausola where
        string where;
        // stringa contenente la clausola with
        string with; 
        // stringa con la clausola delete
        string delete;

        // dizionario usato per contenere i nomi dei parametri usati per inserire 
        // il vettore di etichette di ciascun nodo.
        // le etichette riferite ad altri campi stringa sono salvate in parEticStr
        Dictionary<string,string[]> parNodi = new Dictionary<string, string[]>();

        // dizionario con i parametri che verranno sostituiti da stringhe
        Dictionary<string,string> parStr = new Dictionary<string,string>();

        // dizionario con i parametri che verranno sostituiti da numeri
        Dictionary<string,double> parNum = new Dictionary<string, double>();

        // dizionario per conenere le chiamate apoc e l'eventuale condizione con cui precederla
        Dictionary<string,string> callApoc = new Dictionary<string, string>();
        // dizionario contenere ciascun parametro usato dalle chiamate apoc e il relativo 
        // vettore
        Dictionary<string, string[]> parApoc = new Dictionary<string, string[]>();

        Relazione r;
        Nodo n;
        Catena c;
        

        
        for(int j = 0; j < lc.Count; j++)
        {
            c = (Catena) lc[j];
            where = "";
            delete = "";
            match = "(";
            with = "";

            for(int i = 0; i < c.Els.Count; i++)
            {
                if(c.Els[i].GetType() == typeof(Relazione))
                {
                    r = (Relazione)c.Els[i];
                    match += ")-[r" + j + i + "]->(";
                    

                    if(!string.IsNullOrEmpty(r.IdRel.Etichetta))
                    {
                        if(string.IsNullOrEmpty(where))
                            where = "type(r"+ j + i + ") = $etichetta" + j + i;
                        else 
                            where += " AND type(r"+ j + i + ") = $etichetta" + j + i;
                        parStr.Add("etichetta" + j + i,r.IdRel.Etichetta);
                    }

                    this.CreaWherePropCatena(ref where, r.IdRel.PropStr, r.IdRel.PropNum, ref parStr, ref parNum,"r"+j+i);

                    if(r.RelSens)
                        delete += "r" + j + i + ", ";
                    else if(r.DaSanitizzare != null)
                    {
                        with += "r" + j + i + ", ";
                        this.CreaRemovePropSensCatena(ref with, ref callApoc, ref parApoc, ref parStr, r.DaSanitizzare.PropSempreSens, r.DaSanitizzare.PropSensAssoc,"r"+j+i);
                    }
                }
                else 
                {
                    n = (Nodo)c.Els[i];
                    match += "n" + j + i;
                        if(n.IdNodo.Etichette.Length > 0)
                        {
                            if(string.IsNullOrEmpty(where))
                                where = "ALL(etic IN $etichette" + j+ i + " WHERE etic IN labels(n" + j + i +"))";
                            else 
                                where += " AND ALL(etic IN $etichette" + j +  i + " WHERE etic IN labels(n" + j + i +"))";
                            parNodi.Add("etichette" + j + i, n.IdNodo.Etichette);
                        }

                        this.CreaWherePropCatena(ref where, n.IdNodo.PropStr, n.IdNodo.PropNum, ref parStr,  ref parNum, "n"+j+i);

                        if(n.NodoSens)
                            delete += "n" + j + i + ", ";
                        else if(n.DaSanitizzare != null)
                        {
                            with += "n" + j + i + ", ";
                            if(n.DaSanitizzare.EtichetteSens.Length > 0)
                            {
                                with += "$etSempreSensN"+ j + i+" AS ess_"+j+i+", ";
                                // apoc per la rimozione di tutte le etichette sensibili
                                callApoc.Add("apoc.do.when(ANY (ets IN ess_"+j+i+" WHERE ets IN labels(n" + j + i +")), \'CALL apoc.create.removeLabels(n"+ j + i +", ess_" + j + i+") YIELD node RETURN node\', \'RETURN \"nulla\" AS risultato\',{ess_"+j+i+": ess_"+j+i+", n" + j + i +": n" + j + i +"})","");
                                parApoc.Add("etSempreSensN"+ j + i, n.DaSanitizzare.EtichetteSens);
                            }

                            this.CreaRemovePropSensCatena(ref with, ref callApoc, ref parApoc, ref parStr, n.DaSanitizzare.PropSempreSens, n.DaSanitizzare.PropSensAssoc,"n"+j+i);
                        }
                    }

                }

                match += ")";

                query = bgc.Cypher
                        .Match(match)
                        .Where(where);


                if(!string.IsNullOrEmpty(delete))    
                    // rimuovo la virgola e lo spazio infondo alla stringa delete
                    query = query.DetachDelete(delete.Remove(delete.Length - 2));

                // aggiunta di tutte le chiamate apoc
                foreach(string chiave in callApoc.Keys.ToList())
                {
                    query = query.With(with.Remove(with.Length - 2) + ", 'ok' AS risultato");
                    query = query
                            .WhereIf(!string.IsNullOrEmpty(callApoc[chiave]),callApoc[chiave])
                            .Call(chiave)
                            .Yield("value");
                }

                // inserimento dei parametri usati per le etichette dei nodi 
                foreach(string chiave in parNodi.Keys.ToList())
                    query = query.WithParam(chiave,parNodi[chiave]); 

                // inserimento dei parametri di tipo stringa
                foreach(string chiave in parStr.Keys.ToList())
                    query = query.WithParam(chiave,parStr[chiave]); 

                // inserimento dei parametri numerici
                foreach(string chiave in parNum.Keys.ToList())
                    query = query.WithParam(chiave,parNum[chiave]); 
                
                // inserimento parametri apoc
                foreach(string chiave in parApoc.Keys.ToList())
                    query = query.WithParam(chiave,parApoc[chiave]);

                try 
                {
                    await query.Return(risultato => risultato.As<string>()).ResultsAsync;
                }
                catch(Exception e)
                {
                    return e.Message;
                }
                parNodi.Clear();
                parStr.Clear();
                parNum.Clear();
                callApoc.Clear();
                parApoc.Clear();
            }

        return "";
    }

    #endregion

    #region funzioni di appoggio ai task di sanitizzazione
    /// <summary>
    /// crea la parte di clausola where riguardante <c>propStr</c> e <c>propNum</c> 
    /// appartenenti a <c>IdRel</c> o <c>IdNodo</c>
    /// </summary>
    /// <param name="query">parte di query già composta in precedenza</param>
    /// <param name="propStr">proprietà PropStr di <c>IdRel</c> o <c>IdNodo</c></param>
    /// <param name="propNum">proprietà PropNum di <c>IdRel</c> o <c>IdNodo</c></param>
    /// <param name="nomeElem">nome dato nella clausola match all'elemento di cui si vogliono rimuovere le informazioni sensibili</param>
    /// <returns>
    /// ritorna la parte di query con le condizioni da mettere nel where per rispettare
    /// quanto specificato specificato in <c>propStr</c> e in <c>propNum</c>
    /// </returns>
    private ICypherFluentQuery CreaWhereProp(ICypherFluentQuery query, IDictionary<string,string> propStr, IDictionary<string,double> propNum, string nomeElem)
    {
        IList<string> chiavi;
        // flag usato per sapere se il primo elemento di propNum e propStr sia stato 
        // inserito a parte o meno. flag = 0 se non inserito a parte, 1 altrimenti
        int flag = 0; 

        // aggiunta dei parametri in propNum

        if(propNum.Count != 0)
        {
            chiavi = propNum.Keys.ToList();

            // se non è stata inserita l'etichetta, bisogna ancora inserire il where 
            // nella query
            if(!query.Query.QueryText.Contains("WHERE"))
            {
                query = query.Where(nomeElem + "." + chiavi[0] + " = $valuePN" + nomeElem + "0").WithParam("valuePN" + nomeElem + "0", propNum[chiavi[0]]);
                flag++;
            }
            // tramite il flag, so se includere o meno il primo elemento nel ciclo
            for(int i = 0 + flag; i < chiavi.Count; i++)
               query = query.AndWhere(nomeElem + "." + chiavi[i] + " = $valuePN" + nomeElem + i).WithParam("valuePN" + nomeElem + i, propNum[chiavi[i]]);

        }
        // aggiunta dei parametri in propStr. analogo a PropNum

        if(propStr.Count != 0)
        {
            chiavi = propStr.Keys.ToList();
            flag = 0;
            if(!query.Query.QueryText.Contains("WHERE"))
            {
                query = query.Where(nomeElem + "." + chiavi[0] + " = $valuePS"+ nomeElem + "0").WithParam("valuePS" + nomeElem +"0", propStr[chiavi[0]]);
                flag++;
            }
            for(int i = 0 + flag; i <  chiavi.Count; i ++)
                query = query.AndWhere(nomeElem + "." + chiavi[i] + " = $valuePS" + nomeElem + i).WithParam("valuePS" + nomeElem  + i, propStr[chiavi[i]]);
        }
        return query;
    }

    /// <summary>
    /// crea la parte di query con la rimozione delle informazioni sensibili specificate
    /// in <c>PropSempreSens</c> e <c>PropSensAssoc</c> all'interno degll' oggetto 
    /// <c>DaSanitizzareRel</c> o <c>DaSanitizzareNodo</c>
    /// </summary>
    /// <param name="query">parte di query già composta in precedenza</param>
    /// <param name="propSempreSens">proprietà <c>PropSempreSens</c> di <c>DaSanitizzareRel</c> o <c>DaSanitizzareNodo</c></param>
    /// <param name="propSensAssoc">proprietà <c>PropSensAssoc</c> di <c>DaSanitizzareRel</c> o <c>DaSanitizzareNodo</c></param>
    /// <param name="nomeElem">nome dato nella clausola match all'elemento di cui si vogliono rimuovere le informazioni sensibili</param>
    /// <returns>
    /// ritorna la parte di query con le condizioni da mettere nel remove per rispettare
    /// quanto specificato specificato in <c>propSempreSens</c> e in <c>propSensAssoc</c>
    /// </returns>
    private ICypherFluentQuery CreaRemovePropSens(ICypherFluentQuery query, string[] propSempreSens, IDictionary<string,PropSensAssoc_> propSensAssoc, string nomeElem)
    {
        // stringa usata per chiamare la funzione apoc per i nodi o per le realazioni
        string apoc;
        // stringa che contiene il nome della variabile da mettere nello yield
        string apocYield;

        // lista con le chiavi del dizionario propSensAssoc
        IList<string> chiavi;

        // inserimento di un match per un nodo o per una relazione in base al nome 
        // dell'elemento passato
        if(nomeElem.Contains("n"))
        {
            apoc = "apoc.create.removeProperties";
            apocYield = "node";
        }
        else 
        {
            apoc = "apoc.create.removeRelProperties";
            apocYield = "rel";
        }
        // rimozione di tutte le proprietà sempre sensibili

        if(propSempreSens.Length > 0)
            query = query
                    .With(nomeElem + ", $pSempreSens AS pss, 'ok' AS risultato")
                    .WithParam("pSempreSens",propSempreSens)
                    .Call("apoc.do.when(ANY(prop in pss WHERE "+nomeElem+"[prop] IS NOT NULL), \'CALL "+apoc+"("+nomeElem+",pss) YIELD "+apocYield+" RETURN "+apocYield+"\', \'RETURN \"nulla\" AS risultato\',{"+nomeElem+":"+nomeElem+",pss:pss})")
                    .Yield("value");

        if(propSensAssoc.Keys.Count > 0)
        {
            chiavi = propSensAssoc.Keys.ToList();
            for(int i = 0; i < propSensAssoc.Keys.Count; i++)
            {
                query = query
                        .With(nomeElem + ", $propAssoc"+i+" AS pAssoc, $p"+i+" AS p, 'ok' AS risultato")
                        .WithParam("propAssoc"+i, propSensAssoc[chiavi[i]].PropAssoc)
                        .WithParam("p"+i,chiavi[i]);
                if(propSensAssoc[chiavi[i]].SanitizzareProp)
                {
                    // se sanitizzare prop è a true, sanitizzo la proprietà in p, 
                    // ovvero quella che è resa sensibile dal vettore di proprietà 
                    // associate
                    query = query
                            .Call("apoc.do.when(ANY(pA IN pAssoc WHERE "+nomeElem+"[pA] IS NOT NULL),\'CALL "+apoc + "(" + nomeElem + ", [p]) YIELD "+apocYield+" RETURN "+apocYield+"\', \'RETURN \"nulla\" AS risultato\', {"+nomeElem+":"+nomeElem+",pAssoc:pAssoc,p:p})")
                            .Yield("value");
                }
                else 
                {
                    // in caso contrario, sanitizzo tutte le proprietà associate se p è 
                    // presente
                    query = query
                            .Call("apoc.do.when("+nomeElem+"[p] IS NOT NULL,\'CALL "+apoc + "(" + nomeElem + ", pAssoc) YIELD "+apocYield+" RETURN "+apocYield+"\', \'RETURN \"nulla\" AS risultato\', {"+nomeElem+":"+nomeElem+",pAssoc:pAssoc,p:p})")
                            .Yield("value");
                }
            }
        }
 
        return query;
    }

    /// <summary>
    /// crea la parte di query con la rimozione delle informazioni sensibili specificate
    /// in <c>PropSempreSens</c> e <c>PropSensAssoc</c> all'interno degll' oggetto 
    /// <c>DaSanitizzareRel</c> o <c>DaSanitizzareNodo</c> per gli elementi del 
    /// dominio nelle catene.
    /// il parametro formale <c>where</c> conterrà la stringa aggiornata con le condizioni 
    /// specificare negli oggetti
    /// </summary>
    /// <param name="where">vettore con le clausole where scritte in precedenza</param>
    /// <param name="propStr">proprietà PropStr di <c>IdRel</c> o <c>IdNodo</c></param>
    /// <param name="propNum">proprietà PropNum di <c>IdRel</c> o <c>IdNodo</c></param>
    /// <param name="parStr">dizionario usato per tenere traccia dei parametri che verranno sostituiti con stringhe e dei rispettivi valori</param>
    /// <param name="parNum">analogo di <c>parStr</c> per le etichette cui corrisponde un valore numerico</param>
    /// <param name="nomeElem">nome dato nella clausola match all'elemento di cui si vogliono rimuovere le informazioni sensibili</param>
    private void CreaWherePropCatena(ref string where, IDictionary<string,string> propStr, IDictionary<string,double> propNum, ref Dictionary<string,string> parStr, ref Dictionary<string,double> parNum, string nomeElem)
    {
        IList<string> chiavi;
        int flag;
        if(propNum.Count != 0)
        {
            chiavi = propNum.Keys.ToList();
            // flag usato per sapere se il primo elemento di propNum e propStr sia stato 
            // inserito a parte o meno. flag = 0 se non inserito a parte, 1 altrimenti
            flag = 0;
            //inserimento degli elementi in propNum
            if(string.IsNullOrEmpty(where))
            {
                where = nomeElem + "." + chiavi[0] + " = $valuePN" + nomeElem + "0";
                parNum.Add("valuePN" + nomeElem + "0",propNum[chiavi[0]]);
                flag++;
            }
            // tramite il flag, so se includere o meno il primo elemento nel ciclo
            for(int k = 0 + flag; k < chiavi.Count; k++)
            {
                where += " AND " + nomeElem + "." + chiavi[k] + " = $valuePN" + nomeElem + k;
                parNum.Add("valuePN" + nomeElem + k, propNum[chiavi[k]]);
            }
        }

        if(propStr.Count != 0)
        {
            chiavi = propStr.Keys.ToList();
            flag = 0;

            //inserimento degli elementi in propstr
            if(string.IsNullOrEmpty(where))
            {
                where = nomeElem + "." + chiavi[0] + " = $valuePS" + nomeElem + "0";
                parStr.Add("valuePS" + nomeElem + "0", propStr[chiavi[0]]);
                flag++;
            }
            // tramite il flag, so se includere o meno il primo elemento nel ciclo
            for(int k = 0 + flag; k < chiavi.Count; k++)
            {
                where +=  " AND " + nomeElem + "." + chiavi[k] + " = $valuePS" + nomeElem + k;
                parStr.Add("valuePS" + nomeElem + k, propStr[chiavi[k]]);
            }
        }
    }


    /// <summary>
    /// crea la parte di query con la rimozione delle informazioni sensibili specificate
    /// in <c>PropSempreSens</c> e <c>PropSensAssoc</c> all'interno degll' oggetto 
    /// <c>DaSanitizzareRel</c> o <c>DaSanitizzareNodo</c> per gli elementi del 
    /// dominio nelle catene
    /// </summary>
    /// <param name="with">stringa con la parte di clausola with scritta in precedenza</param>
    /// <param name="propSempreSens">proprietà <c>PropSempreSens</c> di <c>DaSanitizzareRel</c> o <c>DaSanitizzareNodo</c></param>
    /// <param name="propSensAssoc">proprietà <c>PropSensAssoc</c> di <c>DaSanitizzareRel</c> o <c>DaSanitizzareNodo</c></param>
    /// <param name="nomeElem">nome dato nella clausola match all'elemento di cui si vogliono rimuovere le informazioni sensibili</param>
    /// <param name="callApoc">dizionario avente per chiave le chiamate apoc e per valore le condizioni where relative</param>
    /// <param name="parApoc">dizionario usato per contenere i parametri usati nelle chiamate apoc ed i rispettivi valori</param>
    private void CreaRemovePropSensCatena(ref string with, ref Dictionary<string,string> callApoc, ref Dictionary<string,string[]> parApoc, ref  Dictionary<string,string> parStr, string[] propSempreSens, IDictionary<string,PropSensAssoc_> propSensAssoc, string nomeElem)
    {
        // sintra usata per le relazioni: siccome i detach delete possono cancellare 
        // delle relazioni specificate nel file, bisogna controllare che effettivamente 
        // esistano prima di provedere con la chiamata della apoc
        string el = "";
        // stringa usata per chiamare la funzione apoc per i nodi o per le realazioni
        string apoc;
        // stringa che contiene il nome della variabile da mettere nello yield
        string apocYield;

        // lista con le chiavi del dizionario propSensAssoc
        IList<string> chiavi;

        // inserimento di un match per un nodo o per una relazione in base al nome 
        // dell'elemento passato
        if(nomeElem.Contains("n"))
        {
            apoc = "apoc.create.removeProperties";
            apocYield = "node";
        }
        else 
        {
            el = "EXISTS(()-[" + nomeElem +"]->())";
            apoc = "apoc.create.removeRelProperties";
            apocYield = "rel";
        }

        // rimozione di tutte le proprietà sempre sensibili
        if(propSempreSens.Length > 0)
        {
            with += "$pSempreSens"+nomeElem+" AS pss_"+nomeElem+", ";
            callApoc.Add("apoc.do.when(ANY(prop in pss_"+nomeElem+" WHERE "+nomeElem+"[prop] IS NOT NULL),\'CALL "+apoc + "(" + nomeElem + ", pss_"+nomeElem+") YIELD "+ apocYield+ " RETURN "+apocYield+"\',\'RETURN \"nulla\" AS risultato\',{"+nomeElem+":"+nomeElem+",pss_"+nomeElem+":pss_"+nomeElem+"})",el);
            parApoc.Add("pSempreSens"+nomeElem,propSempreSens);
        }

        if(propSensAssoc.Keys.Count > 0)
        {
            chiavi = propSensAssoc.Keys.ToList();
            for(int i = 0; i < propSensAssoc.Keys.Count; i++)//foreach(string p in propSensAssoc.Keys)
            {
                with += "$propAssoc"+nomeElem+i+" AS pAssoc_"+nomeElem+i+", $p"+nomeElem+i +" AS p_"+nomeElem+i + ", ";
                parApoc.Add("propAssoc"+nomeElem+i, propSensAssoc[chiavi[i]].PropAssoc);
                parStr.Add("p"+nomeElem+i,chiavi[i]);
                if(propSensAssoc[chiavi[i]].SanitizzareProp)
                    // se sanitizzare prop è a true, sanitizzo la proprietà in p, 
                    // ovvero quella che è resa sensibile dal vettore di proprietà 
                    // associate
                    callApoc.Add("apoc.do.when(ANY(pA IN pAssoc_"+nomeElem+i+" WHERE "+nomeElem+"[pA] IS NOT NULL), \'CALL "+ apoc + "(" + nomeElem + ", [p_"+nomeElem+i+"]) YIELD "+apocYield+" RETURN "+apocYield+"\', \'RETURN \"nulla\" AS risultato\',{"+nomeElem+":"+nomeElem+", pAssoc_"+nomeElem+i+":pAssoc_"+nomeElem+i+",p_"+nomeElem+i+":p_"+nomeElem+i+"} )",el);
                else 
                    // in caso contrario, sanitizzo tutte le proprietà associate se p è 
                    // presente
                    callApoc.Add("apoc.do.when("+nomeElem+"[p_"+nomeElem+i+"] IS NOT NULL, \'CALL "+ apoc + "(" + nomeElem + ", pAssoc_"+nomeElem+i+") YIELD "+apocYield+" RETURN "+apocYield+"\', \'RETURN \"nulla\" AS risultato\',{"+nomeElem+":"+nomeElem+", pAssoc_"+nomeElem+i+":pAssoc_"+nomeElem+i+",p_"+nomeElem+i+":p_"+nomeElem+i+"} )",el);
                
            }
        }

    }

    #endregion

}

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
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
                ret += "\t- Linea numero: " + e.LineNumber + " - Percorso: " +  e.Path +  " - Valore: " +  e.Value + "\n\t\tErrore: " +  e.Message +  "\n----\n";
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
    private void SanitizzaRel(BoltGraphClient bgc, List<IDom> lr)
    {
        
        ICypherFluentQuery query;
        
        IdRel_ idr;
        
        foreach(Relazione r in lr)
        {
            idr = r.IdRel;
            query = bgc.Cypher
                .Match("()-[r]->()")
                .WhereIf(!string.IsNullOrEmpty(idr.Etichetta),"$etichetta in labels(r)")
                .WithParam("etichetta",idr.Etichetta);
            
            query = this.CreaWhereProp(query, idr.PropStr, idr.PropNum,"r");

            if(r.RelSens)
                query = query.Delete("r");  // relazione interamente sensibile
            else 
                // relazione sensibile solo in parte
                query = this.CreaRemovePropSens(query, r.DaSanitizzare.PropSempreSens, r.DaSanitizzare.PropSensAssoc,"r");

            query.ExecuteWithoutResultsAsync();
        }
        
    }

    private void SanitizzaNodi(BoltGraphClient bgc, List<IDom> ln)
    {
        ICypherFluentQuery query;
        IdNodo_ idn;
        
        foreach(Nodo n in ln)
        {
            idn = n.IdNodo;
            query = bgc.Cypher
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
                foreach(string e in n.DaSanitizzare.EtichetteSens)
                    query = query.Remove("n:" + e);

                query = this.CreaRemovePropSens(query, n.DaSanitizzare.PropSempreSens,n.DaSanitizzare.PropSensAssoc,"n");
            }

            query.ExecuteWithoutResultsAsync();
        }
    }

    private void SanitizzaCat(BoltGraphClient bgc, List<IDom> lc)
    {
        ICypherFluentQuery query;
        // sintrga con la clausola match
        string match;
        // stringa con la clausola where
        string where;
        // stringa con la clausola remove
        string remove;
        // stringa con la clausola delete
        string delete;

        // lista di stringe usata per contenere i valori delle etichette usate come 
        // parametri per inserire il vettore di etichette di ciascun nodo.
        // le etichette riferite ad altri campi stringa sono salvate in parEticStr
        List<string> parNodi = new List<string>();
        // lista parallela a parNodi che conteiene tutti i vettori di etichette di 
        // ciascun nodo
        List<string[]> parValNodi = new List<string[]>();

        // lista che contiene i valori delle etichette usate come parametri che 
        // verranno sostituiti da stringhe
        List<string> parEticStr = new List<string>();
        // lista parallela a parEticStr con le stringhe relative alle etichette
        List<string> parValStr = new List<string>();

        // coppia di liste analoghe a parEticStr e parValStr per i valori numerici
        List<string> parEticNum = new List<string>();
        List<double> parValNum = new List<double>();

        Relazione r;
        Nodo n;
        Catena c;
        

        if(lc.Count > 0 )
        {
            for(int j = 0; j < lc.Count; j++)
            {
                c = (Catena) lc[j];
                where = "";
                remove = "";
                delete = "";
                match = "(";

                for(int i = 0; i < c.Els.Count; i++)
                {
                    if(c.Els[i].GetType() == typeof(Relazione))
                    {
                        r = (Relazione)c.Els[i];
                        match += ")-[r" + j + i + "]->(";

                        if(!string.IsNullOrEmpty(r.IdRel.Etichetta))
                        {
                            if(string.IsNullOrEmpty(where))
                                where = "$etichetta" + j + i + " in labels(r"+ j + i + ")";
                            else 
                                where += " AND $etichetta" + j + i + " in labels(r"+ j + i + ")";
                            parEticStr.Add("etichetta" + j + i);
                            parValStr.Add(r.IdRel.Etichetta);
                        }

                        this.CreaWherePropCatena(ref where, r.IdRel.PropStr, r.IdRel.PropNum, ref parEticStr, ref parValStr, ref parEticNum, ref parValNum,"r"+j+i);

                        if(r.RelSens)
                            delete += "r" + j + i + ", ";
                        else if(r.DaSanitizzare != null)
                            remove += this.CreaRemovePropSensCatena(r.DaSanitizzare.PropSempreSens, r.DaSanitizzare.PropSensAssoc,"r"+j+i);
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
                            parNodi.Add("etichette" + j + i);
                            parValNodi.Add(n.IdNodo.Etichette);
                        }

                        this.CreaWherePropCatena(ref where, n.IdNodo.PropStr, n.IdNodo.PropNum, ref parEticStr, ref parValStr, ref parEticNum, ref parValNum,"n"+j+i);

                        if(n.NodoSens)
                            delete += "n" + j + i + ", ";
                        else if(n.DaSanitizzare != null)
                        {
                            // rimozione di tutte le etichette sempre sensibili 
                            foreach(string e in n.DaSanitizzare.EtichetteSens)
                                remove += "n" + j + i + ":" + e + ", ";
                            remove += this.CreaRemovePropSensCatena(n.DaSanitizzare.PropSempreSens, n.DaSanitizzare.PropSensAssoc,"n"+j+i);
                        }
                    }

                }

                match += ")";

                query = bgc.Cypher
                    .Match(match)
                    .Where(where);

                if(!string.IsNullOrEmpty(delete))    
                    // rimuovo la virgola infondo alla stringa delete
                    query = query.DetachDelete(delete.Remove(delete.Length - 2));

                if(!string.IsNullOrEmpty(remove))
                    // rimuovo la virgola infondo alla stringa remove
                    query = query.Remove(remove.Remove(remove.Length-2));

                // inserimento dei parametri usati per le etichette dei nodi 
                for(int i = 0; i < parNodi.Count; i++)
                    query = query.WithParam(parNodi[i],parValNodi[i]); 

                // inserimento dei parametri di tipo stringa
                for(int i = 0; i < parEticStr.Count; i++)
                    query = query.WithParam(parEticStr[i],parValStr[i]); 

                // inserimento dei parametri numerici
                for(int i = 0; i < parEticNum.Count; i++)
                    query = query.WithParam(parEticNum[i],parValNum[i]); 

                query.ExecuteWithoutResultsAsync();
            }
        }
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

        // aggiunta dei parametri in propStr. analogo a PropNum

        chiavi = propStr.Keys.ToList();
        flag = 0;
        if(!query.Query.QueryText.Contains("WHERE"))
        {
            query = query.Where(nomeElem + "." + chiavi[0] + " = $valuePS"+ nomeElem + "0").WithParam("valuePS" + nomeElem +"0", propStr[chiavi[0]]);
            flag++;
        }
        for(int i = 0 + flag; i <  chiavi.Count; i ++)
            query = query.AndWhere(nomeElem + "." + chiavi[i] + " = $valuePS" + nomeElem + i).WithParam("valuePS" + nomeElem  + i, propStr[chiavi[i]]);

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
        // rimozione di tutte le proprietà sempre sensibili
        foreach(string s in propSempreSens)
            query = query.Remove(nomeElem + "." + s);
                
        foreach(string p in propSensAssoc.Keys)
            if(propSensAssoc[p].SanitizzareProp)
                // se sanitizzare prop è a true, sanitizzo la proprietà in p, 
                // ovvero quella che è resa sensibile dal vettore di proprietà 
                // associate
                query = query.Remove(nomeElem + "." + p);
            else 
                // in caso contrario, sanitizzo tutte le proprietà associate
                foreach (string pAssoc in propSensAssoc[p].PropAssoc)
                    query = query.Remove(nomeElem + "." + pAssoc);

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
    /// <param name="where">parte di clausola where già scritta</param>
    /// <param name="propStr">proprietà PropStr di <c>IdRel</c> o <c>IdNodo</c></param>
    /// <param name="propNum">proprietà PropNum di <c>IdRel</c> o <c>IdNodo</c></param>
    /// <param name="parEticStr">lista usata per tenere traccia delle etichette usate come parametro</param>
    /// <param name="parValStr">lista parallela a <c>parEticStr</c> per contenere i valori corrispondenti a ciascuna etichetta</param>
    /// <param name="parEticNum">analogo di <c>parEticStr</c> per le etichette cui corrisponde un valore numerico</param>
    /// <param name="parValNum">analogo di <c>parValStr</c> che contiene i valori numerici relativi a <c>parEticNum</c></param>
    /// <param name="nomeElem">nome dato nella clausola match all'elemento di cui si vogliono rimuovere le informazioni sensibili</param>
    private void CreaWherePropCatena(ref string where, IDictionary<string,string> propStr, IDictionary<string,double> propNum, ref List<string> parEticStr, ref List<string> parValStr, ref List<string> parEticNum, ref List<double> parValNum, string nomeElem)
    {
        IList<string> chiavi = propNum.Keys.ToList();
        // flag usato per sapere se il primo elemento di propNum e propStr sia stato 
        // inserito a parte o meno. flag = 0 se non inserito a parte, 1 altrimenti
        int flag = 0;
        //inserimento degli elementi in propNum
        if(string.IsNullOrEmpty(where))
        {
            where = nomeElem + "." + chiavi[0] + " = $valuePN" + nomeElem + "0";
            parEticNum.Add("valuePN" + nomeElem + "0");
            parValNum.Add(propNum[chiavi[0]]);
            flag++;
        }
        // tramite il flag, so se includere o meno il primo elemento nel ciclo
        for(int k = 0 + flag; k < chiavi.Count; k++)
        {
            where += " AND " + nomeElem + "." + chiavi[k] + " = $valuePN" + nomeElem + k;
            parEticNum.Add("valuePN" + nomeElem + k);
            parValNum.Add(propNum[chiavi[k]]);
        }

        chiavi = propStr.Keys.ToList();
        flag = 0;
    
        //inserimento degli elementi in propstr
        if(string.IsNullOrEmpty(where))
        {
            where = nomeElem + "." + chiavi[0] + " = $valuePS" + nomeElem + "0";
            parEticStr.Add("valuePS" + nomeElem + "0");
            parValStr.Add(propStr[chiavi[0]]);
            flag++;
        }
        // tramite il flag, so se includere o meno il primo elemento nel ciclo
        for(int k = 0 + flag; k < chiavi.Count; k++)
        {
            where +=  " AND " + nomeElem + "." + chiavi[k] + " = $valuePS" + nomeElem + k;
            parEticStr.Add("valuePS" + nomeElem + k);
            parValStr.Add(propStr[chiavi[k]]);
        }
    }


    /// <summary>
    /// crea la parte di query con la rimozione delle informazioni sensibili specificate
    /// in <c>PropSempreSens</c> e <c>PropSensAssoc</c> all'interno degll' oggetto 
    /// <c>DaSanitizzareRel</c> o <c>DaSanitizzareNodo</c> per gli elementi del 
    /// dominio nelle catene
    /// </summary>
    /// <param name="remove">parte di clausola remove già composta in precedenza</param>
    /// <param name="propSempreSens">proprietà <c>PropSempreSens</c> di <c>DaSanitizzareRel</c> o <c>DaSanitizzareNodo</c></param>
    /// <param name="propSensAssoc">proprietà <c>PropSensAssoc</c> di <c>DaSanitizzareRel</c> o <c>DaSanitizzareNodo</c></param>
    /// <param name="nomeElem">nome dato nella clausola match all'elemento di cui si vogliono rimuovere le informazioni sensibili</param>
    /// <returns>
    /// ritorna la stringa <c>remove</c> aggioranta con le condizioni per rispettare
    /// quanto specificato in <c>propSempreSens</c> e in <c>propSensAssoc</c>
    /// </returns>
    private string CreaRemovePropSensCatena(string[] propSempreSens, IDictionary<string,PropSensAssoc_> propSensAssoc, string nomeElem)
    {
        string remove = "";
        
        // rimozione di tutte le proprietà sempre sensibili
        foreach(string s in propSempreSens)
            remove += nomeElem + "." + s + ", ";
                
        foreach(string p in propSensAssoc.Keys)
            if(propSensAssoc[p].SanitizzareProp)
                // se sanitizzare prop è a true, sanitizzo la proprietà in p, 
                // ovvero quella che è resa sensibile dal vettore di proprietà 
                // associate
                remove += nomeElem + "." + p + ", ";
            else 
                // in caso contrario, sanitizzo tutte le proprietà associate
                foreach (string pAssoc in propSensAssoc[p].PropAssoc)
                    remove += nomeElem + "." + pAssoc + ", ";

        return remove;
    }
    #endregion

}

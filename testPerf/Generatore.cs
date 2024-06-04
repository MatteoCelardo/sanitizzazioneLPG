using Neo4jClient;

namespace testPerf;

public class Generatore
{
    // liste per contenere tutte le etichette di nodi e relazioni, con 
    // tutte le proprietà presenti 
    private List<string> _eticNodi;
    private List<string> _eticRel;
    private List<string> _propNodi;
    private List<string> _propRel;
    // numero di elementi minimo per ogni catena
    private const int EL_PER_CAT = 2;

    #region costruttore
    public Generatore()
    {
        Task<IEnumerable<List<string>>> resLst;
        Task<IEnumerable<string>> resStr;

        if(!Directory.Exists("./fileInput"))
            Directory.CreateDirectory("./fileInput");

        if(!Directory.Exists("./fileInput/Nodi"))
            Directory.CreateDirectory("./fileInput/Nodi");
        
        if(!Directory.Exists("./fileInput/Rel"))
            Directory.CreateDirectory("./fileInput/Rel");

        if(!Directory.Exists("./fileInput/Cat"))
            Directory.CreateDirectory("./fileInput/Cat");

        _eticNodi = new List<string>();
        _eticRel = new List<string>();
        _propNodi = new List<string>();
        _propRel = new List<string>();

        resLst = DatiLista("(n)","DISTINCT labels(n) AS res");
        foreach(List<string> ls in resLst.Result)
            foreach(string s in ls)
                _eticNodi.Add(s);

        resStr = DatiStr("()-[r]->()","DISTINCT TYPE(r) AS res");
        foreach(string s in resStr.Result)
            _eticRel.Add(s);

        resLst = DatiLista("(n)","DISTINCT KEYS(n) AS res");
        foreach(List<string> ls in resLst.Result)
            foreach(string s in ls)
                _propNodi.Add(s);
        
        _propNodi = _propNodi.Distinct().ToList();

        resLst = DatiLista("()-[r]->()","DISTINCT KEYS(r) AS res");
        foreach(List<string> ls in resLst.Result)
            foreach(string s in ls)
                _propRel.Add(s);
        
        _propRel = _propRel.Distinct().ToList();

    }

    
    /// <summary>
    /// interroga il database con query che ritornano liste di stringhe. Metodo 
    /// usato per rimpire le liste _eticNodi, _propNodi e _propRel
    /// </summary>
    /// <param name="match">clausola match da usare</param>
    /// <param name="with">clausola with da usare</param>
    /// <returns>Task con i risultati della query</returns>
    private async Task<IEnumerable<List<string>>> DatiLista(string match, string with)
    {
        BoltGraphClient? client = new BoltGraphClient("bolt://3.85.228.201:7687", "neo4j", "stars-tens-quarters");
        await client.ConnectAsync();

        return await client.Cypher
            .Match(match)
            .With(with)
            .Return(res => res.As<List<string>>()).ResultsAsync;


    }

    /// <summary>
    /// interroga il database con query che ritornano liste di stringhe. Metodo 
    /// usato per rimpire la lista _eticRel
    /// </summary>
    /// <param name="match">clausola match da usare</param>
    /// <param name="with">clausola with da usare</param>
    /// <returns>Task con i risultati della query</returns>
    private async Task<IEnumerable<string>> DatiStr(string match, string with)
    {
        BoltGraphClient? client = new BoltGraphClient("bolt://3.85.228.201:7687", "neo4j", "stars-tens-quarters");
        await client.ConnectAsync();

        return await client.Cypher
            .Match(match)
            .With(with)
            .Return(res => res.As<string>()).ResultsAsync;
    }

    #endregion

    /// <summary>
    /// generatore di file d'input che crea diversi file con un nodo e percentuali di etichette 
    /// e proprietà diverse, in modo da aumentare di uno le etichetta o le proprietà dal 
    /// file precedente 
    /// </summary>
    public void GeneraFileUnNodo()
    {
        string nodo; 
        // generazione dei file con numero di etichette variabile
        for(int i = 1; i <= _eticNodi.Count; i++)
        {
            nodo = "{\"NodiSensibili\" : [";
            nodo += this.GeneraSingoloNodo((double)i/_eticNodi.Count * 100,0);
            nodo += "]\n}";
            File.WriteAllText("./fileInput/Nodi/FileSingoloNodo_"+Math.Round((double)i/_eticNodi.Count * 100,2)+"-0.json",nodo);
        }

        for(int i = 1; i <= _propNodi.Count; i++)
        {
            nodo = "{\"NodiSensibili\" : [";
            nodo += this.GeneraSingoloNodo(0,(double)i/_propNodi.Count * 100);
            nodo += "]\n}";
            File.WriteAllText("./fileInput/Nodi/FileSingoloNodo_0-"+Math.Round((double)i/_propNodi.Count * 100,2)+".json",nodo);
        }

    }

    /// <summary>
    /// generatore di file d'input che crea diversi file con una relazione e percentuali di etichette 
    /// e proprietà diverse, in modo da aumentare di uno le etichetta o le proprietà dal 
    /// file precedente 
    /// </summary>
    public void GeneraFileUnaRel()
    {
        string rel; 
        // generazione dei file con numero di etichette variabile
        for(int i = 1; i <= _eticRel.Count; i++)
        {
            rel = "{\"RelSensibili\" : [";
            rel += this.GeneraSingolaRel((double)i/_eticRel.Count * 100,0);
            rel += "]\n}";
            File.WriteAllText("./fileInput/Rel/FileSingoloNodo_"+Math.Round((double)i/_eticRel.Count * 100,2)+"-0.json",rel);
        }

        for(int i = 1; i <= _propRel.Count; i++)
        {
            rel = "{\"RelSensibili\" : [";
            rel += this.GeneraSingolaRel(0,(double)i/_propRel.Count * 100);
            rel += "]\n}";
            File.WriteAllText("./fileInput/Rel/FileSingoloNodo_0-"+Math.Round((double)i/_propRel.Count * 100,2)+".json",rel);
        }
    }

    public void GeneraFileCatene(int numCat)
    {
        for(int j = 0; j < numCat; j++)
        {
            string file = "{\"NodiSensibili\" : ["; 
            for(int i = 1; i <= EL_PER_CAT+j-Math.Floor((double)(EL_PER_CAT+j)/2); i++)
                file += this.GeneraSingoloNodo(100,100,"\"IdCat\" : \"idNodo"+i+"\",\n")+",\n";

            file = file.Remove(file.Length - 2, 2) + "],\n\"RelSensibili\" : [";

            for(int i = 1; i <= Math.Floor((double)(EL_PER_CAT+j)/2); i++)
                file += this.GeneraSingolaRel(100,100,"\"IdCat\" : \"idRel"+i+"\",\n")+",\n";

            file = file.Remove(file.Length - 2, 2) + "],\n\"Catene\" : [[";
            for(int i = 1; i <= Math.Floor((double)(EL_PER_CAT+j)/2); i++)
                file += "\"idNodo"+i+"\", \"idRel"+i+"\", ";
            
            if((EL_PER_CAT+j)%2 == 1)
                file += "\"IdNodo"+(Math.Floor((double)(EL_PER_CAT+j)/2)+1)+"\"";
            else
                file = file.Remove(file.Length - 2,2);

            file += "]]\n}";
            File.WriteAllText("./fileInput/Cat/FileCatena_"+j+".json",file);
        }
    }


    /// <summary>
    /// genera un file d'input con un singolo oggetto JSON per i nodi. L'oggetto 
    /// non avrà necessariamente riscontro con un nodo presente nel database
    /// </summary>
    /// <param name="percEtic">percentuale di etichette da usare rispetto a tutte quelle nel DB</param>
    /// <param name="percProp">percentuale di proprietà da usare rispetto a tutte quelle nel DB</param>
    /// <remarks>
    /// A prescindere dalla percentuale specificata, il numero minimo di etichette e 
    /// proprietà sarà 1 
    /// </remarks>
    /// <returns>
    /// ritorna una stringa che contiene l'oggetto JSON
    /// </returns>
    private string GeneraSingoloNodo(double percEtic, double percProp, string idCat = "")
    {
        double maxEtic = _eticNodi.Count * percEtic / 100 >= 1 ? Math.Round((double)_eticNodi.Count * percEtic / 100,5) : 1;  
        double maxProp = _propNodi.Count * percProp / 100 >= 1 || _propNodi.Count == 0 ? Math.Round((double)_propNodi.Count * percProp / 100,5) : 1;  
        string oggJSON = "{"+idCat+"\"IdNodo\":\n\t{\"Etichette\" : [";

        #region costruzione dell'oggetto identificativo
        for(int i = 0; i < maxEtic; i++)
            oggJSON += "\"" + _eticNodi[i] + "\", ";
        oggJSON = oggJSON.Remove(oggJSON.Length - 2,2) +  "],";

        if(maxProp > 0)
        {
            oggJSON += "\n\t\"PropStr\" : {";
            for(int i = 0; i < maxProp; i++)
                oggJSON += "\"" + _propNodi[i] + "\" : \"test\", ";
            oggJSON = oggJSON.Remove(oggJSON.Length - 2,2) +  "},";

            oggJSON += "\n\t\"PropNum\" : {";
            for(int i = 0; i < maxProp; i++)
                oggJSON += "\"" + _propNodi[i] + "\" : 1, ";
            oggJSON = oggJSON.Remove(oggJSON.Length - 2,2) +  "}},";
            #endregion

            oggJSON += "\n\t\"NodoSens\" : false,";

            #region costruzione dell'oggetto di sanitizzazione
            oggJSON += "\n\t\"DaSanitizzare\" : { \n\t\t\"EtichetteSens\" : [";
            for(int i = 0; i < maxEtic; i++)
                oggJSON += "\"" + _eticNodi[i] + "\", ";
            oggJSON = oggJSON.Remove(oggJSON.Length - 2,2) +  "],";

            oggJSON += "\n\t\t\"PropSempreSens\" : [";
            for(int i = 0; i < maxProp; i++)
                oggJSON += "\"" + _propNodi[i] + "\", ";
            oggJSON = oggJSON.Remove(oggJSON.Length - 2,2) +  "]";
            
            if(maxProp > 2)
            {
                oggJSON += ",\n\t\t\"PropSensAssoc\" : {";
                for(int j = 0; j < 2; j++)
                {   
                    oggJSON += "\n\t\t\t\"" + _propNodi[j] + "\" : {\n\t\t\t\t\"PropAssoc\" : [";
                    for(int i = 1 + j; i < maxProp; i++)
                        oggJSON += "\"" + _propNodi[i] + "\", ";
                    oggJSON = oggJSON.Remove(oggJSON.Length - 2,2) +  "],\n\t\t\t\t\"SanitizzareProp\" : " + Convert.ToBoolean(j).ToString().ToLower() + "},";
                }
                oggJSON = oggJSON.Remove(oggJSON.Length - 2,2) +  "}\n\t\t\t}";
            }
        }
        else 
            oggJSON += "\n\t\"NodoSens\" : true";
        #endregion

        oggJSON += "\n\t\t}\n\t}";

        return oggJSON+",\n"+oggJSON+",\n"+oggJSON+",\n"+oggJSON+",\n"+oggJSON+",\n"+oggJSON+",\n"+oggJSON+",\n"+oggJSON+",\n"+oggJSON+",\n"+oggJSON+",\n"+oggJSON+",\n"+oggJSON+",\n"+oggJSON+",\n"+oggJSON+",\n"+oggJSON;

    }

    /// <summary>
    /// genera un file d'input con un singolo oggetto JSON per le relazioni. L'oggetto 
    /// non avrà necessariamente riscontro con un nodo presente nel database
    /// </summary>
    /// <param name="percEtic">percentuale di etichette da usare rispetto a tutte quelle nel DB</param>
    /// <param name="percProp">percentuale di proprietà da usare rispetto a tutte quelle nel DB</param>
    /// <remarks>
    /// A prescindere dalla percentuale specificata, il numero minimo di etichette e 
    /// proprietà sarà 1 
    /// </remarks>
    /// <returns>
    /// ritorna una stringa che contiene l'oggetto JSON
    /// </returns>
    private string GeneraSingolaRel(double percEtic, double percProp, string idCat = "")
    {
        double maxEtic = _eticRel.Count * percEtic / 100 >= 1 ? Math.Round((double)_eticRel.Count * percEtic / 100,5) : 1;  
        double maxProp = _propRel.Count * percProp / 100 >= 1 || _propRel.Count == 0 ? Math.Round((double)_propRel.Count * percProp / 100,5) : 1;  
        string oggJSON = "{"+idCat+"\"IdRel\":\n\t{\"Etichette\" : [";

        #region costruzione dell'oggetto identificativo
        for(int i = 0; i < maxEtic; i++)
            oggJSON += "\"" + _eticRel[i] + "\", ";
        oggJSON = oggJSON.Remove(oggJSON.Length - 2,2) +  "],";

        if(maxProp > 0)
        {
            oggJSON += "\n\t\"PropStr\" : {";
            for(int i = 0; i < maxProp; i++)
                oggJSON += "\"" + _propRel[i] + "\" : \"test\", ";
            oggJSON = oggJSON.Remove(oggJSON.Length - 2,2) +  "},";

            oggJSON += "\n\t\"PropNum\" : {";
            for(int i = 0; i < maxProp; i++)
                oggJSON += "\"" + _propRel[i] + "\" : 1, ";
            oggJSON = oggJSON.Remove(oggJSON.Length - 2,2) +  "}},";
        
        #endregion

            oggJSON += "\n\t\"RelSens\" : false,";

            #region costruzione dell'oggetto di sanitizzazione
            oggJSON += "\n\t\"DaSanitizzare\" : { \n\t\t\"PropSempreSens\" : [";
            for(int i = 0; i < maxProp; i++)
                oggJSON += "\"" + _propRel[i] + "\", ";
            oggJSON = oggJSON.Remove(oggJSON.Length - 2,2) +  "]";

            if(maxProp > 2)
            {
                oggJSON += ",\n\t\t\"PropSensAssoc\" : {";
                for(int j = 0; j < 2; j++)
                {   
                    oggJSON += "\n\t\t\t\"" + _propRel[j] + "\" : {\n\t\t\t\t\"PropAssoc\" : [";
                    for(int i = 1 + j; i < maxProp; i++)
                        oggJSON += "\"" + _propRel[i] + "\", ";
                    oggJSON = oggJSON.Remove(oggJSON.Length - 2,2) +  "],\n\t\t\t\t\"SanitizzareProp\" : " + Convert.ToBoolean(j).ToString().ToLower() + "},";
                }
                oggJSON = oggJSON.Remove(oggJSON.Length - 2,2) +  "}\n\t\t\t}";
            }
        }
        else 
            oggJSON += "\n\t\"RelSens\" : true";
        #endregion

        oggJSON += "\n\t\t}\n\t}";

        return oggJSON+",\n"+oggJSON+",\n"+oggJSON+",\n"+oggJSON+",\n"+oggJSON+",\n"+oggJSON+",\n"+oggJSON+",\n"+oggJSON+",\n"+oggJSON+",\n"+oggJSON+",\n"+oggJSON+",\n"+oggJSON+",\n"+oggJSON+",\n"+oggJSON+",\n"+oggJSON;

    }
}

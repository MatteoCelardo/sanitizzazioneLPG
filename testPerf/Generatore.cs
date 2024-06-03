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

    #region costruttore
    public Generatore()
    {
        Task<IEnumerable<List<string>>> resLst;
        Task<IEnumerable<string>> resStr;

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
    /// genera un file d'input con gli oggetti JSON per i singoli nodi
    /// </summary>
    /// <param name="percEtic">percentuale di etichette da usare</param>
    /// <param name="percProp">percentuale di proprietà da usare</param>
    /// <remarks>
    /// A prescindere dalla percentuale specificata, il numero minimo di etichette e 
    /// proprietà sarà 1 
    /// </remarks>
    public async void FileNodi(int percEtic, int percProp)
    {
        decimal maxEtic = _eticNodi.Count * percEtic / 100 >= 1 ? Math.Floor((decimal)_eticNodi.Count * percEtic / 100) : 1;  
        decimal maxProp = _propNodi.Count * percProp / 100 >= 1 ? Math.Floor((decimal)_propNodi.Count * percProp / 100) : 1;  
        string idJSON;

        for(int i = 0; i < maxEtic; i++)
        {
            idJSON = "{\"IdNodo\":{\"Etichette\" : [\"" + _eticNodi[i] + "\"]}}";
        }
    }

    /// <summary>
    /// genera un file d'input con gli oggetti JSON per le singole relazioni
    /// </summary>
    /// <param name="percEtic">percentuale di etichette da usare</param>
    /// <param name="percProp">percentuale di proprietà da usare</param>
    /// <remarks>
    /// A prescindere dalla percentuale specificata, il numero minimo di etichette e 
    /// proprietà sarà 1 
    /// </remarks>
    public async void FileRel(int percEtic, int percProp)
    {}

    /// <summary>
    /// genera un file d'input con gli oggetti JSON per le singole catene
    /// </summary>
    /// <param name="percEtic">percentuale di etichette da usare</param>
    /// <param name="percProp">percentuale di proprietà da usare</param>
    /// <remarks>
    /// A prescindere dalla percentuale specificata, il numero minimo di etichette e 
    /// proprietà sarà 1 
    /// </remarks>
    public async void FileCat(int percEtic, int percProp)
    {}
}

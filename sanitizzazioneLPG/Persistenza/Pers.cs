using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using sanitizzazioneLPG.Dominio;
using Avalonia.Platform;


namespace sanitizzazioneLPG.Persistenza;

public class Pers : IPers
{
    // variabile usata per contenere lo schema JSON
    private readonly JSchema _schVal;
    // liste usate per contenere tutti gli elementi da sanitizzare
    private List<Nodo> _nodi;
    private List<Relazione> _relazioni;
    private List<Catena> _catene;

    #region singleton
    private static Pers? _istanza = null;
    private static object _mutex = new object();


    private Pers(){
        // Caricamento in memoria del file con lo schema JSON
        StreamReader file = new StreamReader(AssetLoader.Open(new Uri("avares://"+ typeof(Program).Assembly.GetName().Name +"/Risorse/Schema.json")));
        JsonTextReader reader = new JsonTextReader(file);
        _schVal = JSchema.Load(reader); // memorizzazione dello schema JSON

        _nodi = new List<Nodo>();
        _relazioni = new List<Relazione>();
        _catene = new List<Catena>();
    } 

    public static Pers Istanza {
        get 
        {
            lock(_mutex)
            {
                return _istanza ?? new Pers();
            }
        }
    }
    #endregion

    #region metodi interfaccia
    public void Cancella()
    {
        _nodi.Clear();
        _relazioni.Clear();
        _catene.Clear();
    }

    public void Crea(string path)
    {
        IDom? d;
        string json;
        FileJson? dati;

        // generazione di un'eccezione se sno già presenti dati in memoria
        if(_nodi.Count > 0 || _relazioni.Count > 0 || _catene.Count > 0)
            throw new PersExc("impossibile caricare il contenuto del file JSON: sono già presenti informazioni nella persistenza. Cancellarle prima di importare nuovi dati.");

        json = File.ReadAllText(path);
        // parsing del file JSON per ottenere i rispettivi oggetti C#
        dati = JsonConvert.DeserializeObject<FileJson>(json);

        // importazione di nodi, relazioni e catene senisbili negli attributi della 
        // persistenza.
        // nessuna verifica sul fatto che dati possa essere null siccome è imposto 
        // dallo schema JSON che il file specificato contenga qualcosa
        if(dati.nodiSensibili != null)
            _nodi.AddRange(dati.nodiSensibili);

        if(dati.relSensibili != null)
            _relazioni.AddRange(dati.relSensibili);
        
        if(dati.catene != null)
        {
            for(int i = 0; i < dati.catene.Length; i++)
            {
                // verifica che una catena non contenga più volte lo stesso id
                if(dati.catene[i].Length != dati.catene[i].Distinct().Count())
                    throw new PersExcDupl("la catena numero " + i + " contiene id duplicati.");

                _catene.Add(new Catena());
                // inserimento del primo elemento della catena per semplificare le 
                // operazioni nel ciclo for
                d = (IDom?)_nodi.Find(n => n.IdCat != null && n.IdCat.Equals(dati.catene[i][0])) 
                    ?? _relazioni.Find(r => r.IdCat != null && r.IdCat.Equals(dati.catene[i][0]));
                _catene[i].Els.Add(d ?? throw new PersExcNotFound("l'id specificato nella catena " + i + " in posizione 0 non corrisponde nessun nodo o relazione."));
                
                for(int j = 1; j < dati.catene[i].Length; j++)
                {
                    // ricerca dell'oggetto che ha come id la stringa contenuta in dati.catene[i][j]
                    // nel caso non esista un nodo o una relazione con quell'id, viene sollevata un'eccezione 
                    d = (IDom?)_nodi.Find(n => n.IdCat != null && n.IdCat.Equals(dati.catene[i][j])) 
                        ?? _relazioni.Find(r => r.IdCat != null && r.IdCat.Equals(dati.catene[i][j])) 
                        ?? throw new PersExcNotFound("l'id specificato nella catena " + i + " in posizione " + j + " non corrisponde a nessun nodo o relazione");
                    
                    // se l'elemento appena trovato è dello stesso tipo del precedente, viene sollevata un'eccezione
                    if(_catene[i].Els[j-1].GetType() == d.GetType())
                        throw new PersExcDupl("l'elemento in posizione " + j + " della catena " + i + " è dello stesso tipo dell'elemento precedente.");
                    
                    _catene[i].Els.Add(d);
                }
            }
        }
        // rimozione dalla lista dei nodi e delle relazioni degli elementi che 
        // fanno parte di una catena. gli elementi che non fanno parte di una catena 
        // ma hanno idCat settato verranno rimossi ugualmente
        _nodi.RemoveAll(n => n.IdCat != null);
        _relazioni.RemoveAll(r => r.IdCat != null);
    }

    public List<IDom> ListAll(EnumTipoDom etd)
    {
        switch(etd)
        {
            case EnumTipoDom.NODI:
                return new List<IDom>(_nodi); 
            case EnumTipoDom.RELAZIONI: 
                return new List<IDom>(_relazioni);
            case EnumTipoDom.CATENE:
                return new List<IDom>(_catene);  
            default: 
                throw new ArgumentException("l'enumerativo passato non è valido.");
        }
    }

    public IList<ValidationError> Valida(string path){
        // lettura del file da validare
        JObject json = JObject.Parse(File.ReadAllText(path));
        //inizializzazione della lista che conterrà gli eventuali errori riscontrati nel parsing
        IList<ValidationError> errori = new List<ValidationError>();

        json.IsValid(_schVal, out errori);

        return errori;
            
    }
    #endregion


    //classe privata di appoggio usata dal parser newtonsoft per ricavare gli oggetti C# dal file JSON
    class FileJson {
        public Nodo[]? nodiSensibili { get; set; }
        public Relazione[]? relSensibili {  get; set; } 
        public string[][]? catene {get; set;}
    }

    
}

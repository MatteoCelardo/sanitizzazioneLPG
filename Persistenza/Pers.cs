﻿
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Reflection;

namespace sanitizzazioneLPG;

public class Pers : IPers
{
    private readonly JSchema schVal;

    private List<Nodo> nodi;
    private List<Relazione> relazioni;
    private List<Catena> catene;

    #region singleton
    private static Pers? istanza = null;
    private static object mutex = new object();


    private Pers(){
        Assembly assembly = Assembly.GetExecutingAssembly();
        StreamReader file = new StreamReader(assembly.GetManifestResourceStream("sanitizzazioneLPG.Risorse.Schema.json"));
        JsonTextReader reader = new JsonTextReader(file);
        schVal = JSchema.Load(reader);

        nodi = new List<Nodo>();
        relazioni = new List<Relazione>();
        catene = new List<Catena>();
    } 

    public static Pers Istanza {
        get 
        {
            lock(mutex)
            {
                if (istanza == null)
                    istanza = new Pers();
                return istanza;
            }
        }
    }
    #endregion

    public void Cancella()
    {
        nodi.Clear();
        relazioni.Clear();
        catene.Clear();
    }

    public void Crea(string path)
    {
        string json = File.ReadAllText(path);
        // parsing del file JSON per ottenere i rispettivi oggetti C#
        FileJson? dati = JsonConvert.DeserializeObject<FileJson>(json);
        
        if (dati == null)
            throw new PersEcc("il file al percorso " + path + " è vuoto");

        // importazione di nodi, relazioni e catene senisbili negli attributi della 
        // persistenza
        if(dati.nodiSensibili != null)
            nodi.AddRange(dati.nodiSensibili);

        if(dati.relSensibili != null)
            relazioni.AddRange(dati.relSensibili);
        
        if(dati.catene != null)
            foreach(string[] c in dati.catene)
                catene.Add(new Catena(c));
    }

    public List<IDom> ListAll(EnumTipoDom etd)
    {
        switch(etd)
        {
            case EnumTipoDom.NODI:
                if(nodi.Count == 0)
                    throw new PersEcc("La lista dei nodi è vuota");
                return new List<IDom>(nodi); 
            case EnumTipoDom.RELAZIONI: 
                if(relazioni.Count == 0)
                    throw new PersEcc("La lista delle relazioni è vuota");
                return new List<IDom>(relazioni);
            case EnumTipoDom.CATENE:
                if(catene.Count == 0)
                    throw new PersEcc("La lista delle catene è vuota");
                return new List<IDom>(catene);  
            default: 
                throw new ArgumentException("l'enumerativo passato non è valido");
        }
    }

    public List<string> Valida(string path){
        JObject json = JObject.Parse(File.ReadAllText(path));
        IList<ValidationError> errori = new List<ValidationError>();
        List<string> temp = new List<string>();

        if(json.IsValid(schVal, out errori))
            return temp;
        else 
        {
            foreach(ValidationError e in errori)
                temp.Add("Linea numero: " + e.LineNumber + " - Percorso: " + e.Path + " - Valore: " + e.Value + "\n" + "Errore: " + e.Message + "\n----\n");

            return temp;
        }
            
    }


    //classe privata di appoggio usata dal parser newtonsoft per ricavare gli oggetti C# dal file JSON
    class FileJson {
        public Nodo[]? nodiSensibili { get; set; }
        public Relazione[]? relSensibili {  get; set; } 
        public string[][]? catene {get; set;}
    }

    
}


using Newtonsoft.Json;
using Newtonsoft.Json.Schema;

namespace sanitizzazioneLPG;

public class Pers : IPers
{
    private static Pers? istanza = null;
    private static object mutex = new object();
    private readonly JSchema schVal;

    private List<Nodo> nodi;
    private List<Relazione> relazioni;
    private List<Catena> catene;


    private Pers(){
        // inserire l'inport dello schema di validazione in schVal
        /*
        StreamReader file = File.OpenText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)+"/schemaCatene.json");
        JsonTextReader reader = new JsonTextReader(file);
            
        schVal = JSchema.Load(reader);*/

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

    public void cancella()
    {
        throw new NotImplementedException();
    }

    public void crea(string path)
    {
        string json = File.ReadAllText(path);
        // parsing del file JSON per ottenere i rispettivi oggetti C#
        FileJson dati = JsonConvert.DeserializeObject<FileJson>(json);
        
        // importazione dei nodi e delle relazioni senisbili
        nodi.AddRange(dati.nodiSensibili);
        relazioni.AddRange(dati.relSensibili);
        
        
        foreach(string[] c in dati.catene)
        {
            catene.Add(new Catena(c));
        }
    }

    public List<IDom> listAll(EnumTipoDom etd)
    {
        throw new NotImplementedException();
    }

    class FileJson {
        public Nodo[] nodiSensibili { get; set; }
        public Relazione[] relSensibili {  get; set; } 
        public string[][] catene {get; set;}

    }
}

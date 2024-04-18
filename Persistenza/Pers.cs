
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;

namespace sanitizzazioneLPG;

public class Pers : IPers
{
    private static Pers istanza = null;
    private static object mutex = new object();
    private readonly JSchema schVal;

    //VALUTARE SE USARE LISTE DI STRINGHE PER QUESTI 3 ATTRIBUTI
    private Dictionary<string, Nodo> nodi;
    private Dictionary<string, Nodo> relazioni;
    private Dictionary<string, Nodo> catene;


    private Pers(){
        // inserire l'inport dello schema di validazione in schVal
        StreamReader file = File.OpenText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)+"/schemaCatene.json");
        JsonTextReader reader = new JsonTextReader(file);
            
        schVal = JSchema.Load(reader);
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
        throw new NotImplementedException();
    }

    public List<IDom> listAll(EnumTipoDom etd)
    {
        throw new NotImplementedException();
    }
}

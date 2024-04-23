using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Reflection;

namespace sanitizzazioneLPG;


class Program
{
    static void Main(string[] args)
    {
        /*
        Assembly assembly = Assembly.GetExecutingAssembly();
        StreamReader file = new StreamReader(assembly.GetManifestResourceStream("sanitizzazioneLPG.Risorse.Schema.json"));

        string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/proveJSON/provaRegoleSchema.json";

        string json = File.ReadAllText(path);

        JsonTextReader reader = new JsonTextReader(file);
            
        JSchema schema = JSchema.Load(reader);

        JObject vettore = JObject.Parse(json);

        IList<ValidationError> errors;
        bool prova = vettore.IsValid(schema, out errors);

        FileJson data = JsonConvert.DeserializeObject<FileJson>(json);



        foreach (ValidationError e in errors)
            Console.Write(e.Message);*/
        
    }

    /*
    class FileJson {
        public Nodo[]? nodiSensibili { get; set; }
        public Relazione[]? relSensibili {  get; set; } 
        public string[][]? catene {get; set;}
    }*/

}

namespace sanitizzazioneLPG;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.IO;
using System.Reflection;



class Program
{
    static void Main(string[] args)
    {
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



        Console.Write(data.RelSensibili[0].IdRel.Etichetta);

    }

    public class FileJson
    {
        public Nodo[] NodiSensibili { get; set; }
        public Relazione[] RelSensibili { get; set; }
        public string[][] Catene { get; set; }
    }

    public interface IDOM { }



    public class Relazione : IDOM
    {
        private string _idCat;
        private _IdRel _idRel;
        private bool _relSens;
        private _DaSanitizzareRel _daSanitizzare;

        public Relazione(string idCat, _IdRel idRel, bool relSens, _DaSanitizzareRel ds)
        {
            _idCat = idCat;
            _idRel = idRel;
            _relSens = relSens;
            _daSanitizzare = ds;
        }

        public string IdCat { get => _idCat; set => _idCat = value; }
        public _IdRel IdRel { get => _idRel; set => _idRel = value; }
        public bool RelSens { get => _relSens; set => _relSens = value; }
        public _DaSanitizzareRel DaSanitizzare { get => _daSanitizzare; set => _daSanitizzare = value; }

    }

    public class _IdRel
    {
        private string _etichetta;
        private IDictionary<string, string> _propStr;
        private IDictionary<string, double> _propNum;

        public _IdRel(string etichetta, IDictionary<string, string> propStr, IDictionary<string, double> propNum)
        {
            _etichetta = etichetta;
            _propNum = propNum;
            _propStr = propStr;
        }

        public string Etichetta { get => _etichetta; set => _etichetta = value; }
        public IDictionary<string, string> PropStr { get => _propStr; set => _propStr = value; }
        public IDictionary<string, double> PropNum { get => _propNum; set => _propNum = value; }
    }

    public class _DaSanitizzareRel
    {
        private string[] _propSempreSens;
        private IDictionary<string, _PropSensAssoc> _propSensAssoc;

        public _DaSanitizzareRel(string[] propSempreSens, IDictionary<string, _PropSensAssoc> propSensAssoc)
        {
            _propSempreSens = propSempreSens;
            _propSensAssoc = propSensAssoc;
        }

        public string[] PropSempreSens { get => _propSempreSens; set => _propSempreSens = value; }
        public IDictionary<string, _PropSensAssoc> PropSensAssoc { get => _propSensAssoc; set => _propSensAssoc = value; }

    }

    public class _PropSensAssoc
    {
        private string[] _propAssoc;
        private bool _sanitizzareProp;

        public _PropSensAssoc(string[] propAssoc, bool sanitizzareProp)
        {
            _propAssoc = propAssoc;
            _sanitizzareProp = sanitizzareProp;
        }

        public string[] PropAssoc { get => _propAssoc; set => _propAssoc = value; }
        public bool SanitizzareProp { get => _sanitizzareProp; set => _sanitizzareProp = value; }

    }

    public class Nodo : IDOM
    {
        private string _idCat;
        private _IdNodo _idNodo;
        private bool _nodoSens;
        private _DaSanitizzareNodo _daSanitizzare;

        public Nodo(string idCat, _IdNodo idNodo, bool nodoSens, _DaSanitizzareNodo daSanitizzare)
        {
            _idCat = idCat;
            _idNodo = idNodo;
            _nodoSens = nodoSens;
            _daSanitizzare = daSanitizzare;
        }

        public string IdCat { get => _idCat; set => _idCat = value; }
        public _IdNodo IdNodo { get => _idNodo; set => _idNodo = value; }
        public bool NodoSens { get => _nodoSens; set => _nodoSens = value; }
        public _DaSanitizzareNodo DaSanitizzare { get => _daSanitizzare; set => _daSanitizzare = value; }

    }

    public class _IdNodo
    {
        private string[] _etichette;
        private IDictionary<string, string> _propStr;
        private IDictionary<string, double> _propNum;

        public _IdNodo(string[] etichette, IDictionary<string, string> propStr, IDictionary<string, double> propNum)
        {
            _etichette = etichette;
            _propStr = propStr;
            _propNum = propNum;
        }

        public string[] Etichette { get => _etichette; set => _etichette = value; }
        public IDictionary<string, string> PropStr { get => _propStr; set => _propStr = value; }
        public IDictionary<string, double> PropNum { get => _propNum; set => _propNum = value; }

    }

    public class _DaSanitizzareNodo
    {
        private string[] _etichetteSens;
        private string[] _propSempreSens;
        private IDictionary<string, _PropSensAssoc> _propSensAssoc;

        public _DaSanitizzareNodo(string[] etichetteSens, string[] propSempreSens, IDictionary<string, _PropSensAssoc> propSensAssoc)
        {
            _etichetteSens = etichetteSens;
            _propSempreSens = propSempreSens;
            _propSensAssoc = propSensAssoc;
        }


        public string[] EtichetteSens { get => _etichetteSens; set => _etichetteSens = value; }
        public string[] PropSempreSens { get => _propSempreSens; set => _propSempreSens = value; }
        public IDictionary<string, _PropSensAssoc> PropSensAssoc { get => _propSensAssoc; set => _propSensAssoc = value; }

    }

}

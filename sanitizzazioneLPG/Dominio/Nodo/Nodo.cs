namespace sanitizzazioneLPG.Dominio;

public class Nodo : IDom
{
    private string? _idCat;
    private IdNodo_ _idNodo;
    private bool _nodoSens;
    private DaSanitizzareNodo_? _daSanitizzare;

    public Nodo(string idCat, IdNodo_ idNodo, bool nodoSens, DaSanitizzareNodo_ daSanitizzare)
    {
        _idCat = idCat;
        _idNodo = idNodo;
        _nodoSens = nodoSens;
        _daSanitizzare = daSanitizzare;
    }

    public Nodo(IdNodo_ idNodo, bool nodoSens, DaSanitizzareNodo_ daSanitizzare)
    {
        _idNodo = idNodo;
        _nodoSens = nodoSens;
        _daSanitizzare = daSanitizzare;
    }

    public Nodo()
    {
    }

    public string? IdCat { get => _idCat; set => _idCat = value; }
    public IdNodo_ IdNodo { get => _idNodo; set => _idNodo = value; }
    public bool NodoSens {  get => _nodoSens; set => _nodoSens = value; }
    public DaSanitizzareNodo_? DaSanitizzare { get => _daSanitizzare; set => _daSanitizzare = value; }

}

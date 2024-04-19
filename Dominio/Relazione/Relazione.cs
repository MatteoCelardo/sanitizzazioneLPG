namespace sanitizzazioneLPG;

public class Relazione : IDom
{
    private string? _idCat; 
    private IdRel_ _idRel;
    private bool _relSens;
    private DaSanitizzareRel_ _daSanitizzare;

    public Relazione(string idCat, IdRel_ idRel, bool relSens, DaSanitizzareRel_ ds)
    {
        _idCat = idCat;
        _idRel = idRel;
        _relSens = relSens;
        _daSanitizzare = ds;
    }  

    public Relazione(IdRel_ idRel, bool relSens, DaSanitizzareRel_ ds)
    {
        _idRel = idRel;
        _relSens = relSens;
        _daSanitizzare = ds;
    } 

    public string? IdCat { get => _idCat ; set => _idCat = value; }
    public IdRel_ IdRel { get => _idRel; set => _idRel = value; }
    public bool RelSens { get => _relSens; set => _relSens = value; }
    public DaSanitizzareRel_ DaSanitizzare { get => _daSanitizzare; set => _daSanitizzare = value; }

}

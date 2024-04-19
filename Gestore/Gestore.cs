namespace sanitizzazioneLPG;

public class Gestore : IGestore
{
    private readonly IPers _pers;

    #region singleton
    private static Gestore? _istanza = null;
    private static object mutex = new object();

    private Gestore()
    {
        _pers = Pers.Istanza;
    }

    public static Gestore Istanza
    {
        get 
        {
            lock(mutex)
            {
                if (_istanza == null)
                    _istanza = new Gestore();

                return _istanza;
            }
        }
    }
    #endregion


    public void cancellaJSON()
    {
        _pers.Cancella();
    }

    public void importaJSON(string path)
    {
        try
        {
            _pers.Crea(path);

            //implementare una classe con un metodo statico di questo tipo. vedere se aggiungere la possibilità di usare bottoni e specificare l'icona.
            //inserire un messaggio per dire che tutto è andato a buon fine
            //MessageBox.Show("tipo dominio specificato non esistente in questo contesto", "Erroe di aggiornamento", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }
        catch (PersExc e) 
        {
            // inserire un messagebox con e.Message
        }
    }

    public void sanitizzaDB(EnumSanit s)
    {
        throw new NotImplementedException();
    }

    public void validaJSON(string path)
    {
        List<string> err = _pers.Valida(path);

        if(err.Count == 0)
            //inserire un message box per dire che la validazione è andata a buon fine
        else 
            //inserire un message box che contenga tutti quanti gli errori in err
    }
}

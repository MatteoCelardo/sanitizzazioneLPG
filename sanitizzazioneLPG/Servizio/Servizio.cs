using System;
using System.Collections.Generic;
using sanitizzazioneLPG.Persistenza;

namespace sanitizzazioneLPG.Servizio;

public class Servizio : IServizio
{
    private readonly IPers _pers;

    #region singleton
    private static Servizio? _istanza = null;
    private static object mutex = new object();

    private Servizio()
    {
        _pers = Pers.Istanza;
    }

    public Servizio Istanza 
    {
        get 
        {
            lock(mutex)
            {
                return _istanza ?? new Servizio();
            }
        }
    }
    #endregion

    public void CancellaJSON()
    {
        _pers.Cancella();
    }

    public void ImportaJSON(string path)
    {
        try
        {
            if (ValidaJSON(path))
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

    public void SanitizzaDB(EnumSanit s)
    {
        throw new NotImplementedException();
    }

    public bool ValidaJSON(string path)
    {
        List<string> err = _pers.Valida(path);
        bool ret = err.Count == 0; 
        
        /*
        if(ret)
            //inserire un message box per dire che la validazione è andata a buon fine
        else 
            //inserire un message box che contenga tutti quanti gli errori in err
            */
        return ret;
    }
}

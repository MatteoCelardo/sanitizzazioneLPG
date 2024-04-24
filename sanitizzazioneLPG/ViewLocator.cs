using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using sanitizzazioneLPG.ModelliViste;

namespace sanitizzazioneLPG;

public class ViewLocator : IDataTemplate
{

    public Control? Build(object? data)
    {
        if (data is null)
            return null;
        
        var name = data.GetType().FullName!.Replace("ModelloVista", "Vista", StringComparison.Ordinal);
        var type = Type.GetType(name);

        if (type != null)
        {
            var control = (Control)Activator.CreateInstance(type)!;
            control.DataContext = data;
            return control;
        }
        
        return new TextBlock { Text = "Non trovato: " + name };
    }

    public bool Match(object? data)
    {
        return data is ModelloVistaBase;
    }
}

using System.Collections.Generic;

namespace sanitizzazioneLPG.Dominio;

public class DaSanitizzareRel_
{
        private string[] _propSempreSens;
        private IDictionary<string, PropSensAssoc_> _propSensAssoc;

        public DaSanitizzareRel_(string[] propSempreSens, IDictionary<string, PropSensAssoc_> propSensAssoc)
        {
                _propSempreSens = propSempreSens;
                _propSensAssoc = propSensAssoc;
        }

        public DaSanitizzareRel_ ()
        {
                _propSempreSens = [];
                _propSensAssoc = new Dictionary<string, PropSensAssoc_>();
        }

        public string[] PropSempreSens { get => _propSempreSens; set => _propSempreSens = value; }
        public IDictionary<string, PropSensAssoc_> PropSensAssoc { get => _propSensAssoc; set => _propSensAssoc = value; }

}

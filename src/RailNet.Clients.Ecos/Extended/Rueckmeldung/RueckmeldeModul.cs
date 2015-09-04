using System.Collections.Generic;

namespace RailNet.Clients.Ecos.Extended.Rueckmeldung
{
    internal class RueckmeldeModul
    {
        private readonly List<Rueckmelder> _rueckmelder;

        public IReadOnlyList<Rueckmelder> Rueckmelder => _rueckmelder;

        public int Id { get; private set; }

        private int _ports;

        public int Ports
        {
            get { return _ports; }
            set
            {
                _ports = value;
                UpdateRueckmelderList();
            }
        }

        public RueckmeldeModul(int id, int ports)
        {
            _rueckmelder = new List<Rueckmelder>();

            Id = id;
            Ports = ports;
        }

        /// <summary>
        /// Passt die Rückmelderliste der Portanzahl an.
        /// </summary>
        private void UpdateRueckmelderList()
        {
            if (Rueckmelder.Count == Ports)
                return;

            if (Rueckmelder.Count < Ports)
            {
                for (var i = Rueckmelder.Count; i < Ports; i++)
                    _rueckmelder.Add(new Rueckmelder());
            }
            else
            {
                for (var i = Rueckmelder.Count - 1; i >= Ports; i--)
                {
                    _rueckmelder.RemoveAt(i);
                }
            }
        }
    }
}

using PropertyChanged;

namespace RailNet.Clients.Ecos.Extended.Rueckmeldung
{
    [ImplementPropertyChanged]
    public class Rueckmelder
    {
        public bool Belegt { get; set; }
    }
}

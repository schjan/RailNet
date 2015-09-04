using System;

namespace RailNet.Clients.Ecos.Basic
{
    public class StaticIds
    {
        public const int EcosId = 1;

        [Obsolete(ObsoleteMessage)]
        public const int ProgrammiergleisId = 5;

        public const int LokManagerId = 10;

        public const int SchaltartikelManagerId = 11;

        [Obsolete(ObsoleteMessage)]
        public const int PendelzugsteuerungId = 12;

        [Obsolete(ObsoleteMessage)]
        public const int DeviceManagerId = 20;

        public const int SnifferId = 25;

        public const int FeedbackManagerId = 26;

        [Obsolete(ObsoleteMessage)]
        public const int BoosterId = 27;

        [Obsolete(ObsoleteMessage)]
        public const int StellpultId = 31;
        
        private const string ObsoleteMessage = "Noch nicht weiter spezifiziert durch ESU.";
    }
}

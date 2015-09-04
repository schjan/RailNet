using System;

namespace RailNet.Clients.Ecos.Basic
{
    public class StaticIds
    {
        public const int EcoS = 1;

        [Obsolete(ObsoleteMessage)]
        public const int Programmiergleis = 5;

        public const int LokManager = 10;

        public const int SchaltartikelManager = 11;

        [Obsolete(ObsoleteMessage)]
        public const int Pendelzugsteuerung = 12;

        [Obsolete(ObsoleteMessage)]
        public const int DeviceManager = 20;

        public const int Sniffer = 25;

        public const int FeedbackManager = 26;

        [Obsolete(ObsoleteMessage)]
        public const int Booster = 27;

        [Obsolete(ObsoleteMessage)]
        public const int Stellpult = 31;
        
        private const string ObsoleteMessage = "Noch nicht weiter spezifiziert durch ESU.";
    }
}

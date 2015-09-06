﻿namespace RailNet.Clients.Ecos.Extended.Lok
{
    public class Lok
    {
        public Lok(int id)
        {
            Id = id;
        } 

        public int Id { get; }

        /// <summary>
        /// Anzeigename der Lok innerhalb der ECoS
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Aktuelle Geschwindigkeitsstufe
        /// </summary>
        public int Speed { get; set; }

        /// <summary>
        /// Anzahl der verfügbaren Geschwindigkeitsstufen.
        /// </summary>
        public int SpeedSteps { get; set; }

        public bool Direction { get; set; }
    }
}

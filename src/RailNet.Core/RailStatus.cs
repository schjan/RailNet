using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailNet.Core
{
    /// <summary>
    /// Beschreibt den Status der Modellbahn
    /// </summary>
    public enum RailStatus
    {
        /// <summary>
        /// Anlage befindet sich in einem undefinierten (uninitialisierten) zustand.
        /// </summary>
        Undefined,
        /// <summary>
        /// Nothalt
        /// </summary>
        Stop = 0,
        /// <summary>
        /// Go
        /// </summary>
        Go = 1
    }
}

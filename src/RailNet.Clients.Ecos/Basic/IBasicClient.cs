using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailNet.Clients.Ecos.Basic
{
    public interface IBasicClient
    {
        /// <summary>
        /// Liefert den Abschnitt einer Liste von Objekten die zum Objekt mit bestimmter ID gehoeren.
        /// </summary>
        /// <param name="id">ID des Objektes</param>
        /// <param name="min">Range von</param>
        /// <param name="max">Range bis</param>
        /// <param name="args">Parameter</param>
        /// <returns></returns>
        Task<BasicAntwort> QueryObjects(int id, int min = 0, int max = 0, params string[] args);

        /// <summary>
        /// Liefert Liste von Objekten die zum Objekt mit der ID gehoeren.
        /// </summary>
        /// <param name="id">ID des Objektes</param>
        /// <param name="args">Parameter</param>
        Task<BasicAntwort> QueryObjects(int id, params string[] args);

        /// <summary>
        /// Liefert die groesse der Liste der zu der ID gehoerigen Objekte.
        /// </summary>
        /// <param name="id">ID des Objektes</param>
        Task<BasicAntwort> QueryObjectsSize(int id);

        /// <summary>
        /// Setzt mehrere Eigenschaften eines Objektes
        /// </summary>
        /// <param name="id">ID des Objektes</param>
        /// <param name="args">Dictionary mit jeweils Parameter und Wert</param>
        Task<BasicAntwort> Set(int id, Dictionary<string, string> args);

        /// <summary>
        /// Setzt einzelne Eigenschaften eines Objektes
        /// </summary>
        /// <param name="id">ID des Objektes</param>
        /// <param name="param">Parameter</param>
        /// <param name="value">Wert</param>
        Task<BasicAntwort> Set(int id, string param, string value);

        /// <summary>
        /// Fragt Eigenschaften eines Objektes ab
        /// </summary>
        /// <param name="id">ID des Objektes</param>
        /// <param name="args">Eigenschaften</param>
        Task<BasicAntwort> Get(int id, params string[] args);

        /// <summary>
        /// Legt neues Objekt unterhalb eines Objektmanagers an
        /// </summary>
        /// <param name="id">ID des Objektmanagers</param>
        /// <param name="append">Gibt an ob Append-Befehl hinzugefuegt werden soll.</param>
        Task<BasicAntwort> Create(int id, bool append = false);

        /// <summary>
        /// Legt ein Objekt mit Eigenschaften unterhalb eines Objektmanagers an
        /// <seealso cref="BasicClient.Set"/>
        /// </summary>
        /// <param name="id">ID des Objektmanagers</param>
        /// <param name="args">Argumente wie bei Set</param>
        /// <param name="append">Gibt an ob Append-Befehl hinzugefuegt werden soll.</param>
        Task<BasicAntwort> Create(int id, Dictionary<string, string> args, bool append = false);

        /// <summary>
        /// Loescht Objekt anhand der ID
        /// </summary>
        /// <param name="id">ID des Objektes</param>
        Task<BasicAntwort> Delete(int id);

        /// <summary>
        /// Registriert sich bei einem Objekt als Viewer oder Controller und erhaelt dadurch Events des Objektes oder kann es steuern.
        /// </summary>
        /// <param name="id">ID des Objektes</param>
        /// <param name="param">view oder control</param>
        /// <param name="force">Uebernimmt zwingend Lok von anderem Teilnehmer</param>>
        Task<BasicAntwort> Request(int id, string param, bool force = false);

        /// <summary>
        /// Meldet sich als Viewer oder Controller vom Objekt ab.
        /// Alle anderen Viewer / Controller erhalten eine Nachricht.
        /// </summary>
        /// <param name="id">ID des Objektes</param>
        /// <param name="args">view und oder control</param>
        Task<BasicAntwort> Release(int id, params string[] args);
    }
}
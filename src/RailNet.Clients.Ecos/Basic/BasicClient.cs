using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RailNet.Clients.Ecos.Basic
{
    public class BasicClient : IBasicClient
    {
        private readonly INachrichtenDispo _dispo;

        public BasicClient(INachrichtenDispo dispo)
        {
            _dispo = dispo;
            _dispo.IncomingEvents.Subscribe(RaiseEventReceived);

            EventObservable = _dispo.IncomingEvents;
        }

        /// <summary>
        /// Liefert den Abschnitt einer Liste von Objekten die zum Objekt mit bestimmter ID gehoeren.
        /// </summary>
        /// <param name="id">ID des Objektes</param>
        /// <param name="min">Range von</param>
        /// <param name="max">Range bis</param>
        /// <param name="args">Parameter</param>
        /// <returns></returns>
        public Task<BasicResponse> QueryObjects(int id, int min = 0, int max = 0, params string[] args)
        {
            var b = new StringBuilder("queryObjects(");
            b.Append(id);
            foreach (var arg in args)
            {
                b.Append(", ");
                b.Append(arg);
            }

            if (max > 0 && max >= min)
            {
                b.Append($", nr[{min},{max}]");
            }

            b.Append(')');

            return _dispo.SendCommandAsync(b.ToString());
        }

        /// <summary>
        /// Liefert Liste von Objekten die zum Objekt mit der ID gehoeren.
        /// </summary>
        /// <param name="id">ID des Objektes</param>
        /// <param name="args">Parameter</param>
        public Task<BasicResponse> QueryObjects(int id, params string[] args)
        {
            return QueryObjects(id, 0, 0, args);
        }

        /// <summary>
        /// Liefert die groesse der Liste der zu der ID gehoerigen Objekte.
        /// </summary>
        /// <param name="id">ID des Objektes</param>
        public Task<BasicResponse> QueryObjectsSize(int id)
        {
            return _dispo.SendCommandAsync("queryObjects(" + id + ", size)");
        }

        /// <summary>
        /// Setzt mehrere Eigenschaften eines Objektes
        /// </summary>
        /// <param name="id">ID des Objektes</param>
        /// <param name="args">Dictionary mit jeweils Parameter und Wert</param>
        public async Task<BasicResponse> Set(int id, Dictionary<string, string> args)
        {
            if (args.Count == 0)
                throw new ArgumentNullException(nameof(args), "args duerfen nicht leer sein!");

            var str = new StringBuilder("set(");
            str.Append(id);

            foreach (var arg in args)
            {
                if (string.IsNullOrWhiteSpace(arg.Key) || string.IsNullOrWhiteSpace(arg.Value))
                    throw new ArgumentNullException(nameof(args), "Im Dictionary darf kein leerer String vorkommen!");

                str.Append(", ")
                    .Append(arg.Key)
                    .Append('[')
                    .Append(arg.Key == "name" ? $"\"{arg.Value}\"" : arg.Value)
                    .Append(']');
            }
            str.Append(')');

            return await _dispo.SendCommandAsync(str.ToString());
        }

        /// <summary>
        /// Setzt einzelne Eigenschaften eines Objektes
        /// </summary>
        /// <param name="id">ID des Objektes</param>
        /// <param name="param">Parameter</param>
        /// <param name="value">Wert</param>
        public Task<BasicResponse> Set(int id, string param, string value)
        {
            return Set(id, new Dictionary<string, string> {{param, value}});
        }

        /// <summary>
        /// Setzt einzelne Eigenschaften eines Objektes
        /// </summary>
        /// <param name="id">ID des Objektes</param>
        /// <param name="param">Parameter</param>
        public Task<BasicResponse> Set(int id, string param)
        {
            if (string.IsNullOrWhiteSpace(param))
                throw new ArgumentNullException(nameof(param), "args duerfen nicht leer sein!");

            var b = new StringBuilder("set(");
            b.Append(id)
                .Append(", ")
                .Append(param)
                .Append(')');

            return _dispo.SendCommandAsync(b.ToString());
        }

        /// <summary>
        /// Fragt Eigenschaften eines Objektes ab
        /// </summary>
        /// <param name="id">ID des Objektes</param>
        /// <param name="args">Eigenschaften</param>
        public Task<BasicResponse> Get(int id, params string[] args)
        {
            if (args.Length == 0)
                throw new ArgumentNullException(nameof(args), "args duerfen nicht leer sein!");

            var b = new StringBuilder("get(");
            b.Append(id);
            foreach (var arg in args)
            {
                b.Append(", ")
                    .Append(arg);
            }
            b.Append(')');

            return _dispo.SendCommandAsync(b.ToString());
        }

        /// <summary>
        /// Legt neues Objekt unterhalb eines Objektmanagers an
        /// </summary>
        /// <param name="id">ID des Objektmanagers</param>
        /// <param name="append">Gibt an ob Append-Befehl hinzugefuegt werden soll.</param>
        public Task<BasicResponse> Create(int id, bool append = false)
        {
            return Create(id, null, append);
        }

        /// <summary>
        /// Legt ein Objekt mit Eigenschaften unterhalb eines Objektmanagers an
        /// </summary>
        /// <param name="id">ID des Objektmanagers</param>
        /// <param name="args">Argumente wie bei Set</param>
        /// <param name="append">Gibt an ob Append-Befehl hinzugefuegt werden soll.</param>
        public Task<BasicResponse> Create(int id, Dictionary<string, string> args, bool append = false)
        {
            var b = new StringBuilder("create(");
            b.Append(id);

            if (args != null)
                foreach (var arg in args)
                {
                    if (string.IsNullOrWhiteSpace(arg.Key) || string.IsNullOrWhiteSpace(arg.Value))
                        throw new ArgumentNullException(nameof(args), "Im Dictionary darf kein leerer String vorkommen!");

                    b.Append(", ")
                        .Append(arg.Key)
                        .Append('[')
                        .Append(arg.Key == "name" ? $"\"{arg.Value}\"" : arg.Value)
                        .Append(']');
                }

            if (append)
                b.Append(", append)");
            else
                b.Append(')');

            return _dispo.SendCommandAsync(b.ToString());
        }

        /// <summary>
        /// Loescht Objekt anhand der ID
        /// </summary>
        /// <param name="id">ID des Objektes</param>
        public Task<BasicResponse> Delete(int id)
        {
            return _dispo.SendCommandAsync($"delete({id})");
        }

        /// <summary>
        /// Registriert sich bei einem Objekt als Viewer oder Controller und erhaelt dadurch Events des Objektes oder kann es steuern.
        /// </summary>
        /// <param name="id">ID des Objektes</param>
        /// <param name="param">view oder control</param>
        /// <param name="force">Uebernimmt zwingend Lok von anderem Teilnehmer</param>>
        public Task<BasicResponse> Request(int id, string param, bool force = false)
        {
            if (string.IsNullOrWhiteSpace(param))
                throw new ArgumentNullException(nameof(param), "param darf nicht null sein!");

            string query = $"request({id}, {param}";

            query += force ? ", force)" : ")";

            return _dispo.SendCommandAsync(query);
        }

        /// <summary>
        /// Meldet sich als Viewer oder Controller vom Objekt ab.
        /// Alle anderen Viewer / Controller erhalten eine Nachricht.
        /// </summary>
        /// <param name="id">ID des Objektes</param>
        /// <param name="args">view und oder control</param>
        public Task<BasicResponse> Release(int id, params string[] args)
        {
            if (args.Length == 0)
                throw new ArgumentNullException(nameof(args), "args duerfen nicht leer sein!");

            var b = new StringBuilder("release(");
            b.Append(id);
            foreach (var arg in args)
            {
                b.Append(", ");
                b.Append(arg);
            }
            b.Append(')');

            return _dispo.SendCommandAsync(b.ToString());
        }


        /// <summary>
        /// EventReceivedEvents wird ausgeführt, wenn ein Serverseitiges Event empfangen wurde.
        /// </summary>
        public event EventReceivedHandler EventReceived;
        
        public IObservable<BasicEvent> EventObservable { get; }

        /// <summary>
        /// Raises a BasicEvent
        /// </summary>
        protected void RaiseEventReceived(BasicEvent response)
        {
            EventReceived?.Invoke(this, new EventReceivedArgs(response));
        }
    }
}
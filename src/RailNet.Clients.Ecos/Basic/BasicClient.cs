using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailNet.Clients.Ecos.Basic
{
    public class BasicClient : IBasicClient
    {
        private INachrichtenDispo _dispo;

        public BasicClient()
        {
            
        }

        public BasicClient(INachrichtenDispo dispo)
        {
            _dispo = dispo;
        }

        public async Task<BasicAntwort> QueryObjects(int id, params string[] args)
        {
            var b = new StringBuilder("queryObjects(");
            b.Append(id);
            foreach (var arg in args)
            {
                b.Append(", ");
                b.Append(arg);
            }
            b.Append(')');

            return await _dispo.SendeBefehlAsync(b.ToString());
        }

        public async Task<BasicAntwort> Set(int id, string param, string value)
        {
            if (string.IsNullOrWhiteSpace(param))
                throw new ArgumentNullException("param", "param darf nicht null sein!");
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException("value", "value darf nicht null sein!");

            var query = string.Format("set({0}, {1}[{2}])", id, param, value);

            return await _dispo.SendeBefehlAsync(query);
        }

        public async Task<BasicAntwort> Get(int id, params string[] args)
        {
            if(args.Length == 0)
                throw new ArgumentNullException("args", "args duerfen nicht leer sein!");

            var b = new StringBuilder("get(");
            b.Append(id);
            foreach (var arg in args)
            {
                b.Append(", ");
                b.Append(arg);
            }
            b.Append(')');

            return await _dispo.SendeBefehlAsync(b.ToString());
        }

        public async Task<BasicAntwort> Create(int id)
        {
            return await _dispo.SendeBefehlAsync("create(" + id + ")");
        }

        public async Task<BasicAntwort> Create(int id, Dictionary<string, string> args)
        {
            var b = new StringBuilder("create(");
            b.Append(id);

            foreach (var arg in args)
            {
                b.Append(", ");
                b.Append(arg.Key);
                b.Append('[');
                b.Append(arg.Key == "name" ? string.Format("\"{0}\"", arg.Value) : arg.Value);
                b.Append(']');
            }
            b.Append(')');

            return await _dispo.SendeBefehlAsync(b.ToString());
        }

        public async Task<BasicAntwort> Create(int id, string param)
        {
            if (string.IsNullOrWhiteSpace(param))
                throw new ArgumentNullException("param", "param darf nicht null sein!");

            string query = string.Format("create({0}, {1})", id, param);

            return await _dispo.SendeBefehlAsync(query);
        }

        public async Task<BasicAntwort> Delete(int id)
        {
            return await _dispo.SendeBefehlAsync(string.Format("delete({0})", id));
        }

        public async Task<BasicAntwort> Request(int id, string param, bool force = false)
        {
            if (string.IsNullOrWhiteSpace(param))
                throw new ArgumentNullException("param", "param darf nicht null sein!");

            string query = string.Format("request({0}, {1}", id, param);

            query += force ? ", force)" : ")";

            return await _dispo.SendeBefehlAsync(query);
        }

        public async Task<BasicAntwort> Release(int id, params string[] args)
        {
            if (args.Length == 0)
                throw new ArgumentNullException("args", "args duerfen nicht leer sein!");

            var b = new StringBuilder("release(");
            b.Append(id);
            foreach (var arg in args)
            {
                b.Append(", ");
                b.Append(arg);
            }
            b.Append(')');

            return await _dispo.SendeBefehlAsync(b.ToString());
        }
    }
}

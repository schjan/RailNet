﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RailNet.Clients.Ecos.Basic;

namespace RailNet.Clients.Ecos.Extended.Lok
{
    public class LokManager
    {
        private readonly IBasicClient _basicClient;
        internal readonly Dictionary<int, Lok> Loks; 

        public LokManager(IBasicClient basicClient)
        {
            _basicClient = basicClient;
            Loks = new Dictionary<int, Lok>();
        }

        public async Task<bool> QueryAll()
        {
            var result = await _basicClient.QueryObjects(10, "name", "addr", "protocol");

            if (result.HasError)
                return false;

            foreach (var lokObject in result.Content)
            {
                var id = Convert.ToInt32(lokObject.Split(' ').First());
                var name = BasicParser.TryGetParameterFromContent("name", lokObject, true);
                var protocol = BasicParser.TryGetParameterFromContent("protocol", lokObject);
                var adress = BasicParser.TryGetParameterFromContent("addr", lokObject);

                if (Loks.ContainsKey(id))
                    continue;

                Loks.Add(id, new Lok {});
            }

            return true;
        }

        public async Task<bool> RequestChanges()
        {
            var result = await _basicClient.Request(10, BefehlStrings.ViewS);
            return result.HasError;
        }
    }
}
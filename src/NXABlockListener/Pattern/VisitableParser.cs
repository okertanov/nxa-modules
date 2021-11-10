using Neo;
using Neo.IO.Json;
using Nxa.Plugins.Pattern.Visitables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nxa.Plugins.Pattern
{

    public class JsonVisitableParser
    {
        private IEnumerable<Type> types;
        private ProtocolSettings settings;
        private JObject searchJson;
        public JsonVisitableParser(ProtocolSettings settings, JObject searchJson = null)
        {
            this.settings = settings;
            this.searchJson = searchJson;

            var type = typeof(IVisitable);
            var assembly = type.Assembly;
            types = assembly.GetTypes().Where(x => !x.IsInterface && type.IsAssignableFrom(x));
        }

        public IEnumerable<IVisitable> Parse(JObject obj, ProtocolSettings protocolSettings = null)
        {
            foreach (var type in types)
            {
                IVisitable instance = (IVisitable)Activator.CreateInstance(type);
                if (instance.Parse(obj, protocolSettings == null ? this.settings : protocolSettings, searchJson))
                {
                    yield return instance;
                }
            }
        }

    }
}

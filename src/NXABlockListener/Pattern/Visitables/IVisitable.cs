using Neo;
using Neo.IO.Json;
using Nxa.Plugins.Pattern.Visitors;
using System.Threading;

namespace Nxa.Plugins.Pattern.Visitables
{
    public interface IVisitable
    {
        /// <summary>
        /// Accepts visitor 
        /// </summary>
        /// <param name="visitor">visitor class</param>
        /// <param name="cancellationToken">task cancelation token</param>
        void Accept(IVisitor visitor, CancellationToken cancellationToken);
        
        /// <summary>
        /// Try to parse object to this class
        /// </summary>
        /// <param name="jsonObj">json object</param>
        /// <param name="protocolSettings">neo system protocol settings</param>
        /// <param name="searchJson">search parameters as json object (in case of search task)</param>
        /// <returns></returns>
        bool Parse(JObject jsonObj, ProtocolSettings protocolSettings, JObject searchJson = null);
        
        /// <summary>
        /// Preform serch on successfully parsed json object
        /// </summary>
        /// <param name="jsonObj">json object</param>
        /// <param name="searchType">object type</param>
        /// <param name="searchJson">search parameters as json object</param>
        void Search(JObject jsonObj, string searchType, JObject searchJson = null);
    }

}

using Nxa.Plugins.Pattern.Visitables;
using System.Threading;

namespace Nxa.Plugins.Pattern.Visitors
{
    public interface IVisitor
    {
        /// <summary>
        /// Visit VisitableBlock class
        /// </summary>
        /// <param name="obj">VisitableBlock object</param>
        /// <param name="cancellationToken">cancelation token</param>
        /// <returns></returns>
        bool Visit(VisitableBlock obj, CancellationToken cancellationToken);

        /// <summary>
        /// Visit VisitableTransaction class
        /// </summary>
        /// <param name="obj">VisitableTransaction object</param>
        /// <param name="cancellationToken">cancelation token</param>
        /// <returns></returns>
        bool Visit(VisitableTransaction obj, CancellationToken cancellationToken);

        /// <summary>
        /// Visit VisitableTransfer class
        /// </summary>
        /// <param name="obj">VisitableTransfer object</param>
        /// <param name="cancellationToken">cancelation token</param>
        /// <returns></returns>
        bool Visit(VisitableTransfer obj, CancellationToken cancellationToken);

        /// <summary>
        /// Visit VisitableSCDeployment class
        /// </summary>
        /// <param name="obj">VisitableSCDeployment object</param>
        /// <param name="cancellationToken">cancelation token</param>
        /// <returns></returns>
        bool Visit(VisitableSCDeployment obj, CancellationToken cancellationToken);
    }
}

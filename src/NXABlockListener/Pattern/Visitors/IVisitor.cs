using Nxa.Plugins.Pattern.Visitables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nxa.Plugins.Pattern.Visitors
{
    public interface IVisitor
    {
        bool Visit(VisitableBlock obj, CancellationToken cancellationToken);
        bool Visit(VisitableTransaction obj, CancellationToken cancellationToken);
        bool Visit(VisitableTransfer obj, CancellationToken cancellationToken);
        bool Visit(VisitableSCDeployment obj, CancellationToken cancellationToken);
    }
}

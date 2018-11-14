using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;

namespace Diffused.Core
{

    public class Bootstrap {}

    public class Node
    {
        private readonly ILogger<NodeHostedService> logger;

        internal ActionBlock<Bootstrap> BootstrapBlock; 

        public Node(ILogger<NodeHostedService> logger)
        {
            this.logger = logger;
            BootstrapBlock= new ActionBlock<Bootstrap>(
                b=> logger.LogInformation("Bootstrap")
                );


        }


        public void Init()
        {

     

        }





    }
}

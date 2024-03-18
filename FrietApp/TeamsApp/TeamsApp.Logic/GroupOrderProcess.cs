using Entities;
using Shared.Models;
using TeamsApp.Logic.Abstractions;

namespace TeamsApp.Logic;

public class GroupOrderProcess : IGroupOrderProcess
{
    public GroupOrder StartGroupOrder(Menu menu)
    {
        // Start group order
        var groupOrder = new GroupOrder();
        
        // open threads
        // listen for orders
        var userOrder = ThreadPool.QueueUserWorkItem(StartUserOrder);
        
        // add each order to group order
        // add possibility to remove orders
        // add possibility to edit orders;
        
        // when admin ends group order
        
        
        //return groupOrder;
        
        throw new NotImplementedException();
    }
}


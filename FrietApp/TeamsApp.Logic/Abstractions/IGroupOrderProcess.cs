using Entities;

namespace TeamsApp.Logic.Abstractions;

public interface IGroupOrderProcess
{
    GroupOrder StartGroupOrder(Menu menu);
}
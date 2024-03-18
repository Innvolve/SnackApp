using Entities;
using Shared.Models;

namespace TeamsApp.Logic.Abstractions;

public interface IGroupOrderProcess
{
    GroupOrder StartGroupOrder(Menu menu);
}
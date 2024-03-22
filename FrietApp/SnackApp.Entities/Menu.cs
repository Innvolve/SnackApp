using SnackApp.Models;

namespace Entities;

public struct Menu
{
    public List<Item> Items { get; init; }
    public DateTime Date { get; } = DateTime.UtcNow;

    public Menu(List<Item> items)
    {
        Items = items;
    }
}
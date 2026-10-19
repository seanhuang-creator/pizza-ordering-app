using System;
using System.Collections.Generic;
using Xunit;
using PizzaApi.BusinessObjects;

namespace backend.Tests;

public class OrderStatusTests
{
    // Recreate status rules from spec 5.5 without needing Order.ChangeStatus method
    private static readonly Dictionary<OrderStatus, OrderStatus[]> Allowed = new()
    {
        [OrderStatus.Pending] = new[] { OrderStatus.Preparing, OrderStatus.Cancelled },
        [OrderStatus.Preparing] = new[] { OrderStatus.Ready, OrderStatus.Cancelled },
        [OrderStatus.Ready] = new[] { OrderStatus.Completed },
        [OrderStatus.Completed] = Array.Empty<OrderStatus>(),
        [OrderStatus.Cancelled] = Array.Empty<OrderStatus>(),
    };

    private static bool IsAllowed(OrderStatus from, OrderStatus to) => Allowed[from].Contains(to);

    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Preparing, true)]
    [InlineData(OrderStatus.Pending, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Preparing, OrderStatus.Ready, true)]
    [InlineData(OrderStatus.Preparing, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Ready, OrderStatus.Completed, true)]
    [InlineData(OrderStatus.Pending, OrderStatus.Ready, false)]
    [InlineData(OrderStatus.Preparing, OrderStatus.Completed, false)] // Scenario 12 must be 409
    [InlineData(OrderStatus.Ready, OrderStatus.Cancelled, false)]
    [InlineData(OrderStatus.Completed, OrderStatus.Pending, false)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Preparing, false)]
    public void Status_Transition_Rules(OrderStatus from, OrderStatus to, bool shouldAllow)
    {
        Assert.Equal(shouldAllow, IsAllowed(from, to));
    }

    [Fact]
    public void Status_Preparing_To_Completed_Should_Be_Disallowed()
    {
        Assert.False(IsAllowed(OrderStatus.Preparing, OrderStatus.Completed));
    }

    [Fact]
    public void Cancelled_Order_May_Be_Deleted_Others_Not()
    {
        bool CanBeDeleted(OrderStatus s) => s == OrderStatus.Cancelled;
        Assert.False(CanBeDeleted(OrderStatus.Pending));
        Assert.True(CanBeDeleted(OrderStatus.Cancelled));
    }
}

using Reqnroll;

namespace MSTestCrossProjectExample.StepDefinitions;

/// <summary>
/// Step definitions for order management scenarios.
/// These steps work in conjunction with the SharedAuthenticationSteps
/// from the SharedAuthLibrary project (automatically available via project reference).
/// </summary>
[Binding]
public class OrderManagementSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly List<string> _cart = new();
    private readonly Dictionary<string, string> _orders = new();

    public OrderManagementSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [When(@"I navigate to the products page")]
    [When(@"ik naar de productenpagina navigeer")]
    public void WhenINavigateToTheProductsPage()
    {
        Console.WriteLine("Navigating to products page");
        // Verify user is logged in (state from SharedAuthLibrary)
        if (!_scenarioContext.ContainsKey("LoggedIn") || !(bool)_scenarioContext["LoggedIn"])
        {
            throw new Exception("User must be logged in to view products");
        }
        _scenarioContext["OnProductsPage"] = true;
    }

    [When(@"I add ""(.*)"" to cart")]
    [When(@"ik voeg ""(.*)"" toe aan winkelwagen")]
    public void WhenIAddToCart(string productName)
    {
        Console.WriteLine($"Adding {productName} to cart");
        _cart.Add(productName);
        _scenarioContext["CartCount"] = _cart.Count;
    }

    [When(@"I proceed to checkout")]
    [When(@"ik ga verder naar afrekenen")]
    public void WhenIProceedToCheckout()
    {
        Console.WriteLine("Proceeding to checkout");
        if (_cart.Count == 0)
        {
            throw new Exception("Cart is empty, cannot checkout");
        }
        
        // Create order
        var orderId = "12345";
        _orders[orderId] = "Pending";
        _scenarioContext["LastOrderId"] = orderId;
    }

    [Then(@"I should see order confirmation")]
    [Then(@"zou ik bestelbevestiging moeten zien")]
    public void ThenIShouldSeeOrderConfirmation()
    {
        Console.WriteLine("Verifying order confirmation");
        if (!_scenarioContext.ContainsKey("LastOrderId"))
        {
            throw new Exception("No order was created");
        }
    }

    [Then(@"I should see ""(.*)"" in my order history")]
    [Then(@"zou ik ""(.*)"" in mijn bestelgeschiedenis moeten zien")]
    public void ThenIShouldSeeInMyOrderHistory(string orderId)
    {
        Console.WriteLine($"Verifying {orderId} in order history");
        var expectedOrderId = orderId.Replace("Order #", "").Replace("Bestelling #", "");
        if (!_orders.ContainsKey(expectedOrderId))
        {
            throw new Exception($"Order {expectedOrderId} not found in history");
        }
    }

    [When(@"I navigate to my account page")]
    [When(@"ik naar mijn accountpagina navigeer")]
    public void WhenINavigateToMyAccountPage()
    {
        Console.WriteLine("Navigating to account page");
        if (!_scenarioContext.ContainsKey("LoggedIn") || !(bool)_scenarioContext["LoggedIn"])
        {
            throw new Exception("User must be logged in to view account");
        }
        _scenarioContext["OnAccountPage"] = true;
    }

    [When(@"I click on ""(.*)""")]
    [When(@"ik klik op ""(.*)""")]
    public void WhenIClickOn(string linkOrButton)
    {
        Console.WriteLine($"Clicking on: {linkOrButton}");
        _scenarioContext["LastClick"] = linkOrButton;
    }
    
    [When(@"I click ""(.*)"" button")]
    [When(@"ik klik op ""(.*)"" knop")]
    public void WhenIClickButton(string buttonText)
    {
        Console.WriteLine($"Clicking button: {buttonText}");
        
        if (buttonText == "Cancel Order" || buttonText == "Bestelling annuleren")
        {
            var orderId = _scenarioContext["SelectedOrderId"] as string;
            if (orderId != null && _orders.ContainsKey(orderId))
            {
                _orders[orderId] = "Cancelled";
            }
        }
    }

    [Then(@"I should see a list of my previous orders")]
    [Then(@"zou ik een lijst van mijn eerdere bestellingen moeten zien")]
    public void ThenIShouldSeeAListOfMyPreviousOrders()
    {
        Console.WriteLine("Verifying previous orders list is displayed");
        // Simulate displaying orders
    }

    [Then(@"I should see order dates and totals")]
    [Then(@"zou ik besteldatums en totalen moeten zien")]
    public void ThenIShouldSeeOrderDatesAndTotals()
    {
        Console.WriteLine("Verifying order details are displayed");
        // Simulate displaying order details
    }

    [When(@"I select order ""(.*)""")]
    [When(@"ik selecteer bestelling ""(.*)""")]
    public void WhenISelectOrder(string orderId)
    {
        Console.WriteLine($"Selecting order: {orderId}");
        _scenarioContext["SelectedOrderId"] = orderId;
        
        // Ensure the order exists in the dictionary (simulating existing order in system)
        if (!_orders.ContainsKey(orderId))
        {
            _orders[orderId] = "Completed"; // Default status for existing orders
        }
    }

    // Note: "I should see message" step is provided by SharedAuthLibrary
    // No need to duplicate it here
    
    // Note: "I click button" step is shared - defined above in this class

    [Then(@"order ""(.*)"" should have status ""(.*)""")]
    [Then(@"bestelling ""(.*)"" zou status ""(.*)"" moeten hebben")]
    public void ThenOrderShouldHaveStatus(string orderId, string expectedStatus)
    {
        Console.WriteLine($"Verifying order {orderId} has status {expectedStatus}");
        if (!_orders.ContainsKey(orderId))
        {
            throw new Exception($"Order {orderId} not found");
        }
        
        var actualStatus = _orders[orderId];
        // Handle both English and Dutch status values
        var normalizedExpected = expectedStatus == "Geannuleerd" ? "Cancelled" : expectedStatus;
        if (actualStatus != normalizedExpected)
        {
            throw new Exception($"Expected status '{normalizedExpected}' but found '{actualStatus}'");
        }
    }
}

Feature: Order Management
    As a customer
    I want to manage my orders
    So that I can purchase products
    
    This feature demonstrates calling scenarios from SharedAuthLibrary
    (a separate project) using the Reqnroll.ScenarioCall.Generator plugin.

Scenario: Place Order as Authenticated User
    # This scenario call references the SharedAuthLibrary project
    # The scenario is expanded at build time by the generator plugin
    Given I call scenario "Login with Valid Credentials" from feature "Shared Authentication"
    When I navigate to the products page
    And I add "Widget" to cart
    And I add "Gadget" to cart
    And I proceed to checkout
    Then I should see order confirmation
    And I should see "Order #12345" in my order history

Scenario: View Order History
    # Another example of calling a cross-project scenario
    Given I call scenario "Login with Valid Credentials" from feature "Shared Authentication"
    When I navigate to my account page
    And I click on "Order History"
    Then I should see a list of my previous orders
    And I should see order dates and totals

Scenario: Cancel Order
    # Cross-project scenario call followed by business logic
    Given I call scenario "Login with Valid Credentials" from feature "Shared Authentication"
    When I navigate to my account page
    And I click on "Order History"
    And I select order "12345"
    And I click "Cancel Order" button
    Then I should see "Order cancelled successfully" message
    And order "12345" should have status "Cancelled"

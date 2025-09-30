Feature: Scenario Calling
    As a test developer
    I want to call scenarios from other features
    So that I can reuse test logic

Scenario: Call login scenario from authentication feature
    Given I call scenario "Login with valid credentials" from feature "Authentication"
    Then the user should be authenticated

Scenario: Use scenario call to setup test data
    Given I call scenario "Login with valid credentials" from feature "Authentication"
    When I navigate to the dashboard
    Then I should see the welcome message

Scenario: Multiple scenario calls in one test
    Given I call scenario "Login with valid credentials" from feature "Authentication"
    When I perform some action
    And I call scenario "Login with invalid credentials" from feature "Authentication"
    Then I should see both login attempts were recorded

Feature: Same Feature Scenario Calls
    As a test developer
    I want to call scenarios within the same feature file
    So that I can reuse common steps without creating separate feature files

Scenario: Common Setup Steps
    Given the system is initialized
    And the database is clean
    And test data is loaded

Scenario: Login Steps
    Given I am on the login page
    When I enter username "testuser"
    And I enter password "password123"
    And I click the login button
    Then I should be logged in

Scenario: Test With Same Feature Setup
    Given I call scenario "Common Setup Steps" from feature "Same Feature Scenario Calls"
    When I call scenario "Login Steps" from feature "Same Feature Scenario Calls"
    Then I should be on the dashboard
    And I should see my user profile

Scenario: Another Test With Shared Setup
    Given I call scenario "Common Setup Steps" from feature "Same Feature Scenario Calls"
    When I perform some action
    Then I should see expected results

Scenario: Test Demonstrating Multiple Calls
    Given I call scenario "Common Setup Steps" from feature "Same Feature Scenario Calls"
    And I configure additional settings
    When I call scenario "Login Steps" from feature "Same Feature Scenario Calls"
    And I navigate to reports
    Then I should see the reports page

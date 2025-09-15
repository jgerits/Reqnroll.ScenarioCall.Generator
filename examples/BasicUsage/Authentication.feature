Feature: Authentication
    As a user
    I want to authenticate with the system
    So that I can access protected resources

Scenario: Login with Valid Credentials
    Given I am on the login page
    When I enter username "john.doe@example.com"
    And I enter password "SecurePassword123"
    And I click the login button
    Then I should be redirected to the dashboard
    And I should see "Welcome, John Doe" message

Scenario: Logout
    Given I am logged into the system
    When I click the logout button
    Then I should be redirected to the login page
    And I should see "You have been logged out" message

Scenario: Login with Invalid Credentials
    Given I am on the login page
    When I enter username "invalid@example.com"
    And I enter password "WrongPassword"
    And I click the login button
    Then I should see "Invalid credentials" error message
    And I should remain on the login page
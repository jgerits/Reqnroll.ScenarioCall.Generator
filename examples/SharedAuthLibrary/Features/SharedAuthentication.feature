Feature: Shared Authentication
    This feature contains reusable authentication scenarios
    that can be called from other projects within the solution.
    
    These scenarios demonstrate cross-project scenario reuse,
    allowing teams to maintain authentication logic in a shared library.

Scenario: Login with Valid Credentials
    Given I am on the login page
    When I enter username "john.doe@example.com"
    And I enter password "SecurePassword123"
    And I click the login button
    Then I should be redirected to the dashboard
    And I should see "Welcome, John Doe" message

Scenario: Login with Invalid Credentials
    Given I am on the login page
    When I enter username "invalid@example.com"
    And I enter password "WrongPassword"
    And I click the login button
    Then I should see "Invalid credentials" error message
    And I should remain on the login page

Scenario: Logout
    Given I am logged into the system
    When I click the logout button
    Then I should be redirected to the login page
    And I should see "You have been logged out" message

Scenario: Password Reset Request
    Given I am on the login page
    When I click the "Forgot Password" link
    And I enter email "john.doe@example.com"
    And I click the "Request Reset" button
    Then I should see "Password reset email sent" message

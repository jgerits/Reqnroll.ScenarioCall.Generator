Feature: Background Scenario Call
    Testing that scenario calls include background steps from the target feature

Scenario: Call scenario from feature with background
    Given I call scenario "Login with valid credentials" from feature "Authentication with Background" with background
    Then I should see the success message

Scenario: Multiple calls to scenarios with backgrounds
    Given I call scenario "Create User Account" from feature "User Setup" with background
    When I call scenario "Verify User Exists" from feature "User Setup" with background
    Then the verification should succeed

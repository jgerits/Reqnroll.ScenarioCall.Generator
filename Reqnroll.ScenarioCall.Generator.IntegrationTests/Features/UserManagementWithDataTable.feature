Feature: User Management with DataTable
    As an administrator
    I want to manage users by calling reusable scenarios
    So that I can maintain consistent user management operations

Scenario: Setup user with authentication
    Given I call scenario "Create user with data table" from feature "User Data"
    When I verify the user was created
    Then the user should be in the system

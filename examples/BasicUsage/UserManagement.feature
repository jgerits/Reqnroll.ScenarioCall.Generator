Feature: User Management
    As an administrator
    I want to manage user accounts
    So that I can control system access

Scenario: Create New User Account
    Given I call scenario "Login with Valid Credentials" from feature "Authentication"
    When I navigate to the user management section
    And I click "Add New User" button
    And I fill in the new user form:
        | Field    | Value                |
        | Username | jane.smith           |
        | Email    | jane.smith@test.com  |
        | Role     | Standard User        |
    And I click "Create User" button
    Then I should see "User created successfully" message
    And I should see "jane.smith" in the user list
    And I call scenario "Logout" from feature "Authentication"

Scenario: Delete User Account
    Given I call scenario "Login with Valid Credentials" from feature "Authentication"
    When I navigate to the user management section
    And I select user "jane.smith" from the list
    And I click "Delete User" button
    And I confirm the deletion
    Then I should see "User deleted successfully" message
    And I should not see "jane.smith" in the user list
    And I call scenario "Logout" from feature "Authentication"

Scenario: Update User Permissions
    Given I call scenario "Login with Valid Credentials" from feature "Authentication"
    When I navigate to the user management section
    And I select user "jane.smith" from the list
    And I click "Edit Permissions" button
    And I change the role to "Administrator"
    And I click "Save Changes" button
    Then I should see "Permissions updated successfully" message
    And I should see "Administrator" role for "jane.smith"
    And I call scenario "Logout" from feature "Authentication"
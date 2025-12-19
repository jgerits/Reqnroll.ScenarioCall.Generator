Feature: User Setup
    User management with background setup

Background:
    Given the user management system is initialized
    And the storage is ready

Scenario: Create User Account
    Given I create a user account with username "newuser"
    When the account is saved
    Then the account should exist

Scenario: Verify User Exists
    Given a user account exists with username "testuser"
    When I check if the user exists
    Then the user should be found

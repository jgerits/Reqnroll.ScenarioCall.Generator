Feature: Authentication
    As a user
    I want to be able to authenticate
    So that I can access the system

Scenario: Login with valid credentials
    Given there is a user with username "testuser" and password "password123"
    When the user attempts to log in with username "testuser" and password "password123"
    Then the login should be successful
    And the user should be authenticated

Scenario: Login with invalid credentials
    Given there is a user with username "testuser" and password "password123"
    When the user attempts to log in with username "testuser" and password "wrongpassword"
    Then the login should fail
    And the user should not be authenticated

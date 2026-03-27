Feature: Authentication with Background
    Testing authentication with background setup

Background:
    Given the authentication system is initialized
    And the user database is ready

Scenario: Login with valid credentials
    Given there is a user with username "testuser" and password "password123"
    When the user attempts to log in with username "testuser" and password "password123"
    Then the login should be successful

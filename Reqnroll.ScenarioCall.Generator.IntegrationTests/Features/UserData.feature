Feature: User Data
    As a system
    I want to manage user data
    So that I can store and retrieve user information

Scenario: Create user with data table
    Given I have the following user data:
        | Field    | Value                |
        | Username | john.doe             |
        | Email    | john.doe@example.com |
        | Role     | Admin                |
    When I create the user with the data
    Then the user should exist with the provided data

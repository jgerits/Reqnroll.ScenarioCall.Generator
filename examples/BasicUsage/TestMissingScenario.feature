Feature: Test Missing Scenario Error Messages
    As a test developer
    I want to see helpful error messages when I call non-existent scenarios
    So that I can quickly fix my test calls

Scenario: Call non-existent scenario from existing feature
    Given I call scenario "NonExistentScenario" from feature "Authentication"
    Then this should show an error message

Scenario: Call scenario from non-existent feature
    Given I call scenario "AnyScenario" from feature "NonExistentFeature"
    Then this should show an error message


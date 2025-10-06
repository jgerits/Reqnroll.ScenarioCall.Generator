Feature: Product Reviews
    As a customer
    I want to write and read product reviews
    So that I can share my experience with other customers

Scenario: Submit Product Review
    # Reuse the login scenario from the shared authentication library
    Given I call scenario "Login with Valid Credentials" from feature "Shared Authentication"
    When I navigate to product "Widget"
    And I click "Write a Review" button
    And I enter review title "Great Product!"
    And I enter review text "This widget works perfectly and exceeded my expectations."
    And I select rating "5" stars
    And I submit the review
    Then I should see "Thank you for your review" message
    And my review should appear on the product page

Scenario: Edit My Review
    # Another example showing cross-project scenario reuse
    Given I call scenario "Login with Valid Credentials" from feature "Shared Authentication"
    When I navigate to my account page
    And I click on "My Reviews"
    And I select review for product "Widget"
    And I click "Edit Review" button
    And I change review text to "Updated: Still loving this widget after 6 months!"
    And I save the review
    Then I should see "Review updated successfully" message

Scenario: Report Inappropriate Review
    # Using the login scenario and then reporting functionality
    Given I call scenario "Login with Valid Credentials" from feature "Shared Authentication"
    When I navigate to product "Gadget"
    And I see a review with inappropriate content
    And I click "Report Review" link
    And I select reason "Spam or inappropriate content"
    And I submit the report
    Then I should see "Report submitted" message
    And the review should be flagged for moderation

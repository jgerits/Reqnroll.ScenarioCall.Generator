using Reqnroll;

namespace MSTestCrossProjectExample.StepDefinitions;

/// <summary>
/// Step definitions for product review scenarios.
/// Note: Authentication steps are provided by SharedAuthLibrary
/// and are automatically available through the project reference.
/// </summary>
[Binding]
public class ProductReviewSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly Dictionary<string, List<string>> _productReviews = new();

    public ProductReviewSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [When(@"I navigate to product ""(.*)""")]
    public void WhenINavigateToProduct(string productName)
    {
        Console.WriteLine($"Navigating to product: {productName}");
        // Verify user is logged in (state from SharedAuthLibrary)
        if (!_scenarioContext.ContainsKey("LoggedIn") || !(bool)_scenarioContext["LoggedIn"])
        {
            throw new Exception("User must be logged in to view products");
        }
        _scenarioContext["CurrentProduct"] = productName;
    }

    [When(@"I click ""(.*)"" button")]
    public void WhenIClickButton(string buttonText)
    {
        Console.WriteLine($"Clicking button: {buttonText}");
        _scenarioContext["LastButtonClicked"] = buttonText;
    }

    [When(@"I enter review title ""(.*)""")]
    public void WhenIEnterReviewTitle(string title)
    {
        Console.WriteLine($"Entering review title: {title}");
        _scenarioContext["ReviewTitle"] = title;
    }

    [When(@"I enter review text ""(.*)""")]
    public void WhenIEnterReviewText(string reviewText)
    {
        Console.WriteLine($"Entering review text: {reviewText}");
        _scenarioContext["ReviewText"] = reviewText;
    }

    [When(@"I select rating ""(.*)"" stars")]
    public void WhenISelectRatingStars(int rating)
    {
        Console.WriteLine($"Selecting rating: {rating} stars");
        _scenarioContext["ReviewRating"] = rating;
    }

    [When(@"I submit the review")]
    public void WhenISubmitTheReview()
    {
        Console.WriteLine("Submitting review");
        var product = _scenarioContext["CurrentProduct"] as string;
        var reviewText = _scenarioContext["ReviewText"] as string;
        
        if (product != null && reviewText != null)
        {
            if (!_productReviews.ContainsKey(product))
            {
                _productReviews[product] = new List<string>();
            }
            _productReviews[product].Add(reviewText);
        }
    }

    [Then(@"I should see ""(.*)"" message")]
    public void ThenIShouldSeeMessage(string message)
    {
        Console.WriteLine($"Verifying message: {message}");
        // In a real test, you would verify the message is displayed
    }

    [Then(@"my review should appear on the product page")]
    public void ThenMyReviewShouldAppearOnTheProductPage()
    {
        Console.WriteLine("Verifying review appears on product page");
        var product = _scenarioContext["CurrentProduct"] as string;
        var reviewText = _scenarioContext["ReviewText"] as string;
        
        if (product != null && reviewText != null)
        {
            if (!_productReviews.ContainsKey(product) || !_productReviews[product].Contains(reviewText))
            {
                throw new Exception("Review not found on product page");
            }
        }
    }

    [When(@"I navigate to my account page")]
    public void WhenINavigateToMyAccountPage()
    {
        Console.WriteLine("Navigating to account page");
        if (!_scenarioContext.ContainsKey("LoggedIn") || !(bool)_scenarioContext["LoggedIn"])
        {
            throw new Exception("User must be logged in to view account");
        }
        _scenarioContext["OnAccountPage"] = true;
    }

    [When(@"I click on ""(.*)""")]
    public void WhenIClickOn(string linkOrButton)
    {
        Console.WriteLine($"Clicking on: {linkOrButton}");
        _scenarioContext["LastClick"] = linkOrButton;
    }

    [When(@"I select review for product ""(.*)""")]
    public void WhenISelectReviewForProduct(string productName)
    {
        Console.WriteLine($"Selecting review for product: {productName}");
        _scenarioContext["SelectedReviewProduct"] = productName;
    }

    [When(@"I change review text to ""(.*)""")]
    public void WhenIChangeReviewTextTo(string newReviewText)
    {
        Console.WriteLine($"Changing review text to: {newReviewText}");
        _scenarioContext["ReviewText"] = newReviewText;
    }

    [When(@"I save the review")]
    public void WhenISaveTheReview()
    {
        Console.WriteLine("Saving review");
        var product = _scenarioContext["SelectedReviewProduct"] as string;
        var newReviewText = _scenarioContext["ReviewText"] as string;
        
        if (product != null && newReviewText != null && _productReviews.ContainsKey(product))
        {
            // Update the first review for this product (simplified)
            if (_productReviews[product].Count > 0)
            {
                _productReviews[product][0] = newReviewText;
            }
        }
    }

    [When(@"I see a review with inappropriate content")]
    public void WhenISeeAReviewWithInappropriateContent()
    {
        Console.WriteLine("Identifying inappropriate review");
        _scenarioContext["InappropriateReviewFound"] = true;
    }

    [When(@"I click ""(.*)"" link")]
    public void WhenIClickLink(string linkText)
    {
        Console.WriteLine($"Clicking link: {linkText}");
        _scenarioContext["LastLinkClicked"] = linkText;
    }

    [When(@"I select reason ""(.*)""")]
    public void WhenISelectReason(string reason)
    {
        Console.WriteLine($"Selecting reason: {reason}");
        _scenarioContext["ReportReason"] = reason;
    }

    [When(@"I submit the report")]
    public void WhenISubmitTheReport()
    {
        Console.WriteLine("Submitting report");
        _scenarioContext["ReportSubmitted"] = true;
    }

    [Then(@"the review should be flagged for moderation")]
    public void ThenTheReviewShouldBeFlaggedForModeration()
    {
        Console.WriteLine("Verifying review is flagged for moderation");
        if (!_scenarioContext.ContainsKey("ReportSubmitted") || 
            !(bool)_scenarioContext["ReportSubmitted"])
        {
            throw new Exception("Report was not submitted");
        }
    }
}

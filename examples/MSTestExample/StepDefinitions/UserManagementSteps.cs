using Reqnroll;
using Reqnroll.Assist;

namespace MSTestExample.StepDefinitions;

[Binding]
public class UserManagementSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly List<string> _userList = new List<string>();

    public UserManagementSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [When(@"I navigate to the user management section")]
    public void WhenINavigateToTheUserManagementSection()
    {
        Console.WriteLine("Navigating to user management section");
        if (!_scenarioContext.ContainsKey("LoggedIn") || !(bool)_scenarioContext["LoggedIn"])
        {
            throw new Exception("User must be logged in to access user management");
        }
        _scenarioContext["InUserManagement"] = true;
    }

    [When(@"I click ""(.*)"" button")]
    public void WhenIClickButton(string buttonName)
    {
        Console.WriteLine($"Clicking button: {buttonName}");
        _scenarioContext["LastButtonClicked"] = buttonName;
    }

    [When(@"I fill in the new user form:")]
    public void WhenIFillInTheNewUserForm(Table table)
    {
        Console.WriteLine("Filling in new user form:");
        var formData = new Dictionary<string, string>();
        foreach (var row in table.Rows)
        {
            var field = row["Field"];
            var value = row["Value"];
            formData[field] = value;
            Console.WriteLine($"  {field}: {value}");
        }
        _scenarioContext["FormData"] = formData;
    }

    [Then(@"I should see ""(.*)"" message")]
    public void ThenIShouldSeeMessage(string message)
    {
        Console.WriteLine($"Verifying message: {message}");
        // In a real test, you would verify the message is displayed
    }

    [Then(@"I should see ""(.*)"" in the user list")]
    public void ThenIShouldSeeInTheUserList(string username)
    {
        Console.WriteLine($"Verifying {username} is in the user list");
        
        // Simulate user being added to the list
        if (_scenarioContext.ContainsKey("FormData"))
        {
            var formData = _scenarioContext["FormData"] as Dictionary<string, string>;
            if (formData != null && formData.ContainsKey("Username"))
            {
                var createdUsername = formData["Username"];
                if (!_userList.Contains(createdUsername))
                {
                    _userList.Add(createdUsername);
                }
            }
        }
        
        if (!_userList.Contains(username))
        {
            throw new Exception($"User '{username}' was not found in the user list");
        }
    }

    [When(@"I select user ""(.*)"" from the list")]
    public void WhenISelectUserFromTheList(string username)
    {
        Console.WriteLine($"Selecting user: {username}");
        _scenarioContext["SelectedUser"] = username;
    }

    [When(@"I confirm the deletion")]
    public void WhenIConfirmTheDeletion()
    {
        Console.WriteLine("Confirming deletion");
        var selectedUser = _scenarioContext["SelectedUser"] as string;
        if (selectedUser != null && _userList.Contains(selectedUser))
        {
            _userList.Remove(selectedUser);
        }
    }

    [Then(@"I should not see ""(.*)"" in the user list")]
    public void ThenIShouldNotSeeInTheUserList(string username)
    {
        Console.WriteLine($"Verifying {username} is not in the user list");
        if (_userList.Contains(username))
        {
            throw new Exception($"User '{username}' should not be in the user list");
        }
    }

    [When(@"I change the role to ""(.*)""")]
    public void WhenIChangeTheRoleTo(string role)
    {
        Console.WriteLine($"Changing role to: {role}");
        var selectedUser = _scenarioContext["SelectedUser"] as string;
        if (selectedUser != null)
        {
            _scenarioContext[$"Role_{selectedUser}"] = role;
        }
    }

    [Then(@"I should see ""(.*)"" role for ""(.*)""")]
    public void ThenIShouldSeeRoleFor(string role, string username)
    {
        Console.WriteLine($"Verifying {username} has role: {role}");
        var actualRole = _scenarioContext.ContainsKey($"Role_{username}") 
            ? _scenarioContext[$"Role_{username}"] as string 
            : null;
            
        if (actualRole != role)
        {
            throw new Exception($"Expected role '{role}' for user '{username}', but found '{actualRole}'");
        }
    }
}

# language: nl
Functionaliteit: Authenticatie
    Als gebruiker
    Wil ik kunnen inloggen en uitloggen
    Zodat ik veilig toegang krijg tot de applicatie

Scenario: Inloggen met geldige inloggegevens
    Gegeven ik ben op de inlogpagina
    Als ik gebruikersnaam "john.doe@example.com" invoer
    En ik wachtwoord "SecurePassword123" invoer
    En ik op de inlogknop klik
    Dan zou ik naar het dashboard geleid moeten worden
    En zou ik "Welkom, John Doe" bericht moeten zien

Scenario: Uitloggen
    Gegeven ik ben ingelogd in het systeem
    Als ik op de uitlogknop klik
    Dan zou ik naar de inlogpagina geleid moeten worden
    En zou ik "U bent uitgelogd" bericht moeten zien

Scenario: Inloggen met ongeldige inloggegevens
    Gegeven ik ben op de inlogpagina
    Als ik gebruikersnaam "invalid@example.com" invoer
    En ik wachtwoord "VerkeerdeWachtwoord" invoer
    En ik op de inlogknop klik
    Dan zou ik "Ongeldige inloggegevens" foutmelding moeten zien
    En zou ik op de inlogpagina moeten blijven

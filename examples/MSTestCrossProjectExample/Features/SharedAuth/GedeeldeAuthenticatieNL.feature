# language: nl
Functionaliteit: Gedeelde Authenticatie
    Deze functionaliteit bevat herbruikbare authenticatiescenario's
    die aangeroepen kunnen worden vanuit andere projecten binnen de oplossing.
    
    Deze scenario's demonstreren scenario-hergebruik tussen projecten,
    waarmee teams authenticatielogica in een gedeelde bibliotheek kunnen onderhouden.

Scenario: Inloggen met geldige inloggegevens
    Gegeven ik ben op de inlogpagina
    Als ik gebruikersnaam "john.doe@example.com" invoer
    En ik wachtwoord "SecurePassword123" invoer
    En ik op de inlogknop klik
    Dan zou ik naar het dashboard geleid moeten worden
    En zou ik "Welkom, John Doe" bericht moeten zien

Scenario: Inloggen met ongeldige inloggegevens
    Gegeven ik ben op de inlogpagina
    Als ik gebruikersnaam "invalid@example.com" invoer
    En ik wachtwoord "VerkeerdeWachtwoord" invoer
    En ik op de inlogknop klik
    Dan zou ik "Ongeldige inloggegevens" foutmelding moeten zien
    En zou ik op de inlogpagina moeten blijven

Scenario: Uitloggen
    Gegeven ik ben ingelogd in het systeem
    Als ik op de uitlogknop klik
    Dan zou ik naar de inlogpagina geleid moeten worden
    En zou ik "U bent uitgelogd" bericht moeten zien

Scenario: Aanvraag wachtwoord opnieuw instellen
    Gegeven ik ben op de inlogpagina
    Als ik klik op de "Wachtwoord vergeten" link
    En ik e-mailadres "john.doe@example.com" invoer
    En ik klik op de "Reset aanvragen" knop
    Dan zou ik "E-mail voor wachtwoordherstel verzonden" bericht moeten zien

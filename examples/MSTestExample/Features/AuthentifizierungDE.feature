# language: de
Funktionalität: Authentifizierung
    Als Benutzer
    Möchte ich mich beim System authentifizieren
    Damit ich auf geschützte Ressourcen zugreifen kann

Szenario: Login mit gültigen Anmeldedaten
    Angenommen ich bin auf der Login-Seite
    Wenn ich Benutzername "john.doe@example.com" eingebe
    Und ich Passwort "SecurePassword123" eingebe
    Und ich auf den Login-Button klicke
    Dann sollte ich zum Dashboard weitergeleitet werden
    Und ich sollte "Willkommen, John Doe" Nachricht sehen

Szenario: Abmelden
    Angenommen ich bin im System eingeloggt
    Wenn ich auf den Abmelden-Button klicke
    Dann sollte ich zur Login-Seite weitergeleitet werden
    Und ich sollte "Sie wurden abgemeldet" Nachricht sehen

Szenario: Login mit ungültigen Anmeldedaten
    Angenommen ich bin auf der Login-Seite
    Wenn ich Benutzername "invalid@example.com" eingebe
    Und ich Passwort "FalschesPasswort" eingebe
    Und ich auf den Login-Button klicke
    Dann sollte ich "Ungültige Anmeldedaten" Fehlermeldung sehen
    Und ich sollte auf der Login-Seite bleiben

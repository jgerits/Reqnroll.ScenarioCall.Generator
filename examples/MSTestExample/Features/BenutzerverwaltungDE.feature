# language: de
Funktionalität: Benutzerverwaltung
    Als Administrator
    Möchte ich Benutzerkonten verwalten
    Damit ich den Systemzugriff kontrollieren kann

Szenario: Neues Benutzerkonto erstellen
    Angenommen I call scenario "Login mit gültigen Anmeldedaten" from feature "Authentifizierung"
    Wenn ich zum Benutzerverwaltungsbereich navigiere
    Und ich auf "Neuen Benutzer hinzufügen" klicke
    Und ich das Formular für neue Benutzer ausfülle:
        | Feld        | Wert                    |
        | Benutzername| jane.schmidt            |
        | E-Mail      | jane.schmidt@test.de    |
        | Rolle       | Standard Benutzer       |
    Und ich auf "Benutzer erstellen" klicke
    Dann sollte ich "Benutzer erfolgreich erstellt" sehen
    Und ich sollte "jane.schmidt" in der Benutzerliste sehen
    Und I call scenario "Abmelden" from feature "Authentifizierung"

Szenario: Benutzerkonto löschen
    Angenommen I call scenario "Login mit gültigen Anmeldedaten" from feature "Authentifizierung"
    Wenn ich zum Benutzerverwaltungsbereich navigiere
    Und ich wähle "jane.schmidt" aus der Liste
    Und ich klicke auf "Benutzer löschen"
    Und ich bestätige das Löschen
    Dann sollte ich "Benutzer erfolgreich gelöscht" sehen
    Und ich sollte "jane.schmidt" nicht in der Benutzerliste sehen
    Und I call scenario "Abmelden" from feature "Authentifizierung"

# Titel: WinDrop

## Beschreibung

**Kurzbeschreibung:** Ein C# MAUI Projekt
- AirDrop für Windows & Android.

## Idee

Das Programm ermöglicht das Senden von Dateien, Nachrichten und Bildern zwischen Geräten in einem Chat.

Die Verbindung und der Datenaustausch erfolgen über Bluetooth.

## Benutzeroberfläche

Die Benutzeroberfläche ist einfach gehalten und umfasst drei Hauptbereiche:
- Geräte über Bluetooth erkennen und verbinden
- Nachrichten und Bilder in einem Chat senden
- Dateien übertragen

### Hauptfenster

Das Hauptfenster zeigt eine Liste der verbundenen Geräte und einen Chatbereich, in dem Nachrichten und Bilder angezeigt werden. 
Es gibt auch Schaltflächen zum Senden von Nachrichten, Bildern und Dateien.

### Bildansicht

Ein separates Fenster ermöglicht das Zoomen und Verschieben von Bildern, die im Chat gesendet wurden.

## Systemanforderungen

- Windows 10
- Bluetooth
- .NET 6

## Libraries
- [InTheHand.Net.Bluetooth](https://www.nuget.org/packages/InTheHand.Net.Bluetooth/)

## Zukünftige Erweiterungen
- Nachrichten an einzelne Benutzer senden
- Unterstützung für weitere Betriebssysteme
- Verbesserte Benutzeroberfläche
- Verbesserte Programm stabilität



"Wodrauf sind sie besonders stolz?" 
- Ich bin besonders stolz darauf, dass ich es geschafft habe, daten über Bluetooth zu senden und empfangen ohne vorher mit bluetooth gearbeitet zu haben.
- Dass man Text & Bilder verschicken kann
- Ich bin auch stolz darauf, dass ich Wochenlanges debugging für fehler usw. überlebt habe.

"Wo gab es Probleme und wie wurden diese gelöst?"
- Probleme gab es bei der Übertragung von Dateien, da ich nicht wusste wie ich die Dateien in Byte umwandeln sollte und wie ich sie wieder zurück umwandeln sollte.
- Probleme gab es einige um text-nachrichten, bild-nachrichten und alle anderen datein zu unterscheiden können 
- Probleme gibt es aktuell bei der Datein übertragung diese funktioniert noch nicht. 
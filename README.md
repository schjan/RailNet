RailNet [![Build status](https://ci.appveyor.com/api/projects/status/rny0qetlrrqbmvfo/branch/master?svg=true)](https://ci.appveyor.com/project/schjan/railnet/branch/master) [![Build Status](https://travis-ci.org/schjan/RailNet.svg?branch=master)](https://travis-ci.org/schjan/RailNet) [![NuGet Version](https://img.shields.io/nuget/vpre/RailNet.Clients.Ecos.svg)](https://www.nuget.org/packages/RailNet.Clients.EcoS/)
=======

Modellbahnsteuerungslibrary für ESU ECoS mit .NET 4.5 __async/await__

Die RailNet Library soll helfen Programme für die eigene Modellbahn zu schreiben. Der _RailClient_ stellt Funktionen auf einem sehr hohen Level zur Verfügung und nimmt einem viele lästige Sachen wie zum Beispiel die komplette Netzwerkkommunikation ab, damit man sich alleine auf die Funktionalität des Programmes konzentrieren kann.

Sobald der _RailClient_ teilweise implementiert ist, werde ich einige Beispiele und ein Beispielprojekt in C# veröffentlichen.

Hilfe ist übrigens auch immer gerne gesehen! ;)

Die Library ist [Mono](http://www.mono-project.com/Main_Page) kompaktibel und kann somit auch unter Linux und OSX eingesetzt werden!

### NuGet
RailNet.Clients.EcoS ist über NuGet verfügbar über "[Install-Package RailNet.Clients.Ecos](https://www.nuget.org/packages/RailNet.Clients.EcoS/)"

### Wiki / Anleitung
Im [Wiki](https://github.com/schjan/RailNet/wiki) werden alle verfügbaren Befehle erklärt.

## RailNet.Clients.Ecos
Client für [ESU ECoS](http://www.esu.eu/produkte/digitale-steuerung/ecos-50200-zentrale/was-ecos-kann/)

### Verbinden mit ECoS
```csharp
var client = new EcosClient();
await client.ConnectAsync("Hostname");
```

### BasicClient
Client für standard ECoS Befehlssatz via TCP. Zum jetzigen Zeitpunkt werden alle Messagetypen unterstützt. Die Funktionen sind am original Befehlssatz angelehnt und haben keine erweiterte Funktionalität. Antworten werden ohne Auswertung (außer auf Fehler) an den Client weitergegeben.

#### Befehle
Einfache Befehle, wie in der [ESU Netzwerkspezifikation](http://wiki.rocrail.net/doku.php?id=ecos-de) angegeben.

```csharp
client.BasicClient // is your friend

// Sendet SET Befehl an Controller 11 (SchaltartikelManager) für die angegebene Adresse 'addr'
// und der bool 'red' bestimmt, ob die Adresse auf Rot oder Grün gestellt werden soll.
// Außerdem ist DCC als Protokoll definiert.
BasicResponse result = await client.BasicClient.Set(11, "switch", "DCC" + addr + (red ? "r" : "g"));
```

Befehle die über BasicClient gesendet werden geben alle ein Objekt vom Typ [_BasicResponse_](https://github.com/schjan/RailNet/blob/master/src/RailNet.Clients.Ecos/Basic/BasicResponse.cs) zurück. BasicResponse enthält die Antwort der ECoS, auch wie nach Netzwerkspezifikation definiert, im _string[] Content_ und außerdem allgemeine Fehlerinformationen und den Befehl.

##### Unterstützte Befehle:
Aktuell werden alle Befehle der neuesten ECoS Netzwerkspezifikation unterstützt.

* QueryObjects
* QueryObjectsSize _kurzschreibweise für queryObjects(id, size)_
* Set
* Get
* Create
* Delete
* Request
* Release

#### Events
Serverevents, wie zum Beispiel der Nothalt oder Rückmeldekontakte werden von der ECoS als Events an den Client gesendet. Mit dem Befehl _Request_ kann man Events eines Objektes abbonieren und mit _Release_ abbestellen.
Da Events eher komplex sind, bietet es sich an die Implementationen des erweiterten Clients zu benutzen.
```csharp
await client.Request(5, "view");

BasicClient.EventReceived += ...
//oder
BasicClient.EventObservable.Subscribe(...);

await client.Release(5, "view");
```
### Extended
Es ist angedacht, aufbauend auf die einfachen Befehle, die sich stark an dem eigentlichen Netzwerkprotokoll orientieren, einen weiteren Befehlssatz aufzubauen um zum Beispiel Loks und Signale direkt als Objekte zu Verfügung zu haben.
#### Ecos Manager
Mit dem ECoS Manager Objekt kann man direkt auf Funktionen der ECoS zugreifen (z.B. Go- und Stoptaste)
```csharp
client.Ecos.Start();
client.Ecos.Stop(); //Nothalt
```

#### Rückmelde Manager
Über das RückmeldeManager Objekt ist es möglich direkt auf Rückmelder des _S88 Busses_ oder _ECoS Detectoren_ zuzugreifen. Am einfachsten ist es dort, mit der Funktion __SubscribeAll()__ alle Rückmeldemodule der ECoS zu abbonieren.
```csharp
client.Rueckmelder.SubscribeAll(); //abboniert alle Rückmelder
client.Rueckmelder.Module[100].Rueckmelder[0].Belegt //zugriff auf Belegtinformationen
```

#### Schaltartikel Manager
Über den Schaltartikel ist es bis jetzt lediglich möglich vereinfacht Zustände einer Magnetartikeladresse zu setzen.
```csharp
client.Schaltartikel.SetzeAdresse(1, 1, Digitalsystem.DCC); //setzt Adresse 1 auf Grün und benutzt DCC Schaltbefehle.
```

## RailNet.Core
Angedacht als gemeinsame Library um neben der ECoS auch andere Modellbahnzentralen zu unterstützen.
###IRailClient
__Nicht implementiert__

## Beispiel
Das Projekt [__RailNet.Signalsteuerung.WF__](https://github.com/schjan/RailNet/tree/master/src/Samples/RailNet.Signalsteuerung.WF) ist eine Windows Forms Anwendung, die an einem sehr einfachen Beispiel zeigt wie man sich mit einer ECoS verbinden kann und ein Signal stellen kann. Als Signal wird hier ein Viessmann Ks-Signal an einem Multiplexer genommen. Statt diesem Signal kann man jedoch auch jedes andere Signal oder Weiche nehmen. Lediglich die beschriebenen Signalbegriffe sind dann anders.

## Lizenz

Beim bearbeiten oder kopieren dieses Repositories ist, neben der hier verwendeten Lizenz ([Apache 2.0](LICENSE)), unbedingt auf die Lizenzbestimmungen von ESU (Besonders auf Punkt 1.3) zu achten.
>__1.3. Verbotene Nutzung__


>Es ist ausdrücklich verboten, das Protokoll in einem Server zu implementieren. Insbesondere fällt
hierunter die Verwendung des Protokolls in einem zur Steuerung von Modellspielwaren geeigneten
Gerät.
Dem Lizenznehmer ist es zudem untersagt, auf Grundlage des Protokolls weitere Kommunikationsverfahren
zu entwickeln. Dies umfasst ausdrücklich auch die Modifikation des bestehenden Protokolls.
Zudem untersagt ist die Implementierung des Protokolls in Software, die auf anderen als PC-Systemen
lauffähig ist. - _Auszug aus den ESU Lizenzbestimmungen Protokoll Version 0.2, Dritte Auflage, Juni 2011_

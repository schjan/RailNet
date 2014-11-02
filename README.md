RailNet [![Build status](https://ci.appveyor.com/api/projects/status/rny0qetlrrqbmvfo/branch/master?svg=true)](https://ci.appveyor.com/project/schjan/railnet/branch/master) [![Build Status](https://travis-ci.org/schjan/RailNet.svg?branch=master)](https://travis-ci.org/schjan/RailNet)
=======

Modellbahnsteuerungslibrary für ESU ECoS mit .NET 4.5 __async/await__

Die RailNet Library soll helfen Programme für die eigene Modellbahn zu schreiben. Der _RailClient_ stellt Funktionen auf einem sehr hohen Level zur Verfügung und nimmt einem viele lästige Sachen wie zum Beispiel die komplette Netzwerkkommunikation ab, damit man sich alleine auf die Funktionalität des Programmes konzentrieren kann.

Sobald der _RailClient_ teilweise implementiert ist, werde ich einige Beispiele und ein Beispielprojekt in C# veröffentlichen.

Hilfe ist übrigens auch immer gerne gesehen! ;)

Die Library ist [Mono](http://www.mono-project.com/Main_Page) kompaktibel und kann somit auch unter Linux und OSX eingesetzt werden!

###NuGet
RailNet.Clients.EcoS ist über NuGet verfügbar über "[Install-Package RailNet.Clients.Ecos](https://www.nuget.org/packages/RailNet.Clients.EcoS/)"

###Wiki / Anleitung
Im [Wiki](https://github.com/schjan/RailNet/wiki) werden alle verfügbaren Befehle erklärt.

##RailNet.Clients.Ecos
Client für [ESU ECoS](http://www.esu.eu/produkte/digitale-steuerung/ecos-50200-zentrale/was-ecos-kann/)
###BasicClient
Client für standart ECoS Befehlssatz via TCP. Zum jetzigen Zeitpunkt werden alle Messagetypen unterstützt. Die Funktionen sind am original Befehlssatz angelehnt und haben keine erweiterte Funktionalität. Antworten werden ohne Auswertung (außer auf Fehler) an den Client weitergegeben.
###RailClient
__Nicht implementiert__

Soll erweiterte Funktionalität bereitstellen wie z.B. LokObjekte, LokListen etc.

##RailNet.Core
Angedacht als gemeinsame Library um neben der ECoS auch andere Modellbahnzentralen zu unterstützen.
###IRailClient
__Nicht implementiert__

Siehe RailClient. Nur das gleiche als Interface für erweiterbarkeit.

##Lizenz

Beim bearbeiten oder kopieren dieses Repositories ist, neben der hier verwendeten Lizenz ([Apache 2.0](LICENSE)), unbedingt auf die Lizenzbestimmungen von ESU (Besonders auf Punkt 1.3) zu achten.
>__1.3. Verbotene Nutzung__


>Es ist ausdrücklich verboten, das Protokoll in einem Server zu implementieren. Insbesondere fällt
hierunter die Verwendung des Protokolls in einem zur Steuerung von Modellspielwaren geeigneten
Gerät.
Dem Lizenznehmer ist es zudem untersagt, auf Grundlage des Protokolls weitere Kommunikationsverfahren
zu entwickeln. Dies umfasst ausdrücklich auch die Modifikation des bestehenden Protokolls.
Zudem untersagt ist die Implementierung des Protokolls in Software, die auf anderen als PC-Systemen
lauffähig ist. - _Auszug aus den ESU Lizenzbestimmungen Protokoll Version 0.2, Dritte Auflage, Juni 2011_
